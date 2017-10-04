using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin
{
    public class ViewModel : INotifyPropertyChanged
    {
        private MultiThreadObservableCollection<L2Bot> bots;
        private L2Bot selectedBot;

        public MultiThreadObservableCollection<L2Bot> Bots
        {
            get
            {
                if (this.bots == null)
                {
                    this.bots = new MultiThreadObservableCollection<L2Bot>();
                }
                return this.bots;
            }

            set
            {
                if (this.bots == null)
                {
                    this.bots = new MultiThreadObservableCollection<L2Bot>();
                }
                this.bots.Clear();
                this.bots.AddRange(value);
                //if (this.bots.Count == 1 && this.SelectedBot == null)
                //{
                //    this.SelectedBot = this.bots.First();
                //}
            }
        }

        private PropertyChangedEventHandler _changeCnfgWindowTitleHandler = delegate (object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "Name")
            {
                if (ConfigurationWindow.Opened && ConfigurationWindow.Instance != null && ConfigurationWindow.Instance.GetWindowTitle() != ((MainHero)sender).Name)
                {
                    ConfigurationWindow.Instance.SetWindowTitle(((MainHero) sender).Name ?? string.Empty);
                }
            }
        };

        private MultiThreadObservableCollection<string> availableConfigurations = new MultiThreadObservableCollection<string>();

        public MultiThreadObservableCollection<string> AvailableConfigurations
        {
            get
            {
                return availableConfigurations;
            }
            set { availableConfigurations = value; }
        }

        public void ReloadConfigurations()
        {
            Directory.CreateDirectory("Configurations/");

            availableConfigurations.Clear();
            string[] files = Directory.GetFiles("Configurations/");
            foreach (var file in files)
            {
                availableConfigurations.Add(file.Replace("Configurations/", string.Empty).Replace(".json", string.Empty));
            }

            OnPropertyChanged(nameof(AvailableConfigurations));
        }

        public ViewModel()
        {
            ReloadConfigurations();
        }

        public L2Bot SelectedBot
        {
            get { return selectedBot; }
            set
            {
                if(selectedBot != null)
                    selectedBot.PlayerData.MainHero.PropertyChanged -= _changeCnfgWindowTitleHandler;

                selectedBot = value;
                if (selectedBot != null)
                {
                    selectedBot.PlayerData.MainHero.PropertyChanged += _changeCnfgWindowTitleHandler;
                    selectedBot.PlayerData.MainHero.Name = selectedBot.PlayerData.MainHero.Name;

                    if(ConfigurationWindow.Opened && ConfigurationWindow.Instance != null)
                        ConfigurationWindow.Instance.SetConfigurationBoxTitle(selectedBot.SelectedConfiguration); 
                }

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
