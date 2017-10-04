using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ronin.Network;
using Ronin.Utilities;
using ThreadState = System.Diagnostics.ThreadState;

namespace Ronin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        static bool IsElevatedPrivileges
            => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private void OnExit(object sender, ExitEventArgs e)
        {
            foreach (var server in Server.AllServers)
            {
                server.Stop();
            }

            foreach (var client in Client.AllClients)
            {
                client.Stop();
            }

            Environment.Exit(0);
        }

        System.Threading.Mutex _mutey = null;

        private static Process _antiSuspendProc;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CheckForPrerequisites();

            if (!IsElevatedPrivileges)
            {
                MessageBox.Show("The application has to be runned as administrator.");
                Environment.Exit(0);
            }

            var parent = ParentProcessUtilities.GetParentProcess();
            //MessageBox.Show(parent.ProcessName);
            if (parent != null && !parent.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase)
                && !parent.ProcessName.Equals("csvb", StringComparison.OrdinalIgnoreCase)
                && !parent.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase)
                )
            {
                bool legit = false;
#if DEBUG
               if (Debugger.IsAttached) 
                legit = true;
#endif
                if (!legit)
                {
                    //log.Debug(parent.ProcessName);
                    log.Debug("leet7");
                    Application.Current.Shutdown();
                }
            }

            var antiSuspendThread = new Thread(delegate()
            {
                while (true)
                {
                    if (_antiSuspendProc == null || _antiSuspendProc.HasExited)
                    {
                        try
                        {
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead("scr.exe"))
                                {
                                    BigInteger md5hash = new BigInteger(md5.ComputeHash(stream));
                                    //log.Debug(md5hash);
                                    if (md5hash.ToString() != "-60891429662938329776858406811495394548")
                                    {
                                        log.Debug("Corrupted files. Redownload the software.");
                                        Environment.Exit(0);
                                    }

                                    //log.Debug("z");
                                }
                            }

                            var psi = new ProcessStartInfo("scr.exe", Process.GetCurrentProcess().Id.ToString())
                            {
                                UseShellExecute = true
                            };

                            // Updater should ask for admin privileges if UAC is enabled
                            if (Environment.OSVersion.Version.Major >= 6)
                            {
                                psi.Verb = "runas";
                            }

                            _antiSuspendProc = Process.Start(psi);
                        }
                        catch (Exception e54)
                        {
                            log.Debug("Corrupted files. Redownload the software.");
                            Environment.Exit(0);
                        }
                    }

                    try
                    {
                        //if (_antiSuspendProc.Threads[0].ThreadState == ThreadState.Wait && _antiSuspendProc.Threads[0].WaitReason == ThreadWaitReason.Suspended)
                        _antiSuspendProc.Resume();
                    }
                    catch (Exception)
                    {

                    }

                    Thread.Sleep(500);
                }
            });
            antiSuspendThread.Start();

            var externalSignalThread = new Thread(delegate()
            {
                while (true)
                {
                    bool allGood = !Injector.BadStuff;
                    Thread.Sleep(30*60*1000);
                    try
                    {
                        using (var client = new WebClient())
                        {
                            if (Assembly.GetEntryAssembly().GetName().Version <=
                                new Version(client.DownloadString(new Uri("http://therealronin.com/fallback.txt"))))
                                allGood = false;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    if (!allGood)
                    {
                        log.Debug("leet1");
                        Application.Current.Shutdown();
                    }
                }
            });
            externalSignalThread.Start();

            var debugger = new Thread(delegate()
            {
                while (true)
                {
                    Thread.Sleep(1*60*1000);
                    bool allGood = !Injector.BadStuff;

                    if (!allGood)
                    {
                        log.Debug("leet");
                        Application.Current.Shutdown();
                    }
                }
            });
            debugger.Start();

            var dbCleanSignalThread = new Thread(delegate()
            {
                while (true)
                {
                    Thread.Sleep(30*60*1000);
                    bool allGood = !Injector.BadStuff;
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadString(new Uri("http://therealronin.com/dbclean.txt"));
                        }
                        allGood = false;
                    }
                    catch (Exception ex)
                    {

                    }

                    if (!allGood)
                        File.Delete("Ronin.pdb");
                }
            });
            dbCleanSignalThread.Start();

            Thread.Sleep(100);
            var checkCrackCheckThread = new Thread(delegate()
            {
                while (true)
                {
                    Thread.Sleep(60*60*1000);
                    if (!externalSignalThread.IsAlive || !debugger.IsAlive)
                    {
                        log.Debug("1337");
                        Application.Current.Shutdown();
                    }
                }
            });
            checkCrackCheckThread.Start();

            using (Process p = Process.GetCurrentProcess())
                p.PriorityClass = ProcessPriorityClass.High;

            var windowsPatcherThread = new Thread(Injector.WindowsPatcher);
            windowsPatcherThread.SetApartmentState(ApartmentState.STA);
            windowsPatcherThread.Start();

           

            try
            {
                System.Threading.Mutex.OpenExisting("WPFDefault");
                // myApp is already running...
                MessageBox.Show("Application is already running!");
                Environment.Exit(0);
            }
            catch (Exception)
            {
                _mutey = new System.Threading.Mutex(true, "WPFDefault");
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleCriticalCrash);
            Application.Current.DispatcherUnhandledException += (o, args) =>
            {
                Exception ex = (Exception) args.Exception;
                log.Debug("UI exception: ");
                log.Debug(ex);
                //throw ex;
            };

            var thread = new Thread(checkForUpdates);
            thread.Start();
        }

        private void HandleCriticalCrash(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception) args.ExceptionObject;
            log.Debug("Unhandled exception: ");
            log.Debug(e);
        }

        private void CheckForPrerequisites()
        {
            bool isNet461 = typeof(ProcessStartInfo).GetProperty("PasswordInClearText") != null;
            if (!isNet461)
            {
                MessageBoxResult result = MessageBox.Show(
                    string.Format(
                        "This application requires one of the following versions of the .NET Framework:{0}  .NETFramework,Version=v4.6.1{0}{0}Do you want to download this .NET Framework version now?",
                        Environment.NewLine), "This application could not be started",
                    MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(
                        "https://www.microsoft.com/en-us/download/details.aspx?id=49981");
                }

                Application.Current.Shutdown(12);
            }

            try
            {
                AssemblyName greyMagic =
                    Assembly.GetExecutingAssembly()
                        .GetReferencedAssemblies()
                        .First(a => string.Equals(a.Name, "GreyMagic", StringComparison.OrdinalIgnoreCase));

                Assembly.Load(greyMagic);
            }
            catch (Exception ex)
            {
                // Missing vcredist
                if (ex is FileNotFoundException || ex is BadImageFormatException)
                {
                    MessageBoxResult result = MessageBox.Show(
                        string.Format(
                            "This application requires one of the following versions of the Visual C++ Redistributable Package for:{0}   Visual Studio 2015 Update 1 x86, VC++ 14.0{0}{0}Do you want to download this Visual C++ Redistributable Package now?{0}{0}Note: You must download and install the x86 version regardless of your operating system",
                            Environment.NewLine), "This application could not be started",
                        MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(
                            "https://www.microsoft.com/en-us/download/details.aspx?id=48145");
                    }

                    Application.Current.Shutdown(12);
                }
                // Version mismatch or bad installation
                else if (ex is FileLoadException)
                {
                    MessageBox.Show(
                        string.Format(
                            "This application could not be started because required files are either missing or corrupted.{0}Download the application again and perform a clean installation in another folder.{0}{0} Please contact us if the issue persists.",
                            Environment.NewLine),
                        "This application could not be started", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    Application.Current.Shutdown(12);
                }

                throw;
            }
        }

        private void checkForUpdates()
        {
            try
            {
                Version version;
                var client = new WebClient();
                version = new Version(client.DownloadString(new Uri("http://therealronin.com/version.txt")));

                log.Debug($"Version successfully queued: {version}");
                if (Assembly.GetEntryAssembly().GetName().Version != version)
                {
                    var psi = new ProcessStartInfo("csvb.exe", Assembly.GetEntryAssembly().GetName().Version.ToString())
                    {
                        UseShellExecute = true
                    };

                    // Updater should ask for admin privileges if UAC is enabled
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        psi.Verb = "runas";
                    }

                    log.Debug("A new version detected.");
                    MessageBoxResult result =
                        MessageBox.Show(
                            "A new version is available for download. Would you like to update?" + Environment.NewLine +
                            Environment.NewLine +
                            "Note: Application will be closed for the update. Therefore all characters will be disconnected.",
                            "L2 Ronin - Update", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.GetProcessesByName("scr")[0].Kill();
                        }
                        catch (Exception)
                        {
                            
                        }

                        File.Delete("Ronin.pdb");
                        File.Delete("XMLParser.pdb");
                        Process.Start(psi);
                        log.Debug("Closing for update...");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    log.Info("Bot is up to date.");
                }
            }
            catch (Exception e)
            {
                log.Warn(e.ToString());
                log.Warn("Unsuccessful check for updates.");
            }
        }
    }
}
