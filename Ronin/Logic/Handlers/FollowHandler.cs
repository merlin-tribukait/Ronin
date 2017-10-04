using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class FollowHandler : LogicHandler
    {
        private FollowType _followType;

        public FollowType FollowType
        {
            get { return _followType; }
            set
            {
                _followType = value;
                OnPropertyChanged();
            }
        }

        private bool _followActivated;

        public bool FollowActivated
        {
            get { return _followActivated; }
            set
            {
                _followActivated = value;
                OnPropertyChanged();
            }
        }

        private int _minFollowDistance = 50;

        public int MinFollowDistance
        {
            get { return _minFollowDistance < 5 ? 5 : _minFollowDistance; }
            set
            {
                _minFollowDistance = value;
                if (value > MaxFollowDistance) MaxFollowDistance = value;
                OnPropertyChanged();
            }
        }

        private int _maxFollowDistance = 150;

        public int MaxFollowDistance
        {
            get { return _maxFollowDistance; }
            set
            {
                _maxFollowDistance = value;
                OnPropertyChanged();
            }
        }

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

        private long _followStamp = 0;
        private bool isFollowing = false;

        public bool Follow()
        {
            Player playerToFollow = null;

            try
            {
                switch (FollowType)
                {
                    case FollowType.PartyLeader:
                        if (_data.PartyMembers.Count > 0 &&
                            _data.SurroundingPlayers.Any(player => player.ObjectId == _data.PartyLeaderObjectId))
                        {
                            playerToFollow = _data.SurroundingPlayers.First(player => player.ObjectId == _data.PartyLeaderObjectId);
                        }

                        break;

                    case FollowType.PlayerList:
                        foreach (var player in SelectedPlayersFilter)
                        {
                            if (player.Enable && _data.SurroundingPlayers.Any(surrPlayer => surrPlayer.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
                                playerToFollow = _data.SurroundingPlayers.First(surrPlayer => surrPlayer.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase));
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                LogHelper.GetLogger().Debug(e);
            }
            

            if (playerToFollow != null)
            {
                if ((_data.MainHero.RangeTo(playerToFollow) > MaxFollowDistance ||
                    (isFollowing && _data.MainHero.RangeTo(playerToFollow) > MinFollowDistance +20)))
                {
                    isFollowing = true;
                    double distance = _data.MainHero.RangeTo(playerToFollow);
                    double lastCutToSkip = MinFollowDistance/distance;
                    int differenceInX = _data.MainHero.X - playerToFollow.X;
                    int differenceInY = _data.MainHero.Y - playerToFollow.Y;
                    int differenceInZ = _data.MainHero.Z - playerToFollow.Z;

                    if (Math.Abs(Environment.TickCount - _followStamp) > 500 && _data.MainHero.RangeTo(new Locatable((int)(playerToFollow.X + differenceInX * lastCutToSkip),
                            (int)(playerToFollow.Y + differenceInY * lastCutToSkip),
                            (int)(playerToFollow.Z + differenceInZ * lastCutToSkip))) > 50)
                    {
                        _actionsController.MoveToRaw((int) (playerToFollow.X + differenceInX*lastCutToSkip),
                            (int) (playerToFollow.Y + differenceInY*lastCutToSkip),
                            (int) (playerToFollow.Z + differenceInZ*lastCutToSkip));

                        _followStamp = Environment.TickCount;
                    }

                    return true;
                }
                else if (isFollowing)
                {
                    isFollowing = false;
                }
            }

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
