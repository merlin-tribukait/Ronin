using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class AssistHandler : LogicHandler
    {
        private string _selectedPlayersStr = string.Empty;

        public string SelectedPlayersStr
        {
            get { return _selectedPlayersStr; }
            set
            {
                _selectedPlayersStr = value;
                OnPropertyChanged();
            }
        }

        private bool _assistActivated;

        public bool AssistActivated
        {
            get { return _assistActivated; }
            set
            {
                _assistActivated = value;
                OnPropertyChanged();
            }
        }

        private AssistType _assistType;

        public AssistType AssistType
        {
            get { return _assistType; }
            set
            {
                _assistType = value;
                OnPropertyChanged();
            }
        }

        private int _customDelay;

        public int CustomDelay
        {
            get { return _customDelay; }
            set
            {
                _customDelay = value;
                OnPropertyChanged();
            }
        }

        private int _randomDelayMin;

        public int RandomDelayMin
        {
            get { return _randomDelayMin; }
            set
            {
                _randomDelayMin = value;
                if (value > RandomDelayMax) RandomDelayMax = _randomDelayMin;

                OnPropertyChanged();
                OnPropertyChanged(nameof(RandomDelayMax));
            }
        }

        private int _randomDelayMax;

        public int RandomDelayMax
        {
            get { return _randomDelayMax; }
            set
            {
                _randomDelayMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RandomDelayMin));
            }
        }

        private Random _random = new Random();

        [JsonIgnore]
        public Player ActiveAssister
        {
            get
            {
                Player playerToAssistOn = null;
                foreach (var player in SelectedPlayersFilter)
                {
                    if (player.Enable &&
                        _data.SurroundingPlayers.Any(
                            playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        playerToAssistOn = _data.SurroundingPlayers.First(
                            playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase));
                        break;
                    }
                }

                return playerToAssistOn;
            }
        }

        public void Assist()
        {
            Player playerToAssistOn = null;
            try
            {
                foreach (var player in SelectedPlayersFilter)
                {
                    if (player.Enable &&
                        _data.SurroundingPlayers.Any(
                            playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        playerToAssistOn = _data.SurroundingPlayers.First(
                            playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.GetLogger().Debug(e);
            }

            if(playerToAssistOn == null)
                return;

            if(_data.MainHero.TargetObjectId != playerToAssistOn.TargetObjectId)
                if (playerToAssistOn.TargetObjectId == 0)
                {
                    _actionsController.CancelTarget(true);
                    _actionsController.StopMove();
                }
                else
                {
                    switch (AssistType)
                    {
                        case AssistType.AttackInstant:
                            _actionsController.TargetByObjectId(playerToAssistOn.TargetObjectId);
                            break;

                        case AssistType.WaitForFirstAttack:
                            if(playerToAssistOn.LastUnitAttackedObjectId == playerToAssistOn.TargetObjectId)
                                _actionsController.TargetByObjectId(playerToAssistOn.TargetObjectId);
                            break;

                        case AssistType.WaitCustomDelay:
                            if(Math.Abs(Environment.TickCount - playerToAssistOn.TargetStamp)/1000 >= CustomDelay)
                                _actionsController.TargetByObjectId(playerToAssistOn.TargetObjectId);
                            break;

                        case AssistType.WaitRandomDelay:
                            if(Math.Abs(Environment.TickCount - playerToAssistOn.TargetStamp) >= _random.Next(RandomDelayMin*1000, RandomDelayMax*1000))
                            _actionsController.TargetByObjectId(playerToAssistOn.TargetObjectId);
                            break;
                    }
                }
        }

        public bool ShouldAssist()
        {
            Player playerToAssistOn = null;
            foreach (var player in SelectedPlayersFilter)
            {
                if (player.Enable &&
                    _data.SurroundingPlayers.Any(
                        playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    playerToAssistOn = _data.SurroundingPlayers.First(
                        playera => playera.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                }
            }

            if (playerToAssistOn == null)
                return false;

            if (_data.MainHero.TargetObjectId != playerToAssistOn.TargetObjectId)
                return true;

            return false;
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedPlayersFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var playerSplit = SelectedPlayersStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var player in playerSplit)
                {
                    list.Add(new UIFormElement { Enable = true, Name = player });
                }

                //add pt members first
                foreach (var playera in _data.Players)
                {
                    if(playera.Value.IsMyPartyMember)
                        if (!list.Any(player => player.Name.Equals(playera.Value.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = playera.Value.Name });
                        }
                }

                //then the rest
                foreach (var playera in _data.SurroundingPlayers)
                {
                    if(!playera.IsMyPartyMember)
                        if (!list.Any(player => player.Name.Equals(playera.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = playera.Name });
                        }
                }

                return list;
            }
        }
    }
}
