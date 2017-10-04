using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Ronin
{
    /// <summary>
    /// Interaction logic for SaveConfiguration.xaml
    /// </summary>
    public partial class SaveConfiguration 
    {
        public SaveConfiguration(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
            var prevConfig = ((ViewModel) DataContext).SelectedBot.SelectedConfiguration;
            ConfigFileNameTb.Text = prevConfig==null || prevConfig.Trim().Length==0 ? ((ViewModel)DataContext).SelectedBot?.PlayerData?.MainHero.Name : prevConfig;
        }

        public void SaveConfigurationa(string fileName)
        {
            Directory.CreateDirectory("Configurations/");
            File.WriteAllText(
                "Configurations/" + fileName +
                //L2BotsViewModel.SelectedBot.data.ServerId + 
                ".json",
                JsonConvert.SerializeObject(((ViewModel)this.DataContext).SelectedBot.Engine,
                Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));
        }

        private void SaveConfigurationButton(object sender, RoutedEventArgs e)
        {
            SaveAndExit();
        }

        private void SaveAndExit()
        {
            SaveConfigurationa(ConfigFileNameTb.Text.Trim());
            ((ViewModel)DataContext).ReloadConfigurations();
            ((ViewModel)DataContext).SelectedBot.SelectedConfiguration = ConfigFileNameTb.Text.Trim();

            if (ConfigurationWindow.Opened && ConfigurationWindow.Instance != null)
                ConfigurationWindow.Instance.SetConfigurationBoxTitle(((ViewModel)DataContext).SelectedBot.SelectedConfiguration);
            Close();
        }
    }
}
