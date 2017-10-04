using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class DialogHandler : LogicHandler
    {
        public void HandleDialogs()
        {
            if (AcceptResurrection && _data.MainHero.IsDead && _data.PendingRessurectDialog)
            {
                //Thread.Sleep(5000);
                _actionsController.AnswerRessurectDialog(true);
                //else
                //answer false
            }
        }

        private bool _inviteActivated;

        public bool InviteActivated
        {
            get { return _inviteActivated; }
            set
            {
                _inviteActivated = value;
                OnPropertyChanged();
            }
        }

        private string _playersToInviteStr;

        public string PlayersToInviteStr
        {
            get { return _playersToInviteStr; }
            set
            {
                _playersToInviteStr = value; 
                OnPropertyChanged();
            }
        }

        private int _partyTypeToCreate;

        public int PartyTypeToCreate
        {
            get { return _partyTypeToCreate; }
            set
            {
                _partyTypeToCreate = value;
                OnPropertyChanged();
            }
        }

        private PartyType PartyTypeToCreateEnum
        {
            get { return (PartyType)_partyTypeToCreate; }
            set
            {
                _partyTypeToCreate = (int)value;
                OnPropertyChanged(nameof(PartyTypeToCreate));
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedPlayersToInviteFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var playerSplit = PlayersToInviteStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

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
                    if (!playera.IsMyPartyMember)
                        if (!list.Any(player => player.Name.Equals(playera.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = playera.Name });
                        }
                }

                return list;
            }
        }

        private bool _acceptPartyActivated;

        public bool AcceptPartyActivated
        {
            get { return _acceptPartyActivated; }
            set
            {
                _acceptPartyActivated = value;
                OnPropertyChanged();
            }
        }

        private string _playersToAcceptPartyStr;

        public string PlayersToAcceptPartyStr
        {
            get { return _playersToAcceptPartyStr; }
            set
            {
                _playersToAcceptPartyStr = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedPlayersToAcceptPartyFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var playerSplit = PlayersToAcceptPartyStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var player in playerSplit)
                {
                    list.Add(new UIFormElement { Enable = true, Name = player });
                }

                //add pt members first
                foreach (var playera in _data.Players)
                {
                    if (playera.Value.IsMyPartyMember)
                        if (!list.Any(player => player.Name.Equals(playera.Value.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = playera.Value.Name });
                        }
                }

                //then the rest
                foreach (var playera in _data.SurroundingPlayers)
                {
                    if (!playera.IsMyPartyMember)
                        if (!list.Any(player => player.Name.Equals(playera.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = playera.Name });
                        }
                }

                return list;
            }
        }

        private bool _refusePartyFromStrangers;

        public bool RefusePartyFromStrangers
        {
            get { return _refusePartyFromStrangers; }
            set
            {
                _refusePartyFromStrangers = value;
                OnPropertyChanged();
            }
        }

        private Random _random = new Random();
        
        private int _refusePartyDelayMin;

        public int RefusePartyDelayMin
        {
            get { return _refusePartyDelayMin; }
            set
            {
                _refusePartyDelayMin = value;
                if (value > RefusePartyDelayMax) RefusePartyDelayMax = _refusePartyDelayMin;

                OnPropertyChanged();
                OnPropertyChanged(nameof(RefusePartyDelayMax));
            }
        }

        private int _refusePartyDelayMax=5;

        public int RefusePartyDelayMax
        {
            get { return _refusePartyDelayMax; }
            set
            {
                _refusePartyDelayMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RefusePartyDelayMin));
            }
        }

        private bool _acceptResurrection;

        public bool AcceptResurrection
        {
            get { return _acceptResurrection; }
            set
            {
                _acceptResurrection = value;
                OnPropertyChanged();
            }
        }

        private bool _acceptResurrectionFromEveryone = true;

        public bool AcceptResurrectionFromEveryone
        {
            get { return _acceptResurrectionFromEveryone; }
            set
            {
                _acceptResurrectionFromEveryone = value;
                OnPropertyChanged();
            }
        }

        private bool _acceptResurrectionFromParty;

        public bool AcceptResurrectionFromParty
        {
            get { return _acceptResurrectionFromParty; }
            set
            {
                _acceptResurrectionFromParty = value;
                OnPropertyChanged();
            }
        }

        private bool _acceptResurrectionFromOwnChars;

        public bool AcceptResurrectionFromOwnChars
        {
            get { return _acceptResurrectionFromOwnChars; }
            set
            {
                _acceptResurrectionFromOwnChars = value;
                OnPropertyChanged();
            }
        }

        private bool _acceptResurrectionFromList;

        public bool AcceptResurrectionFromList
        {
            get { return _acceptResurrectionFromList; }
            set
            {
                _acceptResurrectionFromList = value;
                OnPropertyChanged();
            }
        }

        private string _playersToAcceptRessStr;

        public string PlayersToAcceptRessStr
        {
            get { return _playersToAcceptRessStr; }
            set
            {
                _playersToAcceptRessStr = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedPlayersToAcceptRessFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var playerSplit = PlayersToAcceptRessStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var player in playerSplit)
                {
                    list.Add(new UIFormElement { Enable = true, Name = player });
                }

                //then the rest
                foreach (var playera in _data.SurroundingPlayers)
                {
                    if (!playera.IsMyPartyMember)
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
