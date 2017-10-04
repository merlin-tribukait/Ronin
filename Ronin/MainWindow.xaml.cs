using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Cryptlex;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Logic.Handlers;
using Ronin.Network;
using Ronin.Protocols;
using Ronin.Protocols.HighFive;
using Ronin.Utilities;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Ronin.exe.config", Watch = true)]

namespace Ronin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static bool legit;
        public static Thread CrackCheckThread = null;

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public MainWindow()
        {
            ExportedData.Init();
            AutoScroll = true;
            
            Application.Current.DispatcherUnhandledException += (o, args) =>
            {
                Exception ex = (Exception) args.Exception;
                log.Debug("UI exception: ");
                log.Debug(ex);
                //throw ex;
            };

            int status;
            legit = false;

            try
            {
                LexActivator.SetGracePeriodForNetworkError(0);
                LexActivator.SetDayIntervalForServerCheck(1);

            }
            catch (Exception e)
            {
                log.Debug(e);
            }

            status = LexActivator.SetProductFile("Product.dat");
            if (status != LexActivator.LA_OK)
            {
                MessageBox.Show("Corrupted files! Please redownload the software.");
                Environment.Exit(0);
            }

            status = LexActivator.SetVersionGUID("014FF53D-5C6C-5266-7A89-E9601F37F5B1",
                LexActivator.PermissionFlags.LA_USER);
            if (status != LexActivator.LA_OK)
            {
                MessageBox.Show("Corrupted data!");
                Environment.Exit(0);
            }

            LexActivator.ActivateProduct();
            status = LexActivator.IsProductGenuine();
            if (status == LexActivator.LA_OK || status == LexActivator.LA_GP_OVER)
            {
                legit = true;
            }

            status = LexActivator.IsTrialGenuine();
            if (status == LexActivator.LA_OK)
            {
                legit = true;
                uint daysLeft = 0;
                LexActivator.GetTrialDaysLeft(ref daysLeft, LexActivator.TrialType.LA_V_TRIAL);
                MessageBox.Show($"Trial days left: {daysLeft}");
            }

            else if (status == LexActivator.LA_T_EXPIRED && !legit)
            {
                MessageBox.Show("Trial has expired!");
            }

            if (!legit)
            {
                var _loginForm = new LoginForm();
                _loginForm.ShowDialog();
            }

            if (!legit)
            {
                MessageBox.Show("Failed atuh.");
                Environment.Exit(0);
            }

            pSelf = this;
            ViewModel = new ViewModel();
            this.DataContext = ViewModel;
            //var bot = new L2Bot(new Injector(23, 123));
            //bot.Engine.Init(bot.PlayerData,
            //    new H5ActionsController(bot.PlayerData,
            //        new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))));
            //bot.PlayerData.Skills.Add(123, new Skill() {SkillId = 22});

            InitializeComponent();
            //LogHelper.GetLogger($"\\ RONIN .cs").Info($"L2 Ronin BETA {Assembly.GetEntryAssembly().GetName().Version}");
            versionLabel.Text = $"L2 Ronin BETA Release v{Assembly.GetEntryAssembly().GetName().Version}";
            this.Title = RandomString(10);
            //log.Debug(Title);

            

            CrackCheckThread = new Thread(delegate()
            {
                while (true)
                {
                    Thread.Sleep(30 * 60 * 1000);
                    LexActivator.ActivateProduct();
                    status = LexActivator.IsProductGenuine();
                    if (status != LexActivator.LA_OK && status != LexActivator.LA_GP_OVER)
                    {
                        Environment.Exit(0);
                    }
                }
            });
            CrackCheckThread.Start();

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseDoubleClickEvent, 
                new MouseButtonEventHandler(ListBox_MouseDoubleClick)));
            BotsList.ItemContainerStyle = itemContainerStyle;

            log.Debug(Assembly.GetEntryAssembly().GetName().Version);
            log.Debug(getOSInfo());
            string OStype = "";
            if (Environment.Is64BitOperatingSystem) { OStype = "64-Bit, "; } else { OStype = "32-Bit, "; }
            OStype += Environment.ProcessorCount.ToString() + " Processor";
            log.Debug(OStype);


            var l2rerouterThread = new Thread(Injectora.RerouteL2s);
            l2rerouterThread.SetApartmentState(ApartmentState.STA);
            l2rerouterThread.Start();

            //Hierarchy h = (Hierarchy)LogManager.GetRepository();
            //h.Root.Level = Level.All;
            //h.Root.AddAppender(new TextBoxAppender());
            //h.Configured = true;
        }


        public string getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "Windows 2000";
                        else
                            operatingSystem = "Windows XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Windows Vista";
                        else if(vs.Minor == 1)
                            operatingSystem = "Windows 7";
                        else if (vs.Minor == 2)
                            operatingSystem = "Windows 8.0";
                        else if (vs.Minor == 3)
                            operatingSystem = "Windows 8.1";
                        break;
                    case 10:
                        operatingSystem = "Windows 10";
                        break;
                    default:
                        break;
                }

                if(Injector.IsWindows10())
                    operatingSystem = "Windows 10";
            }

            return operatingSystem;
        }

        private static readonly log4net.ILog log = LogHelper.GetLogger();
        public static MainWindow pSelf;
        public static ViewModel ViewModel;

        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (Scroller.VerticalOffset == Scroller.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight);
            }
        }

        public bool AutoScroll { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfigurationWindow.Opened)
            {
                var cfgwdw = new ConfigurationWindow(this.DataContext);
                if (ViewModel.SelectedBot?.PlayerData?.MainHero?.Name != null)
                    cfgwdw.Title = ViewModel.SelectedBot?.PlayerData?.MainHero?.Name;

                Dispatcher.BeginInvoke((Action) (() => cfgwdw.Show()));
            }
            else
            {
                if(ConfigurationWindow.Instance.WindowState == WindowState.Minimized)
                    ConfigurationWindow.Instance.WindowState = WindowState.Normal;

                ConfigurationWindow.Instance.Activate();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(ViewModel.SelectedBot?.Engine == null)
                return;

            if(!ViewModel.SelectedBot.Engine.Running)
                ViewModel.SelectedBot.Engine.Start();
            else
            {
                ViewModel.SelectedBot.Engine.Abort();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModel.Bots.Any(bot => bot.PlayerData.GameState!= GameState.AccountLogin))
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "Upon closure of the program - all of the injected clients will be disconected. Continue?",
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            foreach (var bot in ViewModel.Bots)
            {
                bot?.RestoreInitialState();
            }

            Environment.Exit(0);
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    if(ConfigurationWindow.Instance != null && ConfigurationWindow.Opened)
                        ConfigurationWindow.Instance.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Normal:

                    break;
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.SelectedBot.BringClientToForeground();
        }
    }
}
