using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Annotations;
using Ronin.Data.Constants;
using Ronin.Logic;
using Ronin.Network;
using Ronin.Protocols;
using Ronin.Protocols.HighFive;
using Ronin.Protocols.Interlude;
using Ronin.Utilities;

namespace Ronin.Data
{
    public class L2Bot : INotifyPropertyChanged
    {
        private L2PlayerData _playerData = new L2PlayerData();
        private int _version;
        private ActionsController _actionsController;
        private Client _tcpClient;
        private Engine _engine = new Engine();
        private Injector _injector;

        public L2Bot(Injector var1)
        {
            _injector = var1;
        }

        public void Init(int var1, Client var2)
        {
            PlayerData.MainPlayerLogin += () =>
            {
                if (!Directory.Exists("Configurations/") || SelectedConfiguration!= null)
                    return;

                string[] files = Directory.GetFiles("Configurations/");

                foreach (var file in files)
                {
                    if (file.Replace("Configurations/", string.Empty).Replace(".json", string.Empty) == PlayerData.MainHero.Name)
                        LoadConfiguration(PlayerData.MainHero.Name);
                }
            };

            PlayerData.GameState = GameState.CharacterSelection;
            _tcpClient = var2;
            switch (var1)
            {
                case 268:
                    ActionsController = new H5ActionsController(PlayerData, var2);
                    _engine.Init(_playerData, ActionsController);
                    break;

                case 273:
                    ActionsController = new H5ActionsController(PlayerData, var2);
                    _engine.Init(_playerData, ActionsController);
                    break;

                case 746:
                    ActionsController = new ILActionsController(PlayerData, var2);
                    _engine.Init(_playerData, ActionsController);
                    break;
                default:

                    break;
            }
        }

        public L2PlayerData PlayerData
        {
            get { return _playerData; }
        }

        public Engine Engine
        {
            get { return _engine; }
            set
            {
                _engine = value;
                OnPropertyChanged();
            }
        }

        public ActionsController ActionsController
        {
            get { return _actionsController; }
            set { _actionsController = value; }
        }

        public void RestoreInitialState()
        {
            _injector?.RestoreHookedBytes();
        }

        public int GetPid()
        {
            return _injector?.ProcessId ?? 0;
        }

        public void BringClientToForeground()
        {
            _injector?.BringWindowToForeground();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public L2Bot()
        {
            
        }

        public void LoadConfiguration(string fileName)
        { 
            var configurationFile = File.ReadAllText("Configurations/" + fileName + ".json");
            var configs = JsonConvert.DeserializeObject<Engine>(configurationFile);

            var curbot = this;
            curbot.SelectedConfiguration = fileName;

            curbot.Engine.NukeHandler.NukesToUse.CloneRange(configs.NukeHandler.NukesToUse);
            foreach (var nuke in curbot.Engine.NukeHandler.NukesToUse)
            {
                nuke.Data = curbot.PlayerData;
            }

            curbot.Engine.SelfHealBuffHandler.NukesToUse.CloneRange(configs.SelfHealBuffHandler.NukesToUse);
            foreach (var nuke in curbot.Engine.SelfHealBuffHandler.NukesToUse)
            {
                nuke.Data = curbot.PlayerData;
            }

            curbot.Engine.PartyHealBuffHandler.NukesToUse.CloneRange(configs.PartyHealBuffHandler.NukesToUse);
            foreach (var nuke in curbot.Engine.PartyHealBuffHandler.NukesToUse)
            {
                nuke.Data = curbot.PlayerData;
            }

            curbot.Engine.AttackHandler.UseMeleeAttack = configs.AttackHandler.UseMeleeAttack;
            curbot.Engine.AttackHandler.UseDefaultAttackRangeCalculation = configs.AttackHandler.UseDefaultAttackRangeCalculation;
            if (!curbot.Engine.AttackHandler.UseDefaultAttackRangeCalculation)
            {
                curbot.Engine.AttackHandler.MaximumAttackDistance = configs.AttackHandler.MaximumAttackDistance;
                curbot.Engine.AttackHandler.MinimumAttackDistance = configs.AttackHandler.MinimumAttackDistance;
            }
            
            curbot.Engine.Range = configs.Range;
            curbot.Engine.CombatTargetType = configs.CombatTargetType;
            curbot.Engine.PointX = configs.PointX;
            curbot.Engine.PointY = configs.PointY;
            curbot.Engine.PointZ = configs.PointZ;

            curbot.Engine.AssistHandler.AssistActivated = configs.AssistHandler.AssistActivated;
            curbot.Engine.AssistHandler.SelectedPlayersStr = configs.AssistHandler.SelectedPlayersStr;
            curbot.Engine.AssistHandler.AssistType = configs.AssistHandler.AssistType;

            curbot.Engine.FollowHandler.FollowActivated = configs.FollowHandler.FollowActivated;
            curbot.Engine.FollowHandler.SelectedPlayersStr = configs.FollowHandler.SelectedPlayersStr;
            curbot.Engine.FollowHandler.FollowType = configs.FollowHandler.FollowType;
            curbot.Engine.FollowHandler.MinFollowDistance = configs.FollowHandler.MinFollowDistance;
            curbot.Engine.FollowHandler.MaxFollowDistance = configs.FollowHandler.MaxFollowDistance;

            curbot.Engine.DialogHandler.AcceptResurrection = configs.DialogHandler.AcceptResurrection;

            curbot.Engine.PickupHandler.RulesInUse.CloneRange(configs.PickupHandler.RulesInUse);
            curbot.Engine.PickupHandler.PickupAll = configs.PickupHandler.PickupAll;
            curbot.Engine.PickupHandler.PickupExclusive = configs.PickupHandler.PickupExclusive;
            curbot.Engine.PickupHandler.PickupInclusive = configs.PickupHandler.PickupInclusive;
            curbot.Engine.PickupHandler.PickupMine = configs.PickupHandler.PickupMine;
            curbot.Engine.PickupHandler.Range = configs.PickupHandler.Range;
            curbot.Engine.PickupHandler.DontPickupHerbs = configs.PickupHandler.DontPickupHerbs;
        }

        private string _selectedConfiguration;
        public string SelectedConfiguration {
            get { return _selectedConfiguration; }
            set
            {
                _selectedConfiguration = value;
                OnPropertyChanged();
            }
        }
    }
}
