using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GreyMagic;
using System.IO.Pipes;
using SharpDisasm;

namespace Ronin.Utilities
{
    public class Injectora
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        public static bool ShouldInjectInNextClient = true;
        private Process _procHandle;
        private GreyMagic.ExternalProcessMemory _exmemory;
        private NamedPipeServerStream pipeRecvPackets;
        private NamedPipeServerStream pipeSendActions;
        private NamedPipeServerStream pipeRecvOutgoingPackets;

        private StreamReader RecvPacketsRead;
        private StreamWriter SendActionsSend;
        private StreamReader RecvOutgoingPacketsRead;

        public Injectora(Process procHandle)
        {
            _procHandle = procHandle;
            _exmemory = new ExternalProcessMemory(procHandle, false, false, false);
        }

        

        [STAThread]
        public static void RerouteL2s()
        {
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
                                            var injector = new Injectora(l2);


                                            injector.Inject();
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

                Thread.Sleep(1000);
            }
        }

        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(

         IntPtr hProcess,

         IntPtr lpThreadAttributes,

         uint dwStackSize,

         IntPtr lpStartAddress, // raw Pointer into remote process

         IntPtr lpParameter,

         uint dwCreationFlags,

         out uint lpThreadId

       );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
            int dwSize, uint flNewProtect, out uint lpflOldProtect);

        public void Inject()
        {
            //var dll = File.ReadAllBytes("E:\\Visual Studio Projects\\Rerouter\\Debug\\Rerouter.dll");

            while (!_procHandle.HasExited)
            {
                //PC = new PerformanceCounter();
                //PC.CategoryName = "Process";
                //PC.CounterName = "Working Set - Private";
                //PC.InstanceName = l2.ProcessName;
                //PC.Close();
                //PC.Dispose();
                //memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
                var memsize = MemoryInformation.GetMemoryUsageForProcess(_procHandle.Id);

                if (memsize > 250000000)
                    break;

                //log.Debug(memsize);
                Thread.Sleep(100);
            }
            
            //MessageBox.Show("C:\\Users\\Crepto\\Downloads\\RemoteDLLInjector\\RemoteDLLInjector\\RemoteDLLInjector32.exe" + _procHandle.Id.ToString() + " E:\\Visual Studio Projects\\Rerouter\\Debug\\Rerouter.dll");
            var psi = new ProcessStartInfo("C:\\Users\\Crepto\\Downloads\\RemoteDLLInjector\\RemoteDLLInjector\\RemoteDLLInjector32.exe", _procHandle.Id.ToString() + " \"E:\\Visual Studio Projects\\Rerouter\\Debug\\Rerouter.dll\"")
            //var psi = new ProcessStartInfo("C:\\Users\\Crepto\\Downloads\\RemoteDLLInjector\\RemoteDLLInjector\\ProcessHacker.exe", "-c -ctype process -cobject " + _procHandle.Id.ToString() + " -caction injectdll -cvalue \"E:\\Visual Studio Projects\\Rerouter\\Debug\\Rerouter.dll\"")
            {
                UseShellExecute = true
            };
            if (Environment.OSVersion.Version.Major >= 6)
            {
                psi.Verb = "runas";
            }

            Process.Start(psi);
            

            //UdpClient client;
            //while (true)
            //{
            //    try
            //    {
            //        Thread.Sleep(100);
            //        client = new UdpClient("127.0.0.1", 4567);
            //        break;
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}

            var dllBase = IntPtr.Zero;
            while (dllBase == IntPtr.Zero)
            {
                foreach (ProcessModule module in Process.GetProcessById(_procHandle.Id).Modules)
                {
                    if (string.Equals("Rerouter.dll", module.ModuleName, StringComparison.OrdinalIgnoreCase))
                    {
                        dllBase = module.BaseAddress;
                    }
                }
                Thread.Sleep(500);
            }


            var pkajinazarejdacha = _exmemory.GetProcAddress("rerouter.dll", "KajiNaZarejdacha");

            uint old;
            VirtualProtectEx(_procHandle.Handle, dllBase, 0x350, 0x40, out old);
            _exmemory.WriteBytes(dllBase, new byte[0x350]);

            var pserverpacketcount = _exmemory.GetProcAddress("engine.dll", "?ServerPacketCountStart@UNetworkHandler@@UAEXXZ");

            var memory = _exmemory.AllocateMemory(100);

            byte[] stubforcommunication =
            {
                0x60,//pushad
                0x9C,//pushfd
                0xFF, 0x74, 0x24, 0x34,//push [esp+34]
                0xFF, 0x35, 0x34, 0x12, 0x34, 0x21,//push [rerouter.dll]
                0xE8, 0x01, 0x23, 0x23, 0x10,//call rerouter.KajiNaZarejdacha
                0x83, 0xC4, 0x08,//add esp,8
                0x9D,//popad
                0x61,//popfd
                0xE9, 0x08, 0x43, 0x24, 0xFE//ret jmp
            };



            byte[] stubrecvreroute =
            {
                0x81, 0x7D, 0x04, 0x50, 0x8A, 0x3D, 0x20,//cmp [ebp+04],203D8A50
                0x75, 0x07,//jne legit
                0xC7, 0x45, 0x04, 0x00, 0x0, 0x3D, 0x00,//mov [ebp+00],003D0000
                0x83, 0xc4,0x04,
                0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,//placeholder for hooked bytes                
                0xE9, 0x0B, 0x6B, 0x22, 0x73
            };

            byte[] stubcritsectionreroute =
            {
                0x81, 0x7D, 0x04, 0x87, 0x9A, 0x3D, 0x20,//cmp [ebp+04],203d9a87
                //0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,//cmp [ebp+04],203d9a87
                0x75, 0x28,//jne legit
                //0xEB, 0x2b,//jne legit

                //save ecx
                0x50,//push eax
                0x8B, 0x84, 0x24, 0x20, 0x80, 0x00, 0x00,//mov eax, [esp+8020]
                0xA3, 0x12, 0x23, 0x31, 0x12,//mov [rerouter.dll+12], eax
                0x58,//pop eax

                0x60,//pushad
                0x9C,//pushfd
                //0xFF, 0xB4, 0x24, 0x44, 0x80, 0x00, 0x00,//push [esp+8044]
                0x8b,0xc4,//mov eax,esp
                0x05, 0x44, 0x80, 0x00, 0x00,//add eax,8044
                0x50, //push eax  
                0xFF, 0x35, 0x34, 0x12, 0x34, 0x21,//push [rerouter.dll+8]
                0xE8, 0x01, 0x23, 0x23, 0x10,//call rerouter.KajiNaZarejdacha2
                0x83, 0xC4, 0x08,//add esp,8
                0x9D,//popad
                0x61,//popfd

                0x83, 0xc4,0x4,//add esp,4
                0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,//placeholder for hooked bytes                
                0xE9, 0x0B, 0x6B, 0x22, 0x73//ret jmp
            };

            var legitreturn = pserverpacketcount + 0x155a;

            byte[] stubenginecopy =
            {
                0x8B ,0x96 ,0x7C ,0x91 ,0x00 ,0x00 ,0x8B ,0xF8 ,0xA1 ,0x04 ,0x58 ,0x8F ,0x20 ,0x8D ,0x0C ,0x90 ,0x0F ,0x31 ,0x83 ,0xC0 ,0xF4 ,0x01 ,0x01 ,0x3B ,0xFD ,0x0F ,0x8E ,0x0C ,0xFF 
                ,0xFF ,0xFF ,0x8D ,0xA4 ,0x24 ,0x00 ,0x00 ,0x00 ,0x00 ,0x8B ,0x4C ,0x24 ,0x1C ,0x8B ,0x01 ,0x83 ,0xE8 ,0x00 ,0x0F ,0x84 ,0xB5 ,0x00 ,0x00 ,0x00 ,0x83 ,0xE8 ,0x01 ,0x0F ,0x85
                ,0x4B ,0x01 ,0x00 ,0x00 ,0x8B ,0x96 ,0x7C ,0x91 ,0x00 ,0x00 ,0xA1 ,0x04 ,0x58 ,0x8F ,0x20 ,0x8D ,0x0C ,0x90 ,0x0F ,0x31 ,0x29 ,0x01 ,0x8B ,0x6C ,0x24 ,0x14 ,0x8B ,0x0B ,0x8B 
                ,0x45 ,0x00 ,0x2B ,0xC1 ,0x3B ,0xC7 ,0x7C ,0x02 ,0x8B ,0xC7 ,0x8B ,0x54 ,0x24 ,0x24 ,0x50 ,0x8D ,0x82 ,0xC0 ,0x6A ,0x73 ,0x20 ,0x8B ,0x54 ,0x24 ,0x14 ,0x50 ,0x03 ,0xCA ,0x51 
                ,0xE8 ,0x2D ,0x01 ,0xDA ,0xFF 
                ,0x8B ,0x4D ,0x00 ,0x2B ,0x0B ,0x83 ,0xC4 ,0x0C ,0x3B ,0xCF ,0x7C ,0x02 ,0x8B ,0xCF ,0x8B ,0x86 ,0x7C ,0x91 ,0x00 ,0x00 ,0x8B ,0x15 ,0x04 ,0x58 
                ,0x8F ,0x20 ,0x01 ,0x4C ,0x24 ,0x24 ,0x2B ,0xF9 ,0x8D ,0x2C ,0x82 ,0x0F ,0x31 ,0x83 ,0xC0 ,0xF4 ,0x01 ,0x45 ,0x00 ,0x85 ,0xC9 ,0x7E ,0x06 ,0x01 ,0x0B ,0x01 ,0x4C ,0x24 ,0x18 
                ,0x8B ,0x44 ,0x24 ,0x14 ,0x8B ,0x00 ,0x39 ,0x03 ,0x0F ,0x85 ,0xCD ,0x00 ,0x00 ,0x00 ,0x8B ,0x16 ,0x8B ,0x52 ,0x78 ,0x50 ,0x8B ,0x44 ,0x24 ,0x14 ,0x50 ,0x8B ,0xCE ,0xFF ,0xD2 
                ,0x8B ,0x44 ,0x24 ,0x14 ,0x8B ,0x4C ,0x24 ,0x1C ,0xC7 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xC7 ,0x03 ,0x00 ,0x00 ,0x00 ,0x00 ,0xC7 ,0x01 ,0x00 ,0x00 ,0x00 ,0x00 ,0xE9 ,0x9F ,0x00 
                ,0x00 ,0x00 ,0x8B ,0x96 ,0x7C ,0x91 ,0x00 ,0x00 ,0xA1 ,0x04 ,0x58 ,0x8F ,0x20 ,0x8D ,0x0C ,0x90 ,0x0F ,0x31 ,0x29 ,0x01 ,0x8B ,0x0B ,0xB8 ,0x02 ,0x00 ,0x00 ,0x00 ,0x2B ,0xC1
                ,0x3B ,0xC7 ,0x7C ,0x02 ,0x8B ,0xC7 ,0x8B ,0x54 ,0x24 ,0x24 ,0x50 ,0x8D ,0x82 ,0xC0 ,0x6A ,0x73 ,0x20 ,0x8B ,0x54 ,0x24 ,0x14 ,0x50 ,0x03 ,0xCA ,0x51 ,
                0xE8 ,0x83 ,0x00 ,0xDA ,0xFF //call in engine
                ,0xB9 ,0x02 ,0x00 ,0x00 ,0x00 ,0x2B ,0x0B ,0x83 ,0xC4 ,0x0C ,0x3B ,0xCF ,0x7C ,0x02 ,0x8B ,0xCF ,0x8B ,0x86 ,0x7C ,0x91 ,0x00 ,0x00 ,0x8B ,0x15 ,0x04 ,0x58 ,0x8F ,0x20 
                ,0x01 ,0x4C ,0x24 ,0x24 ,0x2B ,0xF9 ,0x8D ,0x2C ,0x82 ,0x0F ,0x31 ,0x83 ,0xC0 ,0xF4 ,0x01 ,0x45 ,0x00 ,0x85 ,0xC9 ,0x7E ,0x06 ,0x01 ,0x0B ,0x01 ,0x4C ,0x24 ,0x18 ,0x83 ,0x3B 
                ,0x02 ,0x75 ,0x2A ,0x8B ,0x44 ,0x24 ,0x10 ,0x66 ,0x8B ,0x08 ,0x8B ,0x44 ,0x24 ,0x14 ,0x66 ,0x89 ,0x08 ,0x83 ,0x38 ,0x02 ,0x7D ,0x0D ,0x68 ,0x28 ,0x96 ,0x51 ,0x20 ,
                0xE8 ,0x1A ,0xB6 ,0xFF ,0xFF 
                ,0x83 ,0xC4 ,0x04 ,0x8B ,0x54 ,0x24 ,0x1C ,0xC7 ,0x02 ,0x01 ,0x00 ,0x00 ,0x00 ,0x85 ,0xFF ,
                0x0F ,0x8F ,0x95 ,0xFE ,0xFF ,0xFF
            };

            

            //Thread.Sleep(700);
            var pipe1 = _procHandle.Id ^ 0x4444;
            pipeRecvPackets = new NamedPipeServerStream(pipe1.ToString(), PipeDirection.In, 4, PipeTransmissionMode.Message, PipeOptions.WriteThrough);

            var pipe3 = _procHandle.Id ^ 0x6666;
            pipeRecvOutgoingPackets = new NamedPipeServerStream(pipe3.ToString(), PipeDirection.In, 4, PipeTransmissionMode.Message, PipeOptions.WriteThrough);

            RecvPacketsRead = new StreamReader(pipeRecvPackets);
            RecvOutgoingPacketsRead = new StreamReader(pipeRecvOutgoingPackets);


            //Thread.Sleep(3000);

            var pipe2 = _procHandle.Id ^ 0x5555;
            pipeSendActions = new NamedPipeServerStream(pipe2.ToString(), PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough);

            SendActionsSend = new StreamWriter(pipeSendActions);

            pipeSendActions.WaitForConnection();
            PacketBuilder pb = new PacketBuilder();
            MemoryStream data = new MemoryStream();
            BinaryWriter buff = new BinaryWriter(data);
            buff.Write(Encoding.ASCII.GetBytes("cSd"));
            buff.Write((byte)0);
            buff.Write((byte)0x49);
            buff.Write((short)("ez katka".Length));
            buff.Write(Encoding.ASCII.GetBytes("ez katka"));
            buff.Write(0);

            var sad = data.GetBuffer();
            //pipeSendActions.Write(data.GetBuffer(), 0, (int)data.Length);

            var thread = new Thread(receiveIncomingPacketsFromDLL);
            thread.Start();

            thread = new Thread(receiveOutgoingPacketsFromDLL);
            thread.Start();

            var precv = _exmemory.GetProcAddress("WS2_32", "recv");
            var procedure = _exmemory.ReadBytes(precv+5, 100);

            var pcritsection = _exmemory.GetProcAddress("ntdll", "RtlEnterCriticalSection");
            var procedure2 = _exmemory.ReadBytes(pcritsection + 5, 100);

            var disasm = new Disassembler(
                procedure,
                ArchitectureMode.x86_32, 0, true);

            var _hookOffset = 0;
            var curOffset = 0;

            //instruction for recv
            byte[] hookedInstructions = new byte[15];
            int hookedInstructionsSize = 0;
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
                for (int i = 0; i < instructions.Count && suitableBytes < 5; i++)
                {
                    curOffset += instructions[i].Length;
                    int count = 0;
                    while (i >= 0 && suitableBytes < 5)
                    {
                        //if (instructions[i + count].ToString().StartsWith("j") || instructions[i + count].ToString().StartsWith("call"))
                        //{
                        //    suitableBytes = 0;
                        //    hookStub.Clear();
                        //    break;
                        //}

                        hookStub.Add(instructions[i + count]);
                        suitableBytes += instructions[i + count].Length;
                        count++;

                        if (suitableBytes > 8)
                        {
                            suitableBytes = 0;
                            hookStub.Clear();
                            break;
                        }
                    }

                    if (suitableBytes >= 5)
                        _hookOffset = curOffset - instructions[i].Length;
                }

                foreach(var instr in hookStub)
                {
                    for (int i = 0; i < instr.Length; i++)
                    {
                        hookedInstructions[i + hookedInstructionsSize] = instr.Bytes[i];
                    }

                    hookedInstructionsSize += instr.Bytes.Length;
                }

                //write the overwritten execution in the stub
                for (int i = 0; i < hookedInstructionsSize; i++)
                {
                    stubrecvreroute[19 + i] = hookedInstructions[i];
                }

                //instructions for crit section API
                disasm = new Disassembler(
                procedure2,
                ArchitectureMode.x86_32, 0, true);

                hookedInstructions = new byte[15];
                hookedInstructionsSize = 0;
                hookStub = new List<Instruction>();
                if (_hookOffset == 0)
                {
                    curOffset += 0;
                    instructions = new List<Instruction>();
                    foreach (var instruction in disasm.Disassemble())
                    {
                        instructions.Add(instruction);
                    }

                    suitableBytes = 0;
                    for (int i = 0; i < instructions.Count && suitableBytes < 5; i++)
                    {
                        curOffset += instructions[i].Length;
                        int count = 0;
                        while (i >= 0 && suitableBytes < 5)
                        {
                            //if (instructions[i + count].ToString().StartsWith("j") || instructions[i + count].ToString().StartsWith("call"))
                            //{
                            //    suitableBytes = 0;
                            //    hookStub.Clear();
                            //    break;
                            //}

                            hookStub.Add(instructions[i + count]);
                            suitableBytes += instructions[i + count].Length;
                            count++;

                            if (suitableBytes > 8)
                            {
                                suitableBytes = 0;
                                hookStub.Clear();
                                break;
                            }
                        }

                        if (suitableBytes >= 5)
                            _hookOffset = curOffset - instructions[i].Length;
                    }

                    foreach (var instr in hookStub)
                    {
                        for (int i = 0; i < instr.Length; i++)
                        {
                            hookedInstructions[i + hookedInstructionsSize] = instr.Bytes[i];
                        }

                        hookedInstructionsSize += instr.Bytes.Length;
                    }

                    //write the overwritten execution in the stub
                    for (int i = 0; i < hookedInstructionsSize; i++)
                    {
                        stubcritsectionreroute[52 + i] = hookedInstructions[i];
                    }
                }

                stubrecvreroute[3] = BitConverter.GetBytes((pserverpacketcount + 0x155a).ToInt32())[0];
                stubrecvreroute[4] = BitConverter.GetBytes((pserverpacketcount + 0x155a).ToInt32())[1];
                stubrecvreroute[5] = BitConverter.GetBytes((pserverpacketcount + 0x155a).ToInt32())[2];
                stubrecvreroute[6] = BitConverter.GetBytes((pserverpacketcount + 0x155a).ToInt32())[3];

                var mem2 = _exmemory.AllocateMemory(1000);
                stubrecvreroute[12] = BitConverter.GetBytes((mem2).ToInt32())[0];
                stubrecvreroute[13] = BitConverter.GetBytes((mem2).ToInt32())[1];
                stubrecvreroute[14] = BitConverter.GetBytes((mem2).ToInt32())[2];
                stubrecvreroute[15] = BitConverter.GetBytes((mem2).ToInt32())[3];

                long jumpback = (precv.ToInt32() + 0xb + 0x100000000) - (memory.ToInt32() + 36);
                stubrecvreroute[32] = BitConverter.GetBytes((jumpback))[0];
                stubrecvreroute[33] = BitConverter.GetBytes((jumpback))[1];
                stubrecvreroute[34] = BitConverter.GetBytes((jumpback))[2];
                stubrecvreroute[35] = BitConverter.GetBytes((jumpback))[3];


                //fix calls within the stub
                var pisapawn = _exmemory.GetProcAddress("engine.dll", "?IsAPawn@APawn@@UAEHXZ");
                var pgetserverid = _exmemory.GetProcAddress("engine.dll", "?GetServerID@UNetworkHandler@@UAEHXZ");

                var ptopawnoffsetfunc = pisapawn + 0x17a80;
                long distancefromfirstcall = (ptopawnoffsetfunc.ToInt32() + 0x100000000) - (mem2.ToInt32() + 121);
                stubenginecopy[117] = BitConverter.GetBytes((distancefromfirstcall))[0];
                stubenginecopy[118] = BitConverter.GetBytes((distancefromfirstcall))[1];
                stubenginecopy[119] = BitConverter.GetBytes((distancefromfirstcall))[2];
                stubenginecopy[120] = BitConverter.GetBytes((distancefromfirstcall))[3];

                long distancefromsecondcall = (ptopawnoffsetfunc.ToInt32() + 0x100000000) - (mem2.ToInt32() + 291);
                stubenginecopy[287] = BitConverter.GetBytes((distancefromsecondcall))[0];
                stubenginecopy[288] = BitConverter.GetBytes((distancefromsecondcall))[1];
                stubenginecopy[289] = BitConverter.GetBytes((distancefromsecondcall))[2];
                stubenginecopy[290] = BitConverter.GetBytes((distancefromsecondcall))[3];

                _exmemory.WriteBytes(memory, stubrecvreroute);
                _exmemory.WriteBytes(mem2, stubenginecopy);

                long jumptosize = (memory.ToInt32() + 0x100000000) - precv.ToInt32() - 10;
                _exmemory.WriteBytes(precv+5, new byte[] { 0xe8 });
                _exmemory.WriteBytes(precv+6, BitConverter.GetBytes((int)jumptosize));

                pkajinazarejdacha = dllBase + 0x14FD0;
                long distancetocallzarejdacha = (pkajinazarejdacha.ToInt32() + 0x100000000) - (mem2.ToInt32() + stubenginecopy.Length - 6 + 17);
                stubforcommunication[13] = BitConverter.GetBytes((distancetocallzarejdacha))[0];
                stubforcommunication[14] = BitConverter.GetBytes((distancetocallzarejdacha))[1];
                stubforcommunication[15] = BitConverter.GetBytes((distancetocallzarejdacha))[2];
                stubforcommunication[16] = BitConverter.GetBytes((distancetocallzarejdacha))[3];

                stubforcommunication[8] = BitConverter.GetBytes((dllBase.ToInt32()))[0];
                stubforcommunication[9] = BitConverter.GetBytes((dllBase.ToInt32()))[1];
                stubforcommunication[10] = BitConverter.GetBytes((dllBase.ToInt32()))[2];
                stubforcommunication[11] = BitConverter.GetBytes((dllBase.ToInt32()))[3];

                var plandtarget = pserverpacketcount + 0x16e5;
                long distancetolegitenginestub = (plandtarget.ToInt32() + 0x100000000) - (mem2.ToInt32() + stubenginecopy.Length - 6 + stubforcommunication.Length);

                stubforcommunication[stubforcommunication.Length - 4] = BitConverter.GetBytes((distancetolegitenginestub))[0];
                stubforcommunication[stubforcommunication.Length - 3] = BitConverter.GetBytes((distancetolegitenginestub))[1];
                stubforcommunication[stubforcommunication.Length - 2] = BitConverter.GetBytes((distancetolegitenginestub))[2];
                stubforcommunication[stubforcommunication.Length - 1] = BitConverter.GetBytes((distancetolegitenginestub))[3];

                _exmemory.WriteBytes(mem2 + stubenginecopy.Length - 6, stubforcommunication);

                var mem3 = _exmemory.AllocateMemory(100);

                stubcritsectionreroute[18] = BitConverter.GetBytes((dllBase.ToInt32() + 12))[0];
                stubcritsectionreroute[19] = BitConverter.GetBytes((dllBase.ToInt32() + 12))[1];
                stubcritsectionreroute[20] = BitConverter.GetBytes((dllBase.ToInt32() + 12))[2];
                stubcritsectionreroute[21] = BitConverter.GetBytes((dllBase.ToInt32() + 12))[3];

                stubcritsectionreroute[35] = BitConverter.GetBytes((dllBase.ToInt32() + 8))[0];
                stubcritsectionreroute[36] = BitConverter.GetBytes((dllBase.ToInt32() + 8))[1];
                stubcritsectionreroute[37] = BitConverter.GetBytes((dllBase.ToInt32() + 8))[2];
                stubcritsectionreroute[38] = BitConverter.GetBytes((dllBase.ToInt32() + 8))[3];

                _procHandle.Suspend();
                var pkajinazarejdacha2 = dllBase + 0x14810;
                long distancetocallzarejdacha2 = (pkajinazarejdacha2.ToInt32() + 0x100000000) - (mem3.ToInt32() + 44);
                stubcritsectionreroute[40] = BitConverter.GetBytes((distancetocallzarejdacha2))[0];
                stubcritsectionreroute[41] = BitConverter.GetBytes((distancetocallzarejdacha2))[1];
                stubcritsectionreroute[42] = BitConverter.GetBytes((distancetocallzarejdacha2))[2];
                stubcritsectionreroute[43] = BitConverter.GetBytes((distancetocallzarejdacha2))[3];

                jumptosize = (mem3.ToInt32() + 0x100000000) - pcritsection.ToInt32() - 10;
                //_exmemory.WriteBytes(pcritsection + 5, new byte[] { 0xe8 });
                //_exmemory.WriteBytes(pcritsection + 6, BitConverter.GetBytes((int)jumptosize));
                //_exmemory.WriteBytes(pcritsection - 1, new byte[] { 0xe8 });

                plandtarget = pcritsection + 5 + hookedInstructionsSize;
                distancetolegitenginestub = (plandtarget.ToInt32() + 0x100000000) - (mem3.ToInt32() + stubcritsectionreroute.Length);

                stubcritsectionreroute[stubcritsectionreroute.Length - 4] = BitConverter.GetBytes((distancetolegitenginestub))[0];
                stubcritsectionreroute[stubcritsectionreroute.Length - 3] = BitConverter.GetBytes((distancetolegitenginestub))[1];
                stubcritsectionreroute[stubcritsectionreroute.Length - 2] = BitConverter.GetBytes((distancetolegitenginestub))[2];
                stubcritsectionreroute[stubcritsectionreroute.Length - 1] = BitConverter.GetBytes((distancetolegitenginestub))[3];

                _exmemory.WriteBytes(mem3, stubcritsectionreroute);
                _procHandle.Resume();
            }
        }

        void receiveIncomingPacketsFromDLL()
        {
            pipeRecvPackets.WaitForConnection();
            do
            {
                try
                {
                    if (!pipeRecvPackets.IsConnected)
                        pipeRecvPackets.WaitForConnection();
                    string test;
                    //pipeRecvPackets.WaitForPipeDrain();
                    test = RecvPacketsRead.ReadLine();
                    log.Debug("I "+test);
                }

                catch (Exception ex)
                {
                //    throw ex;
                    break;
                }
            } while (true);
        }

        void receiveOutgoingPacketsFromDLL()
        {
            pipeRecvOutgoingPackets.WaitForConnection();
            do
            {
                try
                {
                if (!pipeRecvOutgoingPackets.IsConnected)
                    pipeRecvOutgoingPackets.WaitForConnection();
                string test;
                //pipeRecvPackets.WaitForPipeDrain();
                test = RecvOutgoingPacketsRead.ReadLine();
                log.Debug("O " + test);
                }

                catch (Exception ex)
                {
                //    throw ex;
                    break;
                }
            } while (true);
        }
    }

}
