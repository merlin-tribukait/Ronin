using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Ronin.Data;
using Ronin.Network;
using Ronin.Network.Cryptography.Rpg_club;
using Ronin.Network.Cryptography.SmartGuard;
using SharpDisasm;

namespace Ronin.Utilities
{
    enum Kernel
    {
        Windows7,
        Windows8_0,
        Windows8_1,
        Windows10
    }

    public class Injector
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        public static bool ShouldInjectInNextClient = true;
        public int ProcessId;
        public int HostPort;
        private L2Bot botInstance;

        private GreyMagic.ExternalProcessMemory exmemory;
        private long _origBytesPreHook;
        private long _hookJumpToSize;

        public static bool BadStuff = false;

        public Injector(int pid, int hostPort)
        {
            ProcessId = pid;
            HostPort = hostPort;
            bool pleaseTryAgain = false;

            try
            {
                l2 = Process.GetProcessById(ProcessId);
            }
            catch (Exception e)
            {
                pleaseTryAgain = true;
            }

            if(pleaseTryAgain)
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (process.Id == ProcessId)
                        l2 = process;
                }
            }
            catch (Exception e)
            {
                //log.Debug(e);
                //throw;
            }
        }

        private static Kernel _kernel
        {
            get
            {
                Kernel var = Kernel.Windows7;
                OperatingSystem os = Environment.OSVersion;
                //Get version information about the os.
                Version vs = os.Version;
                if (os.Platform == PlatformID.Win32NT)
                {
                    switch (vs.Major)
                    {
                        case 6:
                            switch (vs.Minor)
                            {
                                case 2:
                                    var = Kernel.Windows8_0;
                                    break;
                                case 3:
                                    var = Kernel.Windows8_1;
                                    break;
                            }
                            break;
                        case 10:
                            var = Kernel.Windows10;
                            break;
                    }
                }

                if (IsWindows10())
                    var = Kernel.Windows10;

                return var;
            }
        }

        public static bool IsWindows10()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            string productName = (string) reg.GetValue("ProductName");

            return productName.StartsWith("Windows 10");
        }

        private static string[] badboys =
        {
            "dotpeek", "ollydbg", "debugger", "cheatengine", "immunity",
            "x32_dbg", "x64_dbg", "reflector", "ilspy", "spyxx", "wireshark", "dumpcap", "procexp",
            "dnspy"
        };

        private static string[] whiteboys =
        {
            "scr"
        };

        private static string[] badboysClassNames =
        {
            "OLLYDBG","ID","PROCEXPL","WinDbgFrameClass", "PROCMON_WINDOW_CLASS"
        };

        private static string[] badboysCaptions =
        {
            "Microsoft Spy++", "x32_dbg", "Cheat Engine 6.4", "Cheat Engine 6.5", "Cheat Engine 6.6",
            "Cheat Engine 6.1", "Cheat Engine 6.2", "Cheat Engine 6.3", "Immunity Debugger - [CPU]",
            "dnSpy 3.0.1 (x86)"
        };

        private Thread _dbgCheckForClient;

        [STAThread]
        public static void WindowsPatcher()
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                return;
#endif
            
            var injectedProcesses = new List<int>();
            Kernel kernel = _kernel;

            foreach (var process in Process.GetProcesses())
            {
                injectedProcesses.Add(process.Id);
                try
                {
                    bool procKilled = false;
                    try
                    {
                        foreach (var badboy in badboys)
                        {
                            if (process.ProcessName.ToLower().Contains(badboy))
                            {
                                process.Kill();
                                procKilled = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    foreach (var whiteboy in whiteboys)
                    {
                        if (process.ProcessName.Equals(whiteboy))
                        {
                            procKilled = true;
                        }
                    }

                    var injector = new Injector(process.Id, 666);
                    if (procKilled || kernel != Kernel.Windows7 || !Environment.Is64BitOperatingSystem) continue;
                    //run in STAThread
                    Thread thread = new Thread(() => {
                        injector.PatchWindowsAPI();
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                catch (Exception e)
                {
                    log.Debug(e.ToString());
                    throw;
                }

            }

            long count = 0;
            var roninHwnd = Process.GetCurrentProcess().Handle;

            int dbgport = 0, retnSize;

            var procs = Process.GetProcesses();

            while (true)
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (!injectedProcesses.Contains(process.Id))
                    {
                        injectedProcesses.Add(process.Id);
                        //log.Debug("new instance "+ process.Id);
                        try
                        {
                            bool procKilled = false;
                            try
                            {
                                foreach (var badboy in badboys)
                                {
                                    if (process.ProcessName.ToLower().Contains(badboy))
                                    {
                                        process.Kill();
                                        procKilled = true;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }

                            foreach (var whiteboy in whiteboys)
                            {
                                if (process.ProcessName.Equals(whiteboy))
                                {
                                    procKilled = true;
                                }
                            }

                            if (procKilled || kernel != Kernel.Windows7 || !Environment.Is64BitOperatingSystem) continue;

                            var injector = new Injector(process.Id, 666);
                            //run in STAThread
                            Thread thread = new Thread(() =>
                            {
                                if (kernel == Kernel.Windows7)
                                    injector.PatchWindowsAPI();
                            });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }
                    }
                }

                NtQueryInformationProcess(roninHwnd, 0x7, ref dbgport, 4, out retnSize);
#if !DEBUG
                if (dbgport != 0 || System.Diagnostics.Debugger.IsAttached)
                {
                    BadStuff = true;
                    ShouldInjectInNextClient = false;
                    log.Debug("Memory access violation.");
                    Environment.Exit(0);
                }
#endif

                Thread.Sleep(200);

                count++;
                if (count % 5 == 0)
                {
                    foreach (var badboysCaption in badboysCaptions)
                    {
                        var hwnd = FindWindow(null, badboysCaption);
                        if (hwnd != IntPtr.Zero)
                        {
                            uint pid;
                            GetWindowThreadProcessId(hwnd, out pid);

                            try
                            {
                                var proc = Process.GetProcessById((int)pid);
                                proc.Kill();
                            }
                            catch (Exception)
                            {

                            }
                        }
                        CloseHandle(hwnd);
                    }

                    foreach (var badboysCaption in badboysClassNames)
                    {
                        var hwnd = FindWindow(badboysCaption, null);
                        if (hwnd != IntPtr.Zero)
                        {
                            uint pid;
                            GetWindowThreadProcessId(hwnd, out pid);

                            try
                            {
                                var proc = Process.GetProcessById((int)pid);
                                proc.Kill();
                            }
                            catch (Exception)
                            {

                            }
                        }
                        CloseHandle(hwnd);
                    }
                }
            }
        }

        [STAThread]
        public static void RerouteL2s()
        {
            SGKey.Init();
            RpgInGameCipher.Init();
            var injectedL2Processes = new List<int>();
            log.Info("Waiting for a game client to be launched...");

            MainWindow.pSelf.Closed += (sender, args) =>
            {
                ShouldInjectInNextClient = false;
            };

            object injectLock = new object();
            List<Process> l2s = new List<Process>();

            while (ShouldInjectInNextClient)
            {
                l2s.Clear();

                foreach (var process in Process.GetProcesses())
                {
                    if (process.ProcessName.Equals("l2", StringComparison.OrdinalIgnoreCase) ||
                        process.ProcessName.Equals("l2.bin", StringComparison.OrdinalIgnoreCase))
                    {
                        l2s.Add(process);
                        //log.Debug(process.PrivateMemorySize64);
                    }
                }

                foreach (var l2 in l2s)
                {
                    if (!l2.HasExited && injectedL2Processes.Contains(l2.Id) == false && (DateTime.Now - l2.StartTime).Seconds >= 0 &&
                        (DateTime.Now - l2.StartTime).TotalSeconds < 5)
                    {
                        if (ShouldInjectInNextClient && !l2.HasExited)
                        {
                            injectedL2Processes.Add(l2.Id);
                            try
                            {
                                log.Debug("Detected game client.");
                                try
                                {
                                    //run in STAThread
                                    Thread thread = new Thread(() =>
                                    {
                                        try
                                        {
                                            var injector = new Injector(l2.Id, PortChecker.GetOpenPort());
                                            //if (_kernel == Kernel.Windows7)
                                            //    injector.RerouteL2Win7(
                                            //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            //else if (_kernel == Kernel.Windows8_1)
                                            //    injector.RerouteL2Win8_1(
                                            //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            //if (_kernel == Kernel.Windows8_0)
                                            //    injector.RerouteL2Win8_0(
                                            //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            //else if (_kernel == Kernel.Windows10)
                                            //    injector.RerouteL2Win10(
                                            //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            //injector.RerouteL2Auto(Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            ////else


                                            injector.RerouteL2Auto(
                                                (5000 - (DateTime.Now - l2.StartTime).Milliseconds) < 0
                                                    ? 0
                                                    : (5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                                            //injector.RerouteL2Win7(
                                            //    Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));

                                            //injector.RerouteL2Auto(0);
                                        }
                                        catch (Exception e)
                                        {
                                            log.Debug(e);
                                        }
                                    });
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                }
                                catch (Exception e)
                                {
                                    log.Debug(e.ToString());
                                    throw;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Debug(e.ToString());
                                throw;
                            }

                            Thread.Sleep(100);
                            //ShouldInjectInNextClient = false;
                        }
                    }
                }

                //var l2s = Process.GetProcessesByName("L2.bin");
                //foreach (var l2 in l2s)
                //{
                //    if (injectedL2Processes.Contains(l2.Id) == false && (DateTime.Now - l2.StartTime).TotalSeconds < 10)
                //    {
                //        injectedL2Processes.Add(l2.Id);
                //        if (ShouldInjectInNextClient)
                //        {
                //            try
                //            {
                //                log.Debug("Detected game client.");
                //                var injector = new Injector(l2.Id, PortChecker.GetOpenPort());
                //                try
                //                {
                //                    //run in STAThread
                //                    Thread thread = new Thread(() =>
                //                    {
                //                        //if (_kernel == Kernel.Windows7)
                //                        //    injector.RerouteL2Win7(
                //                        //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                        //else if (_kernel == Kernel.Windows8_1)
                //                        //    injector.RerouteL2Win8_1(
                //                        //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                        //if (_kernel == Kernel.Windows8_0)
                //                        //    injector.RerouteL2Win8_0(
                //                        //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                        //else if (_kernel == Kernel.Windows10)
                //                        //    injector.RerouteL2Win10(
                //                        //        Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                        //injector.RerouteL2Auto(Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                        //else
                //                        injector.RerouteL2Auto(
                //                            Math.Abs(5000 - (DateTime.Now - l2.StartTime).Milliseconds));
                //                    });
                //                    thread.SetApartmentState(ApartmentState.STA);
                //                    thread.Start();
                //                }
                //                catch (Exception e)
                //                {
                //                    log.Debug(e.ToString());
                //                    throw;
                //                }
                //            }
                //            catch (Exception e)
                //            {
                //                log.Debug(e.ToString());
                //                throw;
                //            }

                //            //ShouldInjectInNextClient = false;
                //        }
                //    }
                //}

                Thread.Sleep(1000);
            }
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref int debugport, 
            int processInformationLength, out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
            int dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(IntPtr hWnd, out bool res);

        [DllImport("USER32.DLL")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static object _botAddSyncLock = new object();

        public void BringWindowToForeground()
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = l2.MainWindowHandle;
            }
            catch (Exception e)
            {
                log.Debug(e);
                return;
            }

            //if (IsIconic(handle))
                ShowWindow(handle, 1);

            SetForegroundWindow(handle);
        }

        private IntPtr _alocatedMem;
        private int _hookOffset = 0;
        private int _doorSize;

        public void RerouteL2Win7(double wait)
        {
            Thread.Sleep(500);
            log.Info($"Injecting at PID: {ProcessId}");
            Thread.Sleep((int) wait);
            //PatchFindWindows();
            int pid = ProcessId;
            byte[] sockAddr = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0}; //for game proxy
            sockAddr[2] = (byte) (HostPort >> 8);
            sockAddr[3] = (byte) (HostPort & 0xff);
            byte[] sockAddr2 = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0};
            //socket for interprocess communication (ip transfer)
            int openPort = PortChecker.GetOpenPort();
            sockAddr2[2] = (byte) (openPort >> 8);
            sockAddr2[3] = (byte) (openPort & 0xff);


            for (int i = 0; i < 4; i++)
            {
                sockAddr[4 + i] = byte.Parse("127.0.0.1".Split('.')[i]);
            }

            byte[] stub =
            {
                //0x83, 0xEC, 0x18, //sub esp,24
                //0x53, //push ebx
                //0x57, //push edi
                0x64, 0xA1, 0x18, 0x0, 0x0, 0x0, //mov eax, dword ptr fs:[0x18]
                0x81, 0x7C, 0x24, 0x1c, 0x0, 0x0, 0x00, 0x0, //cmp [esp+18/1c],ws2_32.connect+18
                0x74, 0xc, //je patch
                0x81, 0x7C, 0x24, 0x18, 0x0, 0x0, 0x00, 0x0, //cmp [esp+18/1c],ws2_32.connect+18
                0x74, 0x69, //je patch2
                0xEB, 0x64, //jmp end
                0x60, //pushad (preserve registers)
                0x8B, 0x45, 0x50, //mov eax,[ebp+4c/50](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x45, 0x50, 0x00, 0x00, 0x00, 0x00, //mov [ebp+4c/50], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x90, 0x90, 0x90, 0x90, 0x90,
                0xc3, //ret

                //@patch2
                0x60, //pushad (preserve registers)
                0x8B, 0x45, 0x4c, //mov eax,[ebp+4c/50](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x45, 0x4c, 0x00, 0x00, 0x00, 0x00, //mov [ebp+4c/50], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x90, 0x90, 0x90, 0x90, 0x90,
                0xc3 //ret
            };

            //Process l2;

            //try
            //{
            //    l2 = Process.GetProcessById(pid);
            //}
            //catch (Exception e)
            //{
            //    log.Debug(e);
            //    return;
            //}

            //different parameter offsets in SG clients
            //if (!IsSGLoaded())
            //{
            //    stub[9] -= 4;
            //    stub[19] -= 4;
            //    stub[32] -= 4;
            //}


            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            IntPtr pTlsGetValue = IntPtr.Zero;
            try
            {
                pTlsGetValue = exmemory.GetProcAddress("KERNELBASE", "TlsGetValue");
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsSGLoaded())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!SGKey.OffsetList.ContainsKey(md5hash.ToString()))
                            return;

                        //log.Debug("z");
                    }
                }
            }

            int counter = 0;
            while (counter < 30 && pTlsGetValue.ToInt32() == 0)
            {
                Thread.Sleep(100);
                exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
                try
                {
                    pTlsGetValue = exmemory.GetProcAddress("KERNELBASE", "TlsGetValue");
                }
                catch (Exception)
                {
                    // ignored
                }

                counter++;
            }

            //var pOpenClipboard = exmemory.GetProcAddress("user32", "OpenClipboard");
            //var pEmptyClipboard = exmemory.GetProcAddress("user32", "EmptyClipboard");
            //var pCloseClipboard = exmemory.GetProcAddress("user32", "CloseClipboard");
            //var pSetClipboardData = exmemory.GetProcAddress("user32", "SetClipboardData");

            //var pGlobalLock = exmemory.GetProcAddress("kernel32", "GlobalLock");
            //var pGlobalUnlock = exmemory.GetProcAddress("kernel32", "GlobalUnlock");
            //var pGlobalAlloc = exmemory.GetProcAddress("kernel32", "GlobalAlloc");

            var plisten = exmemory.GetProcAddress("WS2_32", "listen");
            var pbind = exmemory.GetProcAddress("WS2_32", "bind");
            var paccept = exmemory.GetProcAddress("WS2_32", "accept");
            var psend = exmemory.GetProcAddress("WS2_32", "send");
            var psocket = exmemory.GetProcAddress("WS2_32", "socket");

            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");



            if (pTlsGetValue.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful injection");
            }

            uint dummy;
            var res = VirtualProtectEx(exmemory.ProcessHandle, pTlsGetValue, 10, 0x0020, out dummy);
            _origBytesPreHook = exmemory.Read<long>(pTlsGetValue + 2);

            _alocatedMem = exmemory.AllocateMemory(1000);

            if (_alocatedMem.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful memory allocation.");
            }

            int writtenBytes = exmemory.WriteBytes(_alocatedMem, sockAddr);

            exmemory.WriteBytes(_alocatedMem + 0x200, sockAddr2);

            if (writtenBytes == 0)
            {
                throw new Exception("Unsuccessful memory patch.");
            }

            //calculate connect+18
            long connectOffset = pconnect.ToInt32() + 0x18;

            stub[10] = (byte) (connectOffset);
            stub[11] = (byte) (connectOffset >> 8);
            stub[12] = (byte) (connectOffset >> 0x10);
            stub[13] = (byte) (connectOffset >> 0x18);

            stub[20] = (byte) (connectOffset);
            stub[21] = (byte) (connectOffset >> 8);
            stub[22] = (byte) (connectOffset >> 0x10);
            stub[23] = (byte) (connectOffset >> 0x18);


            stub[45] = (byte) (_alocatedMem.ToInt32());
            stub[46] = (byte) (_alocatedMem.ToInt32() >> 8);
            stub[47] = (byte) (_alocatedMem.ToInt32() >> 0x10);
            stub[48] = (byte) (_alocatedMem.ToInt32() >> 0x18);

            long callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                                 psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 60);

            stub[56] = (byte) (callToSocket);
            stub[57] = (byte) (callToSocket >> 8);
            stub[58] = (byte) (callToSocket >> 0x10);
            stub[59] = (byte) (callToSocket >> 0x18);

            stub[64] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[65] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[66] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[67] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[71] = (byte) ((_alocatedMem.ToInt32() + 0x200));
            stub[72] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[73] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[74] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            long callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                               pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 81);

            stub[77] = (byte) (callToBind);
            stub[78] = (byte) (callToBind >> 8);
            stub[79] = (byte) (callToBind >> 0x10);
            stub[80] = (byte) (callToBind >> 0x18);

            stub[85] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[86] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[87] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[88] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                                 plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 94);

            stub[90] = (byte) (callToListen);
            stub[91] = (byte) (callToListen >> 8);
            stub[92] = (byte) (callToListen >> 0x10);
            stub[93] = (byte) (callToListen >> 0x18);

            stub[97] = (byte) (_alocatedMem.ToInt32() + 200);
            stub[98] = (byte) ((_alocatedMem.ToInt32() + 200) >> 8);
            stub[99] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[100] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[103] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[104] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[105] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[106] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                                 paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 112);

            stub[108] = (byte) (callToAccept);
            stub[109] = (byte) (callToAccept >> 8);
            stub[110] = (byte) (callToAccept >> 0x10);
            stub[111] = (byte) (callToAccept >> 0x18);



            long callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                               psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 123);

            stub[119] = (byte) (callToSend);
            stub[120] = (byte) (callToSend >> 8);
            stub[121] = (byte) (callToSend >> 0x10);
            stub[122] = (byte) (callToSend >> 0x18);


            int patchSize = 103;
            //@patch2

            stub[45 + patchSize] = (byte) (_alocatedMem.ToInt32());
            stub[46 + patchSize] = (byte) (_alocatedMem.ToInt32() >> 8);
            stub[47 + patchSize] = (byte) (_alocatedMem.ToInt32() >> 0x10);
            stub[48 + patchSize] = (byte) (_alocatedMem.ToInt32() >> 0x18);

            callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                            psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 60 + patchSize);

            stub[56 + patchSize] = (byte) (callToSocket);
            stub[57 + patchSize] = (byte) (callToSocket >> 8);
            stub[58 + patchSize] = (byte) (callToSocket >> 0x10);
            stub[59 + patchSize] = (byte) (callToSocket >> 0x18);

            stub[64 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[65 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[66 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[67 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[71 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 0x200));
            stub[72 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[73 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[74 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                          pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 81 + patchSize);

            stub[77 + patchSize] = (byte) (callToBind);
            stub[78 + patchSize] = (byte) (callToBind >> 8);
            stub[79 + patchSize] = (byte) (callToBind >> 0x10);
            stub[80 + patchSize] = (byte) (callToBind >> 0x18);

            stub[85 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[86 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[87 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[88 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                            plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 94 + patchSize);

            stub[90 + patchSize] = (byte) (callToListen);
            stub[91 + patchSize] = (byte) (callToListen >> 8);
            stub[92 + patchSize] = (byte) (callToListen >> 0x10);
            stub[93 + patchSize] = (byte) (callToListen >> 0x18);

            stub[97 + patchSize] = (byte) (_alocatedMem.ToInt32() + 200);
            stub[98 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 200) >> 8);
            stub[99 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[100 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[103 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[104 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[105 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[106 + patchSize] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                            paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 112 + patchSize);

            stub[108 + patchSize] = (byte) (callToAccept);
            stub[109 + patchSize] = (byte) (callToAccept >> 8);
            stub[110 + patchSize] = (byte) (callToAccept >> 0x10);
            stub[111 + patchSize] = (byte) (callToAccept >> 0x18);



            callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                          psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 123 + patchSize);

            stub[119 + patchSize] = (byte) (callToSend);
            stub[120 + patchSize] = (byte) (callToSend >> 8);
            stub[121 + patchSize] = (byte) (callToSend >> 0x10);
            stub[122 + patchSize] = (byte) (callToSend >> 0x18);


            //log.Debug("-1");
            var result = exmemory.WriteBytes(_alocatedMem + 20, stub);
            //log.Debug("0");
            _hookJumpToSize = 0;
            if (_alocatedMem.ToInt32() < pTlsGetValue.ToInt32())
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20 + 0x100000000) - pTlsGetValue.ToInt32() - 10;
            }
            else
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20) - pTlsGetValue.ToInt32() - 10;
            }

            //log.Debug("1");
            InstallHookClient();

            //log.Debug("2");
            botInstance = new L2Bot(this);
            MainWindow.ViewModel.Bots.Add(botInstance);
            if (MainWindow.ViewModel.SelectedBot == null)
                MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();

            exmemory.ProcessExited += (sender, args) =>
            {
                botInstance?.Engine?.Abort();

                if (MainWindow.ViewModel.Bots.Contains(botInstance))
                    MainWindow.ViewModel.Bots.Remove(botInstance);

                if (MainWindow.ViewModel.SelectedBot == null && MainWindow.ViewModel.Bots.Any())
                    MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();
            };

            //log.Debug("3");
            //wait for ip transfer and remove the hook
            string remoteAddr = String.Empty;
            TcpListener server = null;
            try
            {
                Int32 port = openPort;
                TcpClient client;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(100);
                        client = new TcpClient("127.0.0.1", port);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                NetworkStream stream = client.GetStream();

                // Buffer to store the response bytes.
                var data = new Byte[8];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);

                int remotePort = (data[3] | (data[2] << 8));
                string remoteIp = data[4] + "." + data[5] + "." + data[6] + "." + data[7];
                remoteAddr = remoteIp + ":" + remotePort;

                //log.Debug(string.Format("Received: {0}", string.Join(", ", data)));

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                log.Debug($"An exception occured during tranfer: {e}");
                MainWindow.ViewModel.Bots.Remove(botInstance);
                return;
            }

            //RestoreHookedBytes();

            var authServer = new Server
            {
                IsAuthServer = true,
                LocalAddress = "127.0.0.1", //localhost
                LocalPort = HostPort,
                RemoteAddress = remoteAddr.Split(':')[0], //l2authd.lineage2.com
                RemotePort = int.Parse(remoteAddr.Split(':')[1]),
                L2Injector = this,
                BotInstance = botInstance
            };


            authServer.Start();
            //log.Debug($"Started proxying to {remoteAddr} .");
        }

        public void RerouteL2Win10(double wait)
        {
            Thread.Sleep(500);
            log.Info($"Injecting at PID: {ProcessId}");
            Thread.Sleep((int) wait);
            //PatchFindWindows();
            int pid = ProcessId;
            byte[] sockAddr = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0}; //for game proxy
            sockAddr[2] = (byte) (HostPort >> 8);
            sockAddr[3] = (byte) (HostPort & 0xff);
            byte[] sockAddr2 = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0};
            //socket for interprocess communication (ip transfer)
            int openPort = PortChecker.GetOpenPort();
            sockAddr2[2] = (byte) (openPort >> 8);
            sockAddr2[3] = (byte) (openPort & 0xff);


            for (int i = 0; i < 4; i++)
            {
                sockAddr[4 + i] = byte.Parse("127.0.0.1".Split('.')[i]);
            }

            byte[] stub =
            {
                0x60, //pushad (preserve registers)
                0x8B, 0x44, 0x24, 0x5c, //mov eax,[esp+0x5C](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x44, 0x24, 0x5C, 0x00, 0x00, 0x00, 0x00, //mov [esp+0x5C], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x90, 0x90,
                0xFF, 0x34, 0x24, //push [esp] (junk to be cleared after return, filling gap)
                0x81, 0xFE, 0x0, 0x00, 0x0, 0x0, //cmp esi,WSARecv+360
                0xc3, //ret
            };

            Process l2;

            try
            {
                l2 = Process.GetProcessById(pid);
            }
            catch (Exception e)
            {
                log.Debug(e);
                return;
            }

            //different parameter offsets in SG clients
            //if (!IsSGLoaded())
            //{
            //    stub[9] -= 4;
            //    stub[19] -= 4;
            //    stub[32] -= 4;
            //}


            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            IntPtr pconnect = IntPtr.Zero;
            try
            {
                pconnect = exmemory.GetProcAddress("WS2_32", "connect");
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsSGLoaded())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!SGKey.OffsetList.ContainsKey(md5hash.ToString()))
                            return;

                        //log.Debug("z");
                    }
                }
            }

            int counter = 0;
            while (counter < 30 && pconnect.ToInt32() == 0)
            {
                Thread.Sleep(100);
                exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
                try
                {
                    pconnect = exmemory.GetProcAddress("WS2_32", "connect");
                }
                catch (Exception)
                {
                    // ignored
                }

                counter++;
            }

            //var pOpenClipboard = exmemory.GetProcAddress("user32", "OpenClipboard");
            //var pEmptyClipboard = exmemory.GetProcAddress("user32", "EmptyClipboard");
            //var pCloseClipboard = exmemory.GetProcAddress("user32", "CloseClipboard");
            //var pSetClipboardData = exmemory.GetProcAddress("user32", "SetClipboardData");

            //var pGlobalLock = exmemory.GetProcAddress("kernel32", "GlobalLock");
            //var pGlobalUnlock = exmemory.GetProcAddress("kernel32", "GlobalUnlock");
            //var pGlobalAlloc = exmemory.GetProcAddress("kernel32", "GlobalAlloc");

            var plisten = exmemory.GetProcAddress("WS2_32", "listen");
            var pbind = exmemory.GetProcAddress("WS2_32", "bind");
            var paccept = exmemory.GetProcAddress("WS2_32", "accept");
            var psend = exmemory.GetProcAddress("WS2_32", "send");
            var psocket = exmemory.GetProcAddress("WS2_32", "socket");

            //var ws2base = IntPtr.Zero;
            //foreach (ProcessModule processModule in l2.Modules)
            //{
            //    if (processModule.ModuleName.Trim().Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
            //        ws2base = processModule.BaseAddress;
            //}



            if (pconnect.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful injection");
            }

            uint dummy;
            var res = VirtualProtectEx(exmemory.ProcessHandle, pconnect, 10, 0x0020, out dummy);
            _origBytesPreHook = exmemory.Read<long>(pconnect + 0x22);

            _alocatedMem = exmemory.AllocateMemory(1000);

            if (_alocatedMem.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful memory allocation.");
            }

            int writtenBytes = exmemory.WriteBytes(_alocatedMem, sockAddr);

            exmemory.WriteBytes(_alocatedMem + 0x200, sockAddr2);

            if (writtenBytes == 0)
            {
                throw new Exception("Unsuccessful memory patch.");
            }

            //calculate connect+18
            var pWSARecv = exmemory.GetProcAddress("WS2_32", "WSARecv");
            long WSARecvOffset = pWSARecv.ToInt32() + 0x360;

            stub[106] = (byte) (WSARecvOffset);
            stub[107] = (byte) (WSARecvOffset >> 8);
            stub[108] = (byte) (WSARecvOffset >> 0x10);
            stub[109] = (byte) (WSARecvOffset >> 0x18);

            stub[19] = (byte) (_alocatedMem.ToInt32());
            stub[20] = (byte) (_alocatedMem.ToInt32() >> 8);
            stub[21] = (byte) (_alocatedMem.ToInt32() >> 0x10);
            stub[22] = (byte) (_alocatedMem.ToInt32() >> 0x18);

            long callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                                 psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 34);

            stub[30] = (byte) (callToSocket);
            stub[31] = (byte) (callToSocket >> 8);
            stub[32] = (byte) (callToSocket >> 0x10);
            stub[33] = (byte) (callToSocket >> 0x18);

            stub[38] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[39] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[40] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[41] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[45] = (byte) ((_alocatedMem.ToInt32() + 0x200));
            stub[46] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[47] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[48] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            long callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                               pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 55);

            stub[51] = (byte) (callToBind);
            stub[52] = (byte) (callToBind >> 8);
            stub[53] = (byte) (callToBind >> 0x10);
            stub[54] = (byte) (callToBind >> 0x18);

            stub[59] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[60] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[61] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[62] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                                 plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 68);

            stub[64] = (byte) (callToListen);
            stub[65] = (byte) (callToListen >> 8);
            stub[66] = (byte) (callToListen >> 0x10);
            stub[67] = (byte) (callToListen >> 0x18);

            stub[71] = (byte) (_alocatedMem.ToInt32() + 200);
            stub[72] = (byte) ((_alocatedMem.ToInt32() + 200) >> 8);
            stub[73] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[74] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[77] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[78] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[79] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[80] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                                 paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 86);

            stub[82] = (byte) (callToAccept);
            stub[83] = (byte) (callToAccept >> 8);
            stub[84] = (byte) (callToAccept >> 0x10);
            stub[85] = (byte) (callToAccept >> 0x18);



            long callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                               psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 97);

            stub[93] = (byte) (callToSend);
            stub[94] = (byte) (callToSend >> 8);
            stub[95] = (byte) (callToSend >> 0x10);
            stub[96] = (byte) (callToSend >> 0x18);


            //log.Debug("-1");
            var result = exmemory.WriteBytes(_alocatedMem + 20, stub);
            //log.Debug("0");
            _hookJumpToSize = 0;
            if (_alocatedMem.ToInt32() < pconnect.ToInt32())
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20 + 0x100000000) - pconnect.ToInt32() - 0x27;
            }
            else
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20) - pconnect.ToInt32() - 0x27;
            }

            //log.Debug("1");
            InstallHookClient();

            //log.Debug("2");
            botInstance = new L2Bot(this);
            MainWindow.ViewModel.Bots.Add(botInstance);
            if (MainWindow.ViewModel.SelectedBot == null)
                MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();

            exmemory.ProcessExited += (sender, args) =>
            {
                botInstance?.Engine?.Abort();

                if (MainWindow.ViewModel.Bots.Contains(botInstance))
                    MainWindow.ViewModel.Bots.Remove(botInstance);

                if (MainWindow.ViewModel.SelectedBot == null && MainWindow.ViewModel.Bots.Any())
                    MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();
            };

            //log.Debug("3");
            //wait for ip transfer and remove the hook
            string remoteAddr = String.Empty;
            TcpListener server = null;
            try
            {
                Int32 port = openPort;
                TcpClient client;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(100);
                        client = new TcpClient("127.0.0.1", port);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                NetworkStream stream = client.GetStream();

                // Buffer to store the response bytes.
                var data = new Byte[8];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);

                int remotePort = (data[3] | (data[2] << 8));
                string remoteIp = data[4] + "." + data[5] + "." + data[6] + "." + data[7];
                remoteAddr = remoteIp + ":" + remotePort;

                //log.Debug(string.Format("Received: {0}", string.Join(", ", data)));

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                log.Debug($"An exception occured during tranfer: {e}");
                MainWindow.ViewModel.Bots.Remove(botInstance);
                return;
            }

            //RestoreHookedBytes();

            var authServer = new Server
            {
                IsAuthServer = true,
                LocalAddress = "127.0.0.1", //localhost
                LocalPort = HostPort,
                RemoteAddress = remoteAddr.Split(':')[0], //l2authd.lineage2.com
                RemotePort = int.Parse(remoteAddr.Split(':')[1]),
                L2Injector = this,
                BotInstance = botInstance
            };


            authServer.Start();
            //log.Debug($"Started proxying to {remoteAddr} .");
        }

        public void RerouteL2Win8_1(double wait)
        {
            Thread.Sleep(500);
            log.Info($"Injecting at PID: {ProcessId}");
            Thread.Sleep((int) wait);
            //PatchFindWindows();
            int pid = ProcessId;
            byte[] sockAddr = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0}; //for game proxy
            sockAddr[2] = (byte) (HostPort >> 8);
            sockAddr[3] = (byte) (HostPort & 0xff);
            byte[] sockAddr2 = {2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0};
            //socket for interprocess communication (ip transfer)
            int openPort = PortChecker.GetOpenPort();
            sockAddr2[2] = (byte) (openPort >> 8);
            sockAddr2[3] = (byte) (openPort & 0xff);


            for (int i = 0; i < 4; i++)
            {
                sockAddr[4 + i] = byte.Parse("127.0.0.1".Split('.')[i]);
            }

            byte[] stub =
            {
                0x60, //pushad (preserve registers)
                0x8B, 0x44, 0x24, 0x64, //mov eax,[esp+0x64](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x44, 0x24, 0x64, 0x00, 0x00, 0x00, 0x00, //mov [esp+0x64], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x90, 0x90,
                0xFF, 0x34, 0x24, //push [esp] (junk to be cleared after return, filling gap)
                0x81, 0xFE, 0x0, 0x00, 0x0, 0x0, //cmp esi,ntohs+40
                0xc3, //ret
            };

            Process l2;

            try
            {
                l2 = Process.GetProcessById(pid);
            }
            catch (Exception e)
            {
                log.Debug(e);
                return;
            }

            //different parameter offsets in SG clients
            //if (!IsSGLoaded())
            //{
            //    stub[9] -= 4;
            //    stub[19] -= 4;
            //    stub[32] -= 4;
            //}


            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            IntPtr pconnect = IntPtr.Zero;
            try
            {
                pconnect = exmemory.GetProcAddress("WS2_32", "connect");
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsSGLoaded())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!SGKey.OffsetList.ContainsKey(md5hash.ToString()))
                            return;

                        //log.Debug("z");
                    }
                }
            }

            int counter = 0;
            while (counter < 30 && pconnect.ToInt32() == 0)
            {
                Thread.Sleep(100);
                exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
                try
                {
                    pconnect = exmemory.GetProcAddress("WS2_32", "connect");
                }
                catch (Exception)
                {
                    // ignored
                }

                counter++;
            }

            //var pOpenClipboard = exmemory.GetProcAddress("user32", "OpenClipboard");
            //var pEmptyClipboard = exmemory.GetProcAddress("user32", "EmptyClipboard");
            //var pCloseClipboard = exmemory.GetProcAddress("user32", "CloseClipboard");
            //var pSetClipboardData = exmemory.GetProcAddress("user32", "SetClipboardData");

            //var pGlobalLock = exmemory.GetProcAddress("kernel32", "GlobalLock");
            //var pGlobalUnlock = exmemory.GetProcAddress("kernel32", "GlobalUnlock");
            //var pGlobalAlloc = exmemory.GetProcAddress("kernel32", "GlobalAlloc");

            var plisten = exmemory.GetProcAddress("WS2_32", "listen");
            var pbind = exmemory.GetProcAddress("WS2_32", "bind");
            var paccept = exmemory.GetProcAddress("WS2_32", "accept");
            var psend = exmemory.GetProcAddress("WS2_32", "send");
            var psocket = exmemory.GetProcAddress("WS2_32", "socket");

            //var ws2base = IntPtr.Zero;
            //foreach (ProcessModule processModule in l2.Modules)
            //{
            //    if (processModule.ModuleName.Trim().Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
            //        ws2base = processModule.BaseAddress;
            //}



            if (pconnect.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful injection");
            }

            uint dummy;
            var res = VirtualProtectEx(exmemory.ProcessHandle, pconnect, 10, 0x0020, out dummy);
            _origBytesPreHook = exmemory.Read<long>(pconnect + 0x23);

            _alocatedMem = exmemory.AllocateMemory(1000);

            if (_alocatedMem.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful memory allocation.");
            }

            int writtenBytes = exmemory.WriteBytes(_alocatedMem, sockAddr);

            exmemory.WriteBytes(_alocatedMem + 0x200, sockAddr2);

            if (writtenBytes == 0)
            {
                throw new Exception("Unsuccessful memory patch.");
            }

            //calculate connect+18
            var pntohs = exmemory.GetProcAddress("WS2_32", "ntohs");
            long NtohsOffset = pntohs.ToInt32() + 0x40;

            stub[106] = (byte) (NtohsOffset);
            stub[107] = (byte) (NtohsOffset >> 8);
            stub[108] = (byte) (NtohsOffset >> 0x10);
            stub[109] = (byte) (NtohsOffset >> 0x18);

            stub[19] = (byte) (_alocatedMem.ToInt32());
            stub[20] = (byte) (_alocatedMem.ToInt32() >> 8);
            stub[21] = (byte) (_alocatedMem.ToInt32() >> 0x10);
            stub[22] = (byte) (_alocatedMem.ToInt32() >> 0x18);

            long callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                                 psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 34);

            stub[30] = (byte) (callToSocket);
            stub[31] = (byte) (callToSocket >> 8);
            stub[32] = (byte) (callToSocket >> 0x10);
            stub[33] = (byte) (callToSocket >> 0x18);

            stub[38] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[39] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[40] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[41] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[45] = (byte) ((_alocatedMem.ToInt32() + 0x200));
            stub[46] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[47] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[48] = (byte) ((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            long callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                               pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 55);

            stub[51] = (byte) (callToBind);
            stub[52] = (byte) (callToBind >> 8);
            stub[53] = (byte) (callToBind >> 0x10);
            stub[54] = (byte) (callToBind >> 0x18);

            stub[59] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[60] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[61] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[62] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                                 plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 68);

            stub[64] = (byte) (callToListen);
            stub[65] = (byte) (callToListen >> 8);
            stub[66] = (byte) (callToListen >> 0x10);
            stub[67] = (byte) (callToListen >> 0x18);

            stub[71] = (byte) (_alocatedMem.ToInt32() + 200);
            stub[72] = (byte) ((_alocatedMem.ToInt32() + 200) >> 8);
            stub[73] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[74] = (byte) ((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[77] = (byte) ((_alocatedMem.ToInt32() + 16));
            stub[78] = (byte) ((_alocatedMem.ToInt32() + 16) >> 8);
            stub[79] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[80] = (byte) ((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                                 paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 86);

            stub[82] = (byte) (callToAccept);
            stub[83] = (byte) (callToAccept >> 8);
            stub[84] = (byte) (callToAccept >> 0x10);
            stub[85] = (byte) (callToAccept >> 0x18);



            long callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                               psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 97);

            stub[93] = (byte) (callToSend);
            stub[94] = (byte) (callToSend >> 8);
            stub[95] = (byte) (callToSend >> 0x10);
            stub[96] = (byte) (callToSend >> 0x18);


            //log.Debug("-1");
            var result = exmemory.WriteBytes(_alocatedMem + 20, stub);
            //log.Debug("0");
            _hookJumpToSize = 0;
            if (_alocatedMem.ToInt32() < pconnect.ToInt32())
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20 + 0x100000000) - pconnect.ToInt32() - 0x28;
            }
            else
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20) - pconnect.ToInt32() - 0x28;
            }

            //log.Debug("1");
            InstallHookClient();

            //log.Debug("2");
            botInstance = new L2Bot(this);
            MainWindow.ViewModel.Bots.Add(botInstance);
            if (MainWindow.ViewModel.SelectedBot == null)
                MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();

            exmemory.ProcessExited += (sender, args) =>
            {
                botInstance?.Engine?.Abort();

                if (MainWindow.ViewModel.Bots.Contains(botInstance))
                    MainWindow.ViewModel.Bots.Remove(botInstance);

                if (MainWindow.ViewModel.SelectedBot == null && MainWindow.ViewModel.Bots.Any())
                    MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();
            };

            //log.Debug("3");
            //wait for ip transfer and remove the hook
            string remoteAddr = String.Empty;
            TcpListener server = null;
            try
            {
                Int32 port = openPort;
                TcpClient client;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(100);
                        client = new TcpClient("127.0.0.1", port);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                NetworkStream stream = client.GetStream();

                // Buffer to store the response bytes.
                var data = new Byte[8];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);

                int remotePort = (data[3] | (data[2] << 8));
                string remoteIp = data[4] + "." + data[5] + "." + data[6] + "." + data[7];
                remoteAddr = remoteIp + ":" + remotePort;

                //log.Debug(string.Format("Received: {0}", string.Join(", ", data)));

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                log.Debug($"An exception occured during tranfer: {e}");
                MainWindow.ViewModel.Bots.Remove(botInstance);
                return;
            }

            //RestoreHookedBytes();

            var authServer = new Server
            {
                IsAuthServer = true,
                LocalAddress = "127.0.0.1", //localhost
                LocalPort = HostPort,
                RemoteAddress = remoteAddr.Split(':')[0], //l2authd.lineage2.com
                RemotePort = int.Parse(remoteAddr.Split(':')[1]),
                L2Injector = this,
                BotInstance = botInstance
            };


            authServer.Start();
            //log.Debug($"Started proxying to {remoteAddr} .");
        }

        public void RerouteL2Win8_0(double wait)
        {
            Thread.Sleep(500);
            log.Info($"Injecting at PID: {ProcessId}");
            Thread.Sleep((int)wait);
            //PatchFindWindows();
            int pid = ProcessId;
            byte[] sockAddr = { 2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0 }; //for game proxy
            sockAddr[2] = (byte)(HostPort >> 8);
            sockAddr[3] = (byte)(HostPort & 0xff);
            byte[] sockAddr2 = { 2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0 };
            //socket for interprocess communication (ip transfer)
            int openPort = PortChecker.GetOpenPort();
            sockAddr2[2] = (byte)(openPort >> 8);
            sockAddr2[3] = (byte)(openPort & 0xff);


            for (int i = 0; i < 4; i++)
            {
                sockAddr[4 + i] = byte.Parse("127.0.0.1".Split('.')[i]);
            }

            byte[] stub =
            {
                0x60, //pushad (preserve registers)
                0x8B, 0x44, 0x24, 0x50, //mov eax,[esp+0x64](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x44, 0x24, 0x50, 0x00, 0x00, 0x00, 0x00, //mov [esp+0x64], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x90, 0x90,
                0xFF, 0x34, 0x24, //push [esp] (junk to be cleared after return, filling gap)
                0x8b, 0x0d, 0x0, 0x00, 0x0, 0x0, //mov ecx,[ws2_32.dll+3a080]
                0xc3, //ret
            };

            Process l2;

            try
            {
                l2 = Process.GetProcessById(pid);
            }
            catch (Exception e)
            {
                log.Debug(e);
                return;
            }

            //different parameter offsets in SG clients
            //if (!IsSGLoaded())
            //{
            //    stub[9] -= 4;
            //    stub[19] -= 4;
            //    stub[32] -= 4;
            //}


            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            IntPtr pconnect = IntPtr.Zero;
            try
            {
                pconnect = exmemory.GetProcAddress("WS2_32", "connect");
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsSGLoaded())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!SGKey.OffsetList.ContainsKey(md5hash.ToString()))
                            return;

                        //log.Debug("z");
                    }
                }
            }

            int counter = 0;
            while (counter < 30 && pconnect.ToInt32() == 0)
            {
                Thread.Sleep(100);
                exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
                try
                {
                    pconnect = exmemory.GetProcAddress("WS2_32", "connect");
                }
                catch (Exception)
                {
                    // ignored
                }

                counter++;
            }

            //var pOpenClipboard = exmemory.GetProcAddress("user32", "OpenClipboard");
            //var pEmptyClipboard = exmemory.GetProcAddress("user32", "EmptyClipboard");
            //var pCloseClipboard = exmemory.GetProcAddress("user32", "CloseClipboard");
            //var pSetClipboardData = exmemory.GetProcAddress("user32", "SetClipboardData");

            //var pGlobalLock = exmemory.GetProcAddress("kernel32", "GlobalLock");
            //var pGlobalUnlock = exmemory.GetProcAddress("kernel32", "GlobalUnlock");
            //var pGlobalAlloc = exmemory.GetProcAddress("kernel32", "GlobalAlloc");

            var plisten = exmemory.GetProcAddress("WS2_32", "listen");
            var pbind = exmemory.GetProcAddress("WS2_32", "bind");
            var paccept = exmemory.GetProcAddress("WS2_32", "accept");
            var psend = exmemory.GetProcAddress("WS2_32", "send");
            var psocket = exmemory.GetProcAddress("WS2_32", "socket");

            //var ws2base = IntPtr.Zero;
            //foreach (ProcessModule processModule in l2.Modules)
            //{
            //    if (processModule.ModuleName.Trim().Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
            //        ws2base = processModule.BaseAddress;
            //}



            if (pconnect.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful injection");
            }

            uint dummy;
            var res = VirtualProtectEx(exmemory.ProcessHandle, pconnect, 10, 0x0020, out dummy);
            _origBytesPreHook = exmemory.Read<long>(pconnect + 0x29);

            _alocatedMem = exmemory.AllocateMemory(1000);

            if (_alocatedMem.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful memory allocation.");
            }

            int writtenBytes = exmemory.WriteBytes(_alocatedMem, sockAddr);

            exmemory.WriteBytes(_alocatedMem + 0x200, sockAddr2);

            if (writtenBytes == 0)
            {
                throw new Exception("Unsuccessful memory patch.");
            }

            var pws2_32 = IntPtr.Zero;
            foreach (ProcessModule processModule in l2.Modules)
            {
                if (processModule.ModuleName.Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
                    pws2_32 = processModule.BaseAddress;
            }

            if (pws2_32.ToInt32() == 0)
                throw new Exception("Module not found");



            //var disasm = new SharpDisasm.Disassembler(
            //    stub,
            //    ArchitectureMode.x86_32, 0, true);
            //// Disassemble each instruction and output to console
            //foreach (var insn in disasm.Disassemble())
            //    Console.Out.WriteLine(insn.ToString());

            long Offset = pws2_32.ToInt32() + 0x3a080;

            stub[106] = (byte)(Offset);
            stub[107] = (byte)(Offset >> 8);
            stub[108] = (byte)(Offset >> 0x10);
            stub[109] = (byte)(Offset >> 0x18);

            stub[19] = (byte)(_alocatedMem.ToInt32());
            stub[20] = (byte)(_alocatedMem.ToInt32() >> 8);
            stub[21] = (byte)(_alocatedMem.ToInt32() >> 0x10);
            stub[22] = (byte)(_alocatedMem.ToInt32() >> 0x18);

            long callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                                 psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 34);

            stub[30] = (byte)(callToSocket);
            stub[31] = (byte)(callToSocket >> 8);
            stub[32] = (byte)(callToSocket >> 0x10);
            stub[33] = (byte)(callToSocket >> 0x18);

            stub[38] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[39] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[40] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[41] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[45] = (byte)((_alocatedMem.ToInt32() + 0x200));
            stub[46] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[47] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[48] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            long callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                               pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 55);

            stub[51] = (byte)(callToBind);
            stub[52] = (byte)(callToBind >> 8);
            stub[53] = (byte)(callToBind >> 0x10);
            stub[54] = (byte)(callToBind >> 0x18);

            stub[59] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[60] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[61] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[62] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                                 plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 68);

            stub[64] = (byte)(callToListen);
            stub[65] = (byte)(callToListen >> 8);
            stub[66] = (byte)(callToListen >> 0x10);
            stub[67] = (byte)(callToListen >> 0x18);

            stub[71] = (byte)(_alocatedMem.ToInt32() + 200);
            stub[72] = (byte)((_alocatedMem.ToInt32() + 200) >> 8);
            stub[73] = (byte)((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[74] = (byte)((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[77] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[78] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[79] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[80] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                                 paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 86);

            stub[82] = (byte)(callToAccept);
            stub[83] = (byte)(callToAccept >> 8);
            stub[84] = (byte)(callToAccept >> 0x10);
            stub[85] = (byte)(callToAccept >> 0x18);



            long callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                               psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 97);

            stub[93] = (byte)(callToSend);
            stub[94] = (byte)(callToSend >> 8);
            stub[95] = (byte)(callToSend >> 0x10);
            stub[96] = (byte)(callToSend >> 0x18);


            //log.Debug("-1");
            var result = exmemory.WriteBytes(_alocatedMem + 20, stub);
            //log.Debug("0");
            _hookJumpToSize = 0;
            if (_alocatedMem.ToInt32() < pconnect.ToInt32())
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20 + 0x100000000) - pconnect.ToInt32() - 0x2E;
            }
            else
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20) - pconnect.ToInt32() - 0x2E;
            }

            //log.Debug("1");
            InstallHookClient();

            //log.Debug("2");
            botInstance = new L2Bot(this);
            MainWindow.ViewModel.Bots.Add(botInstance);
            if (MainWindow.ViewModel.SelectedBot == null)
                MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();

            exmemory.ProcessExited += (sender, args) =>
            {
                botInstance?.Engine?.Abort();

                if (MainWindow.ViewModel.Bots.Contains(botInstance))
                    MainWindow.ViewModel.Bots.Remove(botInstance);

                if (MainWindow.ViewModel.SelectedBot == null && MainWindow.ViewModel.Bots.Any())
                    MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();
            };

            //log.Debug("3");
            //wait for ip transfer and remove the hook
            string remoteAddr = String.Empty;
            TcpListener server = null;
            try
            {
                Int32 port = openPort;
                TcpClient client;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(100);
                        client = new TcpClient("127.0.0.1", port);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                NetworkStream stream = client.GetStream();

                // Buffer to store the response bytes.
                var data = new Byte[8];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);

                int remotePort = (data[3] | (data[2] << 8));
                string remoteIp = data[4] + "." + data[5] + "." + data[6] + "." + data[7];
                remoteAddr = remoteIp + ":" + remotePort;

                //log.Debug(string.Format("Received: {0}", string.Join(", ", data)));

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                log.Debug($"An exception occured during tranfer: {e}");
                MainWindow.ViewModel.Bots.Remove(botInstance);
                return;
            }

            //RestoreHookedBytes();

            var authServer = new Server
            {
                IsAuthServer = true,
                LocalAddress = "127.0.0.1", //localhost
                LocalPort = HostPort,
                RemoteAddress = remoteAddr.Split(':')[0], //l2authd.lineage2.com
                RemotePort = int.Parse(remoteAddr.Split(':')[1]),
                L2Injector = this,
                BotInstance = botInstance
            };


            authServer.Start();
            //log.Debug($"Started proxying to {remoteAddr} .");
        }


        Process l2;

        public void RerouteL2Auto(double wait)
        {
            //Thread.Sleep(3000);

            long memsize = 0; // memsize in Megabyte

            //PerformanceCounter PC = new PerformanceCounter();
            //PC.CategoryName = "Process";
            //PC.CounterName = "Working Set - Private";
            //PC.InstanceName = l2.ProcessName;
            //memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
            //PC.Close();
            //PC.Dispose();
            int memequalcount = 0;
            long maxmemsize = 0;

            

            log.Info($"Injecting at PID: {ProcessId}");

            while (!l2.HasExited)
            {
                //PC = new PerformanceCounter();
                //PC.CategoryName = "Process";
                //PC.CounterName = "Working Set - Private";
                //PC.InstanceName = l2.ProcessName;
                //PC.Close();
                //PC.Dispose();
                //memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
                memsize = MemoryInformation.GetMemoryUsageForProcess(ProcessId);
                if (memsize - maxmemsize > 20)
                {
                    maxmemsize = memsize;
                    //log.Debug(maxmemsize);
                    memequalcount = 0;
                }
                else
                {
                    memequalcount++;
                    if(memequalcount>4 && memsize> 250000000)
                        break; 
                }

                if (memsize > 250000000)
                    break;

                //log.Debug(memsize);
                Thread.Sleep(100);
            }

            //log.Debug(maxmemsize);
            //Thread.Sleep(700);
            //if (File.Exists(patha))
            //{
            //    log.Debug("pre auth");
            //    while (!l2.HasExited && !IsSGLoaded())
            //    {
            //        Thread.Sleep(100);
                    
            //    }
            //    log.Debug("post auth");
            //}

            if (l2.HasExited)
                return;
            
            //Thread.Sleep((int)wait);
            //PatchFindWindows();
            int pid = ProcessId;
            byte[] sockAddr = { 2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0 }; //for game proxy
            sockAddr[2] = (byte)(HostPort >> 8);
            sockAddr[3] = (byte)(HostPort & 0xff);
            byte[] sockAddr2 = { 2, 0, 8, 0x3a, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x0, 0x0 };
            //socket for interprocess communication (ip transfer)
            int openPort = PortChecker.GetOpenPort();
            sockAddr2[2] = (byte)(openPort >> 8);
            sockAddr2[3] = (byte)(openPort & 0xff);


            for (int i = 0; i < 4; i++)
            {
                sockAddr[4 + i] = byte.Parse("127.0.0.1".Split('.')[i]);
            }

            byte[] stub =
            {
                0x60, //pushad (preserve registers)
                0x8B, 0x44, 0x24, 0x50, //mov eax,[esp+0x50](original connect structure pointer)
                0x81, 0x38, 0x02, 0x00, 0x00, 0x50, //cmp [eax], 50000002
                0x74, 84, //je end (if port 80 skip)
                0x8b, 0xF0,
                0xC7, 0x44, 0x24, 0x50, 0x00, 0x00, 0x00, 0x00, //mov [esp+0x50], custom_sockaddr
                0x6A, 0, //push 0
                0x6A, 1, //push 1
                0x6A, 2, //push 2
                0xE8, 0, 0, 0, 0, //socket(AF_INET (2), SOCK_STREAM (1), 0)
                0x90, 0x90, //mov esi(placeholder),eax(original connect structure pointer)
                0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+16],eax (save server_socket)
                0x6A, 0x10, //push 16
                0x68, 0, 0, 0, 0, // push sockaddr
                0x50, // push eax (server_socket)
                0xE8, 0, 0, 0, 0, //bind(server_socket,sockaddr_in, 16)
                0x6A, 5, //push 5
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0x0, 0x0, 0x0, 0x0, //call listen(server_socket, 5)
                0x6A, 0x0, //push 0
                0x68, 0, 0, 0, 0, // push sockaddr
                0xFF, 0x35, 0, 0, 0, 0, //push server_socket 
                0xE8, 0, 0, 0, 0, //accept(server_socket,sockaddr_in,0)
                //0x89, 0x05, 0x0, 0x0, 0x0, 0x0, //mov [addr+20],eax (save client_socket)
                0x6A, 0x0, //push 0
                0x6A, 0x8, //push 8
                0x56, //push esi (default struct?)
                0x50, //push eax (client_socket)
                0xE8, 0, 0, 0, 0, //send(client_socket,default_struct,8,0)
                0x90, //NOP (pop eax)
                0x61, //popad (restore registers)
                //0xB8, 0, 0, 0, 0, //mov eax, custom_struct
                0x5f,//pop edi (clean return address from call)
                0x8b, 0x0d, 0x0, 0x00, 0x0, 0x90, //mov ecx,[ws2_32.dll+3a080]
                0x90, 0x90,
                0xFF, 0x34, 0x24, //push [esp] (junk to be cleared after return, filling gap)
                0xe9,0,0,0,0, //ret
            };



            //different parameter offsets in SG clients
            //if (!IsSGLoaded())
            //{
            //    stub[9] -= 4;
            //    stub[19] -= 4;
            //    stub[32] -= 4;
            //}

            _dbgCheckForClient = new Thread(delegate ()
            {
                //Process l2a;
                //try
                //{
                //    l2a = Process.GetProcessById(ProcessId);
                //}
                //catch (Exception)
                //{
                //    return;
                //}

                while (true)
                {
                    if (l2.HasExited)
                        break;

                    int dbgport = 0, retnSize;
                    NtQueryInformationProcess(l2.Handle, 0x7, ref dbgport, 4, out retnSize);
#if !DEBUG
                    if (dbgport != 0)
                    {
                        BadStuff = true;
                        ShouldInjectInNextClient = false;
                        if(_origBytesPreHook != 0)
                            RestoreHookedBytes();

                        l2.Kill();
                        log.Debug("Memory access violation.");
                        Environment.Exit(0);
                    }
#endif
                    Thread.Sleep(1000);
                }

            });
            _dbgCheckForClient.Start();

            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            IntPtr pconnect = IntPtr.Zero;
            try
            {
                pconnect = exmemory.GetProcAddress("WS2_32", "connect");
            }
            catch (Exception)
            {
                // ignored
            }
            
            if (IsSGLoaded())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!SGKey.OffsetList.ContainsKey(md5hash.ToString()))
                        {
                            log.Debug("Unsupported protection module.");
                            l2.Kill();
                            Environment.Exit(0);
                            return;
                        }

                        //log.Debug("z");
                    }
                }
            }

            if (IsRpgClient())
            {
                string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "system.dll";

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                        //log.Debug(md5hash);
                        if (!RpgInGameCipher.OffsetList.Contains(md5hash.ToString()))
                        {
                            log.Debug("Unsupported protection module.");
                            l2.Kill();
                            Environment.Exit(0);
                            return;
                        }

                        //log.Debug("z");
                    }
                }
            }

            int counter = 0;
            while (counter < 30 && pconnect.ToInt32() == 0)
            {
                Thread.Sleep(100);
                exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
                try
                {
                    pconnect = exmemory.GetProcAddress("WS2_32", "connect");
                }
                catch (Exception)
                {
                    // ignored
                }

                counter++;
            }

            //var pOpenClipboard = exmemory.GetProcAddress("user32", "OpenClipboard");
            //var pEmptyClipboard = exmemory.GetProcAddress("user32", "EmptyClipboard");
            //var pCloseClipboard = exmemory.GetProcAddress("user32", "CloseClipboard");
            //var pSetClipboardData = exmemory.GetProcAddress("user32", "SetClipboardData");

            //var pGlobalLock = exmemory.GetProcAddress("kernel32", "GlobalLock");
            //var pGlobalUnlock = exmemory.GetProcAddress("kernel32", "GlobalUnlock");
            //var pGlobalAlloc = exmemory.GetProcAddress("kernel32", "GlobalAlloc");

            var plisten = exmemory.GetProcAddress("WS2_32", "listen");
            var pbind = exmemory.GetProcAddress("WS2_32", "bind");
            var paccept = exmemory.GetProcAddress("WS2_32", "accept");
            var psend = exmemory.GetProcAddress("WS2_32", "send");
            var psocket = exmemory.GetProcAddress("WS2_32", "socket");

            //var ws2base = IntPtr.Zero;
            //foreach (ProcessModule processModule in l2.Modules)
            //{
            //    if (processModule.ModuleName.Trim().Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
            //        ws2base = processModule.BaseAddress;
            //}



            if (pconnect.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful injection");
            }
            

            _alocatedMem = exmemory.AllocateMemory(1000);

            if (_alocatedMem.ToInt32() == 0)
            {
                throw new Exception("Unsuccessful memory allocation.");
            }

            int writtenBytes = exmemory.WriteBytes(_alocatedMem, sockAddr);

            exmemory.WriteBytes(_alocatedMem + 0x200, sockAddr2);

            if (writtenBytes == 0)
            {
                throw new Exception("Unsuccessful memory patch.");
            }

            var pws2_32 = IntPtr.Zero;
            foreach (ProcessModule processModule in l2.Modules)
            {
                if (processModule.ModuleName.Equals("ws2_32.dll", StringComparison.OrdinalIgnoreCase))
                    pws2_32 = processModule.BaseAddress;
            }

            if (pws2_32.ToInt32() == 0)
                throw new Exception("Module not found");


            var procedure = exmemory.ReadBytes(pconnect, 100);

            var disasm = new Disassembler(
                procedure,
                ArchitectureMode.x86_32, 0, true);
            // Disassemble each instruction and output to console
            long argOffset = 12;
            bool init = false;
            int curOffset = 0;
            byte[] door = new byte[8];
            long dest = 0;
            bool isCallHook = false;

            foreach (var instruction in disasm.Disassemble())
            {
                curOffset += instruction.Length;
                if (instruction.ToString().StartsWith("sub "))
                    init = true;

                if (!init)
                    continue;

                if (instruction.ToString().Substring(0, 3) == "cmp" && _hookOffset == 0 && instruction.Length == 6)
                {
                    _doorSize = instruction.Length;
                    _hookOffset = curOffset - instruction.Length;
                    instruction.Bytes.CopyTo(door,0);
                    break;
                }
                else if (instruction.ToString().Substring(0, 3) == "mov" && _hookOffset == 0 && instruction.Length == 6)
                {
                    _doorSize = instruction.Length;
                    _hookOffset = curOffset - instruction.Length;
                    instruction.Bytes.CopyTo(door, 0);
                    break;
                }
                else if (instruction.ToString().Substring(0, 4) == "call" && _hookOffset == 0 && (instruction.Length == 5 || instruction.Length == 6))
                {
                    break;
                }
            }

            //PATCH
            _hookOffset = 0;
            curOffset = 0;
            List<Instruction> hookStub = new List<Instruction>();
            if (_hookOffset == 0)
            {
                curOffset += 0;
                List<Instruction> instructions = new List<Instruction>();
                foreach (var instruction in disasm.Disassemble())
                {
                    instructions.Add(instruction);
                }

                int suitableBytes = 0;
                for (int i = 0; i < instructions.Count && suitableBytes<5; i++)
                {
                    curOffset += instructions[i].Length;
                    int count = 0;
                    while (i >=2 && suitableBytes < 5)
                    {
                        if (instructions[i+count].ToString().StartsWith("j") || instructions[i + count].ToString().StartsWith("call"))
                        {
                            suitableBytes = 0;
                            hookStub.Clear();
                            break;
                        }

                        hookStub.Add(instructions[i+count]);
                        suitableBytes += instructions[i + count].Length;
                        count++;

                        if (suitableBytes > 8)
                        {
                            suitableBytes = 0;
                            hookStub.Clear();
                            break;
                        }
                    }

                    if(suitableBytes>=5)
                        _hookOffset=curOffset - instructions[i].Length;
                }
            }

            curOffset = 0;
            init = false;
            foreach (var instruction in disasm.Disassemble())
            {
                curOffset += instruction.Length;
                if (instruction.ToString().StartsWith("sub "))
                    init = true;

                if (!init)
                    continue;

                string beginnning = instruction.ToString().Substring(0, 4);
                switch (beginnning)
                {
                    case "sub ":
                        if (instruction.ToString().Contains("esp"))
                            argOffset += instruction.Operands[1].Value;
                        break;
                    case "push":
                        argOffset += 4;
                        break;
                    case "pop ":
                        argOffset -= 4;
                        break;
                }

                if(curOffset >= _hookOffset)
                    break;
            }

            //call return addr
            argOffset += 4;
            //pushad
            argOffset += 0x20;

            if (_hookOffset == 0)
                throw new Exception("Unsuccessful injection.");

            if (hookStub.Count > 0)
            {
                _doorSize = 0;
                foreach (var instruction in hookStub)
                {
                    for (int i = 0; i < instruction.Length; i++)
                    {
                        door[i + _doorSize] = instruction.Bytes[i];
                    }
                    _doorSize += instruction.Bytes.Length;
                }
            }

            _origBytesPreHook = exmemory.Read<long>(pconnect + _hookOffset);
            //long Offset = pws2_32.ToInt32() + 0x3a080;

            stub[4] = (byte)argOffset;
            stub[18] = (byte) argOffset;

            

            if (_doorSize == 5)
            {
                stub[108] = 0x90;
                stub[109] = 0x90;
                stub[110] = 0x90;
            }

            stub[100] = door[0];
            stub[101] = door[1];
            stub[102] = door[2];
            stub[103] = door[3];
            stub[104] = door[4];

            if(_doorSize > 5)
                stub[105] = door[5];
            if (_doorSize > 6)
                stub[106] = door[6];
            if (_doorSize > 7)
                stub[107] = door[7];

            dest = pconnect.ToInt32() + _hookOffset + _doorSize;
            long calltoHookedDest = ((_alocatedMem.ToInt32() < dest ? 0x100000000 : 0) +
                                 dest) - (_alocatedMem.ToInt32() + 20 + 116);

            stub[112] = (byte)(calltoHookedDest);
            stub[113] = (byte)(calltoHookedDest >> 8);
            stub[114] = (byte)(calltoHookedDest >> 0x10);
            stub[115] = (byte)(calltoHookedDest >> 0x18);


            //if (isCallHook && _doorSize == 5)
            //{
            //    long calltoHookedDest = ((_alocatedMem.ToInt32() < dest ? 0x100000000 : 0) +
            //                     dest) - (_alocatedMem.ToInt32() + 20 + 105);
            //    if (_doorSize == 6) calltoHookedDest--;

            //    int startPos = _doorSize == 5 ? 105 : 106;
            //    for (int i = 0; i < 4; i++)
            //    {
            //        stub[startPos + i] = (byte)(calltoHookedDest >> i*8);
            //    }
            //}

            stub[19] = (byte)(_alocatedMem.ToInt32());
            stub[20] = (byte)(_alocatedMem.ToInt32() >> 8);
            stub[21] = (byte)(_alocatedMem.ToInt32() >> 0x10);
            stub[22] = (byte)(_alocatedMem.ToInt32() >> 0x18);

            long callToSocket = ((_alocatedMem.ToInt32() < psocket.ToInt32() ? 0x100000000 : 0) +
                                 psocket.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 34);

            stub[30] = (byte)(callToSocket);
            stub[31] = (byte)(callToSocket >> 8);
            stub[32] = (byte)(callToSocket >> 0x10);
            stub[33] = (byte)(callToSocket >> 0x18);

            stub[38] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[39] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[40] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[41] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            stub[45] = (byte)((_alocatedMem.ToInt32() + 0x200));
            stub[46] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 8);
            stub[47] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 0x10);
            stub[48] = (byte)((_alocatedMem.ToInt32() + 0x200) >> 0x18);

            long callToBind = ((_alocatedMem.ToInt32() < pbind.ToInt32() ? 0x100000000 : 0) +
                               pbind.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 55);

            stub[51] = (byte)(callToBind);
            stub[52] = (byte)(callToBind >> 8);
            stub[53] = (byte)(callToBind >> 0x10);
            stub[54] = (byte)(callToBind >> 0x18);

            stub[59] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[60] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[61] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[62] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToListen = ((_alocatedMem.ToInt32() < plisten.ToInt32() ? 0x100000000 : 0) +
                                 plisten.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 68);

            stub[64] = (byte)(callToListen);
            stub[65] = (byte)(callToListen >> 8);
            stub[66] = (byte)(callToListen >> 0x10);
            stub[67] = (byte)(callToListen >> 0x18);

            stub[71] = (byte)(_alocatedMem.ToInt32() + 200);
            stub[72] = (byte)((_alocatedMem.ToInt32() + 200) >> 8);
            stub[73] = (byte)((_alocatedMem.ToInt32() + 200) >> 0x10);
            stub[74] = (byte)((_alocatedMem.ToInt32() + 200) >> 0x18);

            stub[77] = (byte)((_alocatedMem.ToInt32() + 16));
            stub[78] = (byte)((_alocatedMem.ToInt32() + 16) >> 8);
            stub[79] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x10);
            stub[80] = (byte)((_alocatedMem.ToInt32() + 16) >> 0x18);

            long callToAccept = ((_alocatedMem.ToInt32() < paccept.ToInt32() ? 0x100000000 : 0) +
                                 paccept.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 86);

            stub[82] = (byte)(callToAccept);
            stub[83] = (byte)(callToAccept >> 8);
            stub[84] = (byte)(callToAccept >> 0x10);
            stub[85] = (byte)(callToAccept >> 0x18);



            long callToSend = ((_alocatedMem.ToInt32() < psend.ToInt32() ? 0x100000000 : 0) +
                               psend.ToInt32()) - (_alocatedMem.ToInt32() + 20 + 97);

            stub[93] = (byte)(callToSend);
            stub[94] = (byte)(callToSend >> 8);
            stub[95] = (byte)(callToSend >> 0x10);
            stub[96] = (byte)(callToSend >> 0x18);


            //log.Debug("-1");
            var result = exmemory.WriteBytes(_alocatedMem + 20, stub);
            //log.Debug("0");
            _hookJumpToSize = 0;
            if (_alocatedMem.ToInt32() < pconnect.ToInt32())
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20 + 0x100000000) - pconnect.ToInt32() - (_hookOffset+5);
            }
            else
            {
                _hookJumpToSize = (_alocatedMem.ToInt32() + 20) - pconnect.ToInt32() - (_hookOffset + 5);
            }

            //log.Debug("1");
            InstallHookClient();

            //log.Debug("2");

            lock (_botAddSyncLock)
            {
                botInstance = new L2Bot(this);
                MainWindow.ViewModel.Bots.Add(botInstance);
                if (MainWindow.ViewModel.SelectedBot == null)
                    MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.First();
            }

            exmemory.ProcessExited += (sender, args) =>
            {
                lock (_botAddSyncLock)
                {
                    botInstance?.Engine?.Abort();
                    if (_dbgCheckForClient?.IsAlive == true)
                        _dbgCheckForClient.Abort();

                    if (MainWindow.ViewModel.SelectedBot == botInstance && MainWindow.ViewModel.Bots.Any())
                        MainWindow.ViewModel.SelectedBot = MainWindow.ViewModel.Bots.FirstOrDefault(bot => !ReferenceEquals(bot, botInstance));

                    if (MainWindow.ViewModel.Bots.Contains(botInstance))
                        MainWindow.ViewModel.Bots.Remove(botInstance);
                }

            };

            //log.Debug("3");
            //wait for ip transfer and remove the hook
            string remoteAddr = String.Empty;
            TcpListener server = null;
            try
            {
                Int32 port = openPort;
                TcpClient client;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(100);
                        client = new TcpClient("127.0.0.1", port);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                NetworkStream stream = client.GetStream();

                // Buffer to store the response bytes.
                var data = new Byte[8];

                // String to store the response ASCII representation.
                string responseData = string.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);

                int remotePort = (data[3] | (data[2] << 8));
                string remoteIp = data[4] + "." + data[5] + "." + data[6] + "." + data[7];
                remoteAddr = remoteIp + ":" + remotePort;

                //log.Debug(string.Format("Received: {0}", string.Join(", ", data)));

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                log.Debug($"An exception occured during tranfer: {e}");
                MainWindow.ViewModel.Bots.Remove(botInstance);
                return;
            }

            //RestoreHookedBytes();

            var authServer = new Server
            {
                IsAuthServer = true,
                LocalAddress = "127.0.0.1", //localhost
                LocalPort = HostPort,
                RemoteAddress = remoteAddr.Split(':')[0], //l2authd.lineage2.com
                RemotePort = int.Parse(remoteAddr.Split(':')[1]),
                L2Injector = this,
                BotInstance = botInstance
            };


            authServer.Start();
            //log.Debug($"Started proxying to {remoteAddr} .");
        }

        private bool _iswin10 = IsWindows10();

        public void PatchWindowsAPI()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;
            if (os.Platform == PlatformID.Win32NT && vs.Major != 10 && !_iswin10 && vs.Major != 8)
            {
                //patch Windows APIs
                try
                {
                    PatchEnumProcesses();
                    PatchProcess32NextW();
                }
                catch (Exception)
                {
                }
            }
        }

        public void PatchEnumProcesses()
        {
            byte[] stub1 =
            {
                0x6a, 0x10, //push 10 (junk)
                0x72, 0x5a, //jb K32EnumProcesses+fd
            };

            byte[] stub2 =
            {
                0x59, //pop ecx (clean junk)
                0x83, 0x65, 0xFC, 0, //and [ebp-4], 0
                0x8B, 0x4C, 0x1a, 0x44, //mov ecx, [edx+ebx+0x44]
                0x8B, 0x7D, 8, //mov edi, [ebp+8]
                0x81, 0xf9, 0x0, 0x0, 0, 0, // cmp ecx, currPID
                0x75, 0x99, //jnz K32EnumProcesses+AA
                0xb9, 0x6e, 0x4, 0, 0, //mov ecx,0x46e
                0x74, 0x95, //je K32EnumProcesses+AD
            };

            int pidToHide = Process.GetCurrentProcess().Id;

            stub2[14] = (byte) (pidToHide);
            stub2[15] = (byte) (pidToHide >> 8);
            stub2[16] = (byte) (pidToHide >> 0x10);
            stub2[17] = (byte) (pidToHide >> 0x18);

            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);
            var pEnumProcesses = exmemory.GetProcAddress("kernel32", "K32EnumProcesses");

            var result = exmemory.WriteBytes(pEnumProcesses + 0x9f, stub1);
            exmemory.WriteBytes(pEnumProcesses + 0xfd, stub2);
        }

        public void PatchProcess32NextW()
        {
            byte[] stub1 =
            {
                0x0F, 0x85, 0xF6, 0x04, 00, 00, //jnz Process32FirstW+0x31f
                0x6A, 0xFF //push -1 (for whatever reasons)
            };

            byte[] stub2 =
            {
                0x89, 0x48, 0x24, //mov [eax+24],ecx
                0x90, //
                0xB9, 0x8B, 0x00, 00, 00, //mov ecx,0000008B
                0x81, 0x7E, 08, 0x0, 0x0, 00, 00, //cmp [esi+08],currpid
                0x0F, 0x85, 0xF6, 0xFA, 0xFF, 0xFF, //jne Process32NextW+6E
                0x8D, 0xB6, 0x8B, 00, 00, 00, //lea esi,[esi+0000008B]
                0x0F, 0x84, 0xEA, 0xFA, 0xFF, 0xFF //je Process32NextW+6E
            };

            int pidToHide = Process.GetCurrentProcess().Id;

            stub2[12] = (byte) (pidToHide);
            stub2[13] = (byte) (pidToHide >> 8);
            stub2[14] = (byte) (pidToHide >> 0x10);
            stub2[15] = (byte) (pidToHide >> 0x18);

            exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

            var memory = exmemory.AllocateMemory(100);


            var pProcess32NextW = exmemory.GetProcAddress("kernel32", "Process32NextW");

            long jmpToStub = memory.ToInt32() - (pProcess32NextW.ToInt32() + 0x66) - 6;

            stub1[2] = (byte) (jmpToStub);
            stub1[3] = (byte) (jmpToStub >> 8);
            stub1[4] = (byte) (jmpToStub >> 0x10);
            stub1[5] = (byte) (jmpToStub >> 0x18);

            long jumpBackSize = (pProcess32NextW.ToInt32() + 0x66) - (memory.ToInt32() + 14);

            stub2[18] = (byte) (jumpBackSize);
            stub2[19] = (byte) (jumpBackSize >> 8);
            stub2[20] = (byte) (jumpBackSize >> 0x10);
            stub2[21] = (byte) (jumpBackSize >> 0x18);

            long jumpBackSize2 = (pProcess32NextW.ToInt32() + 0x66) - (memory.ToInt32() + 26);

            stub2[30] = (byte) (jumpBackSize2);
            stub2[31] = (byte) (jumpBackSize2 >> 8);
            stub2[32] = (byte) (jumpBackSize2 >> 0x10);
            stub2[33] = (byte) (jumpBackSize2 >> 0x18);

            var pProcess32FirstW = exmemory.GetProcAddress("kernel32", "Process32FirstW");
            //var result = exmemory.WriteBytes(pProcess32FirstW + 0x31f, stub2);
            var result = exmemory.WriteBytes(memory, stub2);
            //log.Debug(result);


            result = exmemory.WriteBytes(pProcess32NextW + 0x66, stub1);
            //log.Debug(result);
        }

        //public void PatchFindWindows()
        //{
        //    byte[] stub1 =
        //    {
        //        0xe9, 0x0, 0x0, 0x0, 0x0, 0x90,
        //    };

        //    byte[] stub2 =
        //    {
        //        0x50,//push eax
        //        0x83, 0x7C, 0x24, 0x28, 00,//cmp [esp+28],0
        //        0x74, 0x20,//je legit end+1
        //        0x60,//pushad
        //        0xFF, 0x74, 0x24, 0x48,//push [esp+48] (window title)
        //        0xE8, 0xEE, 0xEF, 0x6F, 01,//call wcslen
        //        0x5F,//pop edi
        //        0x83,0xF8, 08,//cmp eax,8
        //        0x74, 0x15,//je patched end
        //        0x83, 0xF8, 0x0,//cmp eax,0
        //        0x74, 0x10,//je patched end
        //        0x83, 0xF8, 0x0A,//cmp eax,10
        //        0x74, 0x0B,//je patched end
        //        0x83, 0xF8, 0x14,//cmp eax,20
        //        0x74, 0x06,//je patched end
        //        //@legit end
        //        0x61,
        //        0x58,//pop eax
        //        0x5F,//pop edi
        //        //0x83, 0xC4,08,//add esp, 8
        //        0xC2, 0x14,00,//retn 14

        //        //@patched end
        //        0x61,
        //        0xB8, 00, 00, 00, 00,//mov eax,0
        //        0x83, 0xC4, 0x08,//add esp,8
        //        0xC2, 0x14, 00//retn 14
        //    };

        //    Process l2;

        //    try
        //    {
        //        l2 = Process.GetProcessById(ProcessId);
        //    }
        //    catch (Exception e)
        //    {
        //        log.Debug(e);
        //        return;
        //    }

        //    exmemory = new GreyMagic.ExternalProcessMemory(l2, false, false, false);

        //    var memory = exmemory.AllocateMemory(100);

        //    var pwcslen = exmemory.GetProcAddress("ntdll", "wcslen");

        //    var pgapfnScSendMessage = exmemory.GetProcAddress("USER32", "gapfnScSendMessage");

        //    long jmpToStub = memory.ToInt32() - (pgapfnScSendMessage.ToInt32() + 0x82a) - 5;

        //    stub1[1] = (byte)(jmpToStub);
        //    stub1[2] = (byte)(jmpToStub >> 8);
        //    stub1[3] = (byte)(jmpToStub >> 0x10);
        //    stub1[4] = (byte)(jmpToStub >> 0x18);

        //    long callToWcslen = ((memory.ToInt32() < pwcslen.ToInt32() ? 0x100000000 : 0) +
        //   pwcslen.ToInt32()) - ((memory.ToInt32() + 13 + 5));

        //    stub2[14] = (byte)(callToWcslen);
        //    stub2[15] = (byte)(callToWcslen >> 8);
        //    stub2[16] = (byte)(callToWcslen >> 0x10);
        //    stub2[17] = (byte)(callToWcslen >> 0x18);


        //    var result = exmemory.WriteBytes(memory, stub2);
        //    result = exmemory.WriteBytes(pgapfnScSendMessage + 0x82a, stub1);
        //}

        public void RestoreHookedBytes()
        {
            //run in STAThread
            //Thread thread = new Thread(() => {
            //    var pTlsGetValue = exmemory.GetProcAddress("KERNELBASE", "TlsGetValue");
            //    exmemory.WriteBytes(pTlsGetValue + 2, BitConverter.GetBytes(_origBytesPreHook));
            //});

            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();

            //if win7
            Thread thread = null;
            //if (_kernel == Kernel.Windows7)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            //null reroutes to patch and make it jump straight to end
            //            exmemory.WriteBytes(_alocatedMem + 20 + 10, new byte[]
            //            {
            //                0x90, 0x90, 0x90, 0x90,
            //            });
            //            exmemory.WriteBytes(_alocatedMem + 20 + 20, new byte[]
            //            {
            //                0x90, 0x90, 0x90, 0x90,
            //            });
            //        }
            //        catch (Exception e)
            //        {

            //        }
            //    });
            //}
            //else if (_kernel == Kernel.Windows10)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            //null reroutes to patch and make it jump straight to end
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x22, BitConverter.GetBytes(_origBytesPreHook));
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else if (_kernel == Kernel.Windows8_1)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            //null reroutes to patch and make it jump straight to end
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x23, BitConverter.GetBytes(_origBytesPreHook));
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else if (_kernel == Kernel.Windows8_0)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            //null reroutes to patch and make it jump straight to end
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x29, BitConverter.GetBytes(_origBytesPreHook));
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else 
            //{
                thread = new Thread(() =>
                {
                    try
                    {
                        //null reroutes to patch and make it jump straight to end
                        var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

                        exmemory.WriteBytes(pconnect + _hookOffset, BitConverter.GetBytes(_origBytesPreHook));
                    }
                    catch (Exception)
                    {
                    }
                });
            //}

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void ReHookClient()
        {
            InstallHookClient();
            return;

            Thread thread = null;
            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");
            int connectOffset = pconnect.ToInt32() + 0x18;
            if (_kernel == Kernel.Windows7)
            {
                thread = new Thread(() =>
                {
                    try
                    {
                        exmemory.WriteBytes(_alocatedMem + 20 + 10, BitConverter.GetBytes(connectOffset));
                        exmemory.WriteBytes(_alocatedMem + 20 + 20, BitConverter.GetBytes(connectOffset));
                    }
                    catch (Exception)
                    {
                    }
                });
            }
            else if (_kernel == Kernel.Windows10)
            {
                InstallHookClient();
                return;
            }
            else if (_kernel == Kernel.Windows8_1)
            {
                InstallHookClient();
                return;
            }
            else if (_kernel == Kernel.Windows8_0)
            {
                InstallHookClient();
                return;
            }

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void InstallHookClient()
        {
            //run in STAThread
            Thread thread = null;
            if (_kernel == Kernel.Windows7 && false)
            {
                thread = new Thread(() =>
                {
                    try
                    {
                        var pTlsGetValue = exmemory.GetProcAddress("KERNELBASE", "TlsGetValue");

                        exmemory.WriteBytes(pTlsGetValue + 0x5, new byte[] {0xe8});
                        exmemory.WriteBytes(pTlsGetValue + 0x6, BitConverter.GetBytes((int) _hookJumpToSize));
                        exmemory.WriteBytes(pTlsGetValue + 0xa, new byte[] {0x90});
                    }
                    catch (Exception)
                    {

                    }
                });
            }
            //else if (_kernel == Kernel.Windows10)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x22, new byte[] {0xe8});
            //            exmemory.WriteBytes(pconnect + 0x23, BitConverter.GetBytes((int) _hookJumpToSize));
            //            exmemory.WriteBytes(pconnect + 0x27, new byte[] {0x5F});
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else if (_kernel == Kernel.Windows8_1)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x23, new byte[] { 0xe8 });
            //            exmemory.WriteBytes(pconnect + 0x24, BitConverter.GetBytes((int)_hookJumpToSize));
            //            exmemory.WriteBytes(pconnect + 0x28, new byte[] { 0x5F });
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else if (_kernel == Kernel.Windows8_0)
            //{
            //    thread = new Thread(() =>
            //    {
            //        try
            //        {
            //            var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

            //            exmemory.WriteBytes(pconnect + 0x29, new byte[] { 0xe8 });
            //            exmemory.WriteBytes(pconnect + 0x2a, BitConverter.GetBytes((int)_hookJumpToSize));
            //            exmemory.WriteBytes(pconnect + 0x2e, new byte[] { 0x5F });
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    });
            //}
            //else
            //{
                thread = new Thread(() =>
                {
                    try
                    {
                        var pconnect = exmemory.GetProcAddress("WS2_32", "connect");

                        var res = exmemory.WriteBytes(pconnect + _hookOffset, new byte[] { 0xe8 });
                        if(res == 0)
                            log.Debug("Failed inject");

                        exmemory.WriteBytes(pconnect + _hookOffset+1, BitConverter.GetBytes((int)_hookJumpToSize));
                        if(_doorSize > 5)
                            exmemory.WriteBytes(pconnect + _hookOffset+5, new byte[] { 0x5F });
                        if (_doorSize > 6)
                            exmemory.WriteBytes(pconnect + _hookOffset + 6, new byte[] { 0x90 });
                        if (_doorSize > 7)
                            exmemory.WriteBytes(pconnect + _hookOffset + 7, new byte[] { 0x90 });
                    }
                    catch (Exception)
                    {
                    }
                });
            //}

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private bool? _isSGClient = null;
        public bool IsSGLoaded()
        {
            if (_isSGClient != null)
                return _isSGClient.Value;

            //Process l2 = null;
            try
            {
                //l2 = Process.GetProcessById(ProcessId);
            }
            catch (Exception e)
            {
                log.Debug(e);
                return true;
                //throw new Exception("Invalid client handle");
            }


            
            foreach (ProcessModule module in l2.Modules)
            {
                //log.Debug(module.ModuleName);
                if (string.Equals("guard.des", module.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    string path =
                    l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) +
                    "guard.des";
                    long length = new System.IO.FileInfo(path).Length;
                    if (length < 5000000)
                    {
                        _isSGClient = false;
                        return false;
                    }

                    _isSGClient = true;
                    return true;
                }
            }

            return false;
        }

        private bool? _isRpgClient = null;
        public bool IsRpgClient()
        {
            if (_isRpgClient != null)
                return _isRpgClient.Value;

            //Process l2 = null;
            try
            {
                //l2 = Process.GetProcessById(ProcessId);
            }
            catch (Exception e)
            {
                log.Debug(e);
                throw new Exception("Invalid client handle");
            }

            foreach (ProcessModule module in l2.Modules)
            {
                if (string.Equals("system.dll", module.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    _isRpgClient = true;
                    return true;
                }
            }

            _isRpgClient = false;
            return false;
        }

        public byte[] GetRpgSecretKey()
        {
            byte[] key = new byte[8];

            //run in STAThread
            Thread thread = new Thread(() =>
            {
                var uintPtr = new UIntPtr(0xEFA6914D);
                key = exmemory.ReadBytes((IntPtr)(int)(uint)uintPtr, 8);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return key;
        }

        public bool? _sgObjInit;

        public void FillSGObject(SGInGameCipher sgobj)
        {
            //run in STAThread
            Thread thread = new Thread(() =>
            {
                try
                {
                    _sgObjInit = false;
                    //LogHelper.GetLogger().Debug("1");
                    //Thread.Sleep(500);
                    //Process l2 = null;
                    //try
                    //{
                    //    l2 = Process.GetProcessById(ProcessId);
                    //}
                    //catch (Exception e)
                    //{
                    //    log.Debug(e);
                    //    throw new Exception("Invalid client handle");
                    //}

                   if(l2.HasExited)
                        return;

                    //LogHelper.GetLogger().Debug("2");
                    ProcessModule sgDll = l2.Modules[0];
                    int baseOffset = 0;
                    PropertyOffsets offsets = new PropertyOffsets();
                    BigInteger md5hash;

                    using (var md5 = MD5.Create())
                    {
                        string path = l2.MainModule.FileName.ToLower().Replace("l2.bin", String.Empty).Replace("l2.exe", String.Empty) + "guard.des";
                        using (var stream = File.OpenRead(path))
                        {
                            md5hash = new BigInteger(md5.ComputeHash(stream));
                            baseOffset = SGKey.OffsetList[md5hash.ToString()].Offset;
                            offsets = SGKey.OffsetList[md5hash.ToString()];
                        }
                    }

                    //LogHelper.GetLogger().Debug("3");
                    foreach (ProcessModule module in l2.Modules)
                    {
                        if (string.Equals("guard.des", module.ModuleName, StringComparison.OrdinalIgnoreCase))
                            sgDll = module;
                    }

                    if (!string.Equals("guard.des", sgDll.ModuleName, StringComparison.OrdinalIgnoreCase))
                        log.Debug("Protection module not found.");

                    //LogHelper.GetLogger().Debug("4");
                    l2.Suspend();
                    var rndArrAddr1 = exmemory.Read<int>(sgDll.BaseAddress + (baseOffset - 12));
                    //if (md5hash != "Њ‡±‡¦ІP©e\f—•hЯ")
                        rndArrAddr1 = sgDll.BaseAddress.ToInt32() + baseOffset + offsets.InArr;
                    //log.Debug(rndArrAddr1);
                    var rndArr1 = exmemory.ReadBytes(new IntPtr(rndArrAddr1), 256);
                    var inObjVar1 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset - 8));
                    //if (md5hash != "Њ‡±‡¦ІP©e\f—•hЯ")
                        inObjVar1 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset + offsets.InX));

                    //log.Debug(inObjVar1);
                    var inObjVar2 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset - 7));
                    //if (md5hash != "Њ‡±‡¦ІP©e\f—•hЯ")
                        inObjVar2 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset + offsets.InY));

                    //log.Debug(inObjVar2);
                    sgobj.legitKeyReceive.var1 = inObjVar1;
                    sgobj.legitKeyReceive.var2 = inObjVar2;
                    Array.Copy(rndArr1, sgobj.legitKeyReceive.ContentBytes, 256);
                    sgobj.clientKeyReceive.var1 = inObjVar1;
                    sgobj.clientKeyReceive.var2 = inObjVar2;
                    Array.Copy(rndArr1, sgobj.clientKeyReceive.ContentBytes, 256);

                    var rndArrAddr2 = exmemory.Read<int>(sgDll.BaseAddress + (baseOffset - 4));
                    //if (md5hash != "Њ‡±‡¦ІP©e\f—•hЯ")
                        rndArrAddr2 = sgDll.BaseAddress.ToInt32() + baseOffset + offsets.OutArr;

                    var rndArr2 = exmemory.ReadBytes(new IntPtr(rndArrAddr2), 256);
                    var outObjVar1 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset + offsets.OutX));
                    //log.Debug(outObjVar1);
                    var outObjVar2 = exmemory.Read<byte>(sgDll.BaseAddress + (baseOffset + offsets.OutY));
                    //log.Debug(outObjVar2);
                    sgobj.legitKeySend.var1 = outObjVar1;
                    sgobj.legitKeySend.var2 = outObjVar2;
                    Array.Copy(rndArr2, sgobj.legitKeySend.ContentBytes, 256);
                    sgobj.clientKeySend.var1 = outObjVar1;
                    sgobj.clientKeySend.var2 = outObjVar2;
                    Array.Copy(rndArr2, sgobj.clientKeySend.ContentBytes, 256);
                    l2.Resume();
                    _sgObjInit = true;
                    //LogHelper.GetLogger().Debug("5");
                }
                catch (Exception e)
                {
                    log.Debug(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            //Thread.Sleep(100);
            //thread.Join();
            //LogHelper.GetLogger().Debug("6");
        }

        
    }
}
