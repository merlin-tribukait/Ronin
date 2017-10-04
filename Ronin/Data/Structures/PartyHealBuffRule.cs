using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Annotations;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Data.Structures
{
    public class PartyHealBuffRule : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _useDefaultRange = true;
        private double _interval = 1; //20min
        private int _partyHealBuffId;
        private long _minDistance;
        private long _maxDistance;
        private string _nukeName;
        public NukeType NukeType;
        public long LastUsed;
        private bool _useInCombat;
        private bool _requiresTarget;
        private bool _useIfBuffIsMissing;
        private bool _useIfBuffIsPresent;
        private bool _timerCheckActivated;
        private int _timerComparisonType;
        private int _timerSeconds;
        private bool _skillLevelComparisonActivated;
        private int _skillLevelComparisonType;
        private int _skillLevelRequired;
        private bool _useOnSummonPet;
        private bool _useOnPlayer = true;

        [JsonIgnore]
        public GameFigure ChosenTarget;

        private L2PlayerData _data;
        //key = name, value = shouldInclude
        public Dictionary<string,FilterType> PlayersFilter = new Dictionary<string, FilterType>();

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> PlayersToBuff
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                foreach (var player in Data.Players)
                {
                    if (player.Value.IsMyPartyMember)
                    {
                        list.Add(new UIFormElement { Enable = PlayersFilter.ContainsKey(player.Value.Name) ? PlayersFilter[player.Value.Name] == FilterType.Inclusive : UseOnPlayer , Name = player.Value.Name });
                        if (player.Value.PlayerSummons.Count > 0)
                        {
                            var exprName = player.Value.Name + "'s " + player.Value.PlayerSummons.First().Name;
                            list.Add(new UIFormElement { Enable = PlayersFilter.ContainsKey(exprName) ? PlayersFilter[exprName] == FilterType.Inclusive : UseOnSummonPet, Name = exprName});
                        }
                    }
                }

                return list;
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedBuffsFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var buffsSplit = SelectedBuffsStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var buff in buffsSplit)
                {
                    if (ExportedData.SkillsIdToName.Any(pair => pair.Value.Equals(buff, StringComparison.OrdinalIgnoreCase)))
                        list.Add(new UIFormElement { Enable = true, Name = buff });
                }

                foreach (var buffKeyPair in Data.MainHero.Buffs)
                {
                    if (!list.Any(unit => ((string)unit.Name).Equals(buffKeyPair.Value.BuffName, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(new UIFormElement { Enable = false, Name = buffKeyPair.Value.BuffName });
                    }
                }

                return list;
            }
        }

        private string _selectedBuffsStr = string.Empty;

        public string SelectedBuffsStr
        {
            get { return _selectedBuffsStr; }
            set
            {
                _selectedBuffsStr = value;
                OnPropertyChanged();
            }
        }

        public PartyHealBuffRule(L2PlayerData data)
        {
            this.Data = data;
            PropertyChanged +=
              (obj, args) =>
              {
                  if (args.PropertyName != nameof(ConditionsString))
                  {
                      OnPropertyChanged(nameof(ConditionsString));
                  }
              };
        }

        public string ConditionsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                //if (HealthPercentOver > 0 || HealthPercentBelow > 0 || ManaPercentOver > 0 || ManaPercentBelow > 0 || CombatPointsPercentOver > 0 || CombatPointsPercentBelow > 0)
                //    sb.Append(ConditionsTargetType == TargetType.Self ? "S" : "T");
                sb.Append(HealthPercentOver > 0 ? $" HP > {HealthPercentOver} %" : string.Empty);
                sb.Append(HealthPercentBelow > 0 ? $" HP < {HealthPercentBelow} %" : string.Empty);
                sb.Append(ManaPercentOver > 0 ? $" MP > {ManaPercentOver} %" : string.Empty);
                sb.Append(ManaPercentBelow > 0 ? $" MP < {ManaPercentBelow} %" : string.Empty);
                sb.Append(CombatPointsPercentOver > 0 ? $" CP > {CombatPointsPercentOver} %" : string.Empty);
                sb.Append(CombatPointsPercentBelow > 0 ? $" CP < {CombatPointsPercentBelow} %" : string.Empty);
                sb.Append(RequiresTarget ? " T" : string.Empty);
                sb.Append(UseInCombat ? " C" : string.Empty);
                return sb.ToString();
            }
        }
        
        private int _healthPercentOver;

        public int HealthPercentOver
        {
            get { return _healthPercentOver; }
            set
            {
                _healthPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _healthPercentBelow;

        public int HealthPercentBelow
        {
            get { return _healthPercentBelow; }
            set
            {
                _healthPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _manaPercentBelow;

        public int ManaPercentBelow
        {
            get { return _manaPercentBelow; }
            set
            {
                _manaPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _manaPercentOver;

        public int ManaPercentOver
        {
            get { return _manaPercentOver; }
            set
            {
                _manaPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _combatPointsPercentBelow;

        public int CombatPointsPercentBelow
        {
            get { return _combatPointsPercentBelow; }
            set
            {
                _combatPointsPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _combatPointsPercentOver;

        public int CombatPointsPercentOver
        {
            get { return _combatPointsPercentOver; }
            set
            {
                _combatPointsPercentOver = value;
                OnPropertyChanged();
            }
        }

        private bool _partyMemberIsDead;

        public bool PartyMemberIsDead
        {
            get { return _partyMemberIsDead; }
            set
            {
                _partyMemberIsDead = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberHealthPercentOver;

        public int PartyMemberHealthPercentOver
        {
            get { return _partyMemberHealthPercentOver; }
            set
            {
                _partyMemberHealthPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberHealthPercentBelow;

        public int PartyMemberHealthPercentBelow
        {
            get { return _partyMemberHealthPercentBelow; }
            set
            {
                _partyMemberHealthPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberManaPercentBelow;

        public int PartyMemberManaPercentBelow
        {
            get { return _partyMemberManaPercentBelow; }
            set
            {
                _partyMemberManaPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberManaPercentOver;

        public int PartyMemberManaPercentOver
        {
            get { return _partyMemberManaPercentOver; }
            set
            {
                _partyMemberManaPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberCombatPointsPercentBelow;

        public int PartyMemberCombatPointsPercentBelow
        {
            get { return _partyMemberCombatPointsPercentBelow; }
            set
            {
                _partyMemberCombatPointsPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _partyMemberCombatPointsPercentOver;

        public int PartyMemberCombatPointsPercentOver
        {
            get { return _partyMemberCombatPointsPercentOver; }
            set
            {
                _partyMemberCombatPointsPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _averagePartyMemberHealthPercentOver;

        public int AveragePartyMemberHealthPercentOver
        {
            get { return _averagePartyMemberHealthPercentOver; }
            set
            {
                _averagePartyMemberHealthPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _averagePartyMemberHealthPercentBelow;

        public int AveragePartyMemberHealthPercentBelow
        {
            get { return _averagePartyMemberHealthPercentBelow; }
            set
            {
                _averagePartyMemberHealthPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _deadPartyMembersOver;

        public int DeadPartyMembersOver
        {
            get { return _deadPartyMembersOver; }
            set
            {
                _deadPartyMembersOver = value;
                OnPropertyChanged();
            }
        }

        private int _deadPartyMembersBelow;

        public int DeadPartyMembersBelow
        {
            get { return _deadPartyMembersBelow; }
            set
            {
                _deadPartyMembersBelow = value;
                OnPropertyChanged();
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string Name;

        public string SelfHealBuffName
        {
            get
            {
                switch (NukeType)
                {
                    case NukeType.Item:
                        return Name;
                    case NukeType.Skill:
                        return ExportedData.SkillsIdToName[PartyHealBuffId];
                }
                return _nukeName;
            }
            set { _nukeName = value; }
        }

        public bool UseDefaultRange
        {
            get { return _useDefaultRange; }
            set
            {
                _useDefaultRange = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaxDistance));
                OnPropertyChanged(nameof(MinDistance));
            }
        }

        public double Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                OnPropertyChanged();
            }
        }

        public long MinDistance
        {
            get { return _minDistance; }
            set
            {
                _minDistance = value;
                OnPropertyChanged();
            }
        }

        public long MaxDistance
        {
            get
            {
                if (UseDefaultRange)
                    switch (NukeType)
                    {
                        case NukeType.Skill:
                            if (ExportedData.SkillIdToMaxDistance.ContainsKey(PartyHealBuffId))
                                if (ExportedData.SkillIdToMaxDistance[PartyHealBuffId] > 2000)
                                    return _maxDistance = 150;
                            return _maxDistance = ExportedData.SkillIdToMaxDistance[PartyHealBuffId];
                        case NukeType.Item:
                            return _maxDistance = 600;
                    }
                return _maxDistance;
            }
            set
            {
                _maxDistance = value;
                OnPropertyChanged();
            }
        }

        public int PartyHealBuffId
        {
            get { return _partyHealBuffId; }
            set { _partyHealBuffId = value; }
        }

        public bool UseInCombat
        {
            get { return _useInCombat; }
            set
            {
                _useInCombat = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresTarget
        {
            get { return _requiresTarget; }
            set
            {
                _requiresTarget = value;
                OnPropertyChanged();
            }
        }

        public int TimerComparisonType
        {
            get { return _timerComparisonType; }
            set
            {
                _timerComparisonType = value;
                OnPropertyChanged();
            }
        }

        public ComparisonType TimerComparisonTypeEnum
        {
            get { return (ComparisonType)_timerComparisonType; }
            set { _timerComparisonType = (int)value; }
        }

        public int SkillLevelComparisonType
        {
            get { return _skillLevelComparisonType; }
            set
            {
                _skillLevelComparisonType = value;
                OnPropertyChanged();
            }
        }

        public ComparisonType SkillLevelComparisonTypeEnum
        {
            get { return (ComparisonType)_skillLevelComparisonType; }
            set { _skillLevelComparisonType = (int)value; }
        }

        public bool UseIfBuffIsMissing
        {
            get { return _useIfBuffIsMissing; }
            set
            {
                _useIfBuffIsMissing = value;
                if (value && Data != null)
                {
                    UseIfBuffIsPresent = false;

                    if (SelectedBuffsFilter.All(buff => buff.Enable == false))
                        SelectedBuffsStr = SelfHealBuffName + ";";
                }

                OnPropertyChanged();
            }
        }

        public bool UseIfBuffIsPresent
        {
            get { return _useIfBuffIsPresent; }
            set
            {
                _useIfBuffIsPresent = value;
                if (value)
                    UseIfBuffIsMissing = false;
                OnPropertyChanged();
            }
        }

        public int TimerSeconds
        {
            get { return _timerSeconds; }
            set
            {
                _timerSeconds = value;
                OnPropertyChanged();
            }
        }

        public int SkillLevelRequired
        {
            get { return _skillLevelRequired; }
            set
            {
                _skillLevelRequired = value;
                OnPropertyChanged();
            }
        }

        public bool TimerCheckActivated
        {
            get { return _timerCheckActivated; }
            set
            {
                _timerCheckActivated = value;
                OnPropertyChanged();
            }
        }

        public bool SkillLevelComparisonActivated
        {
            get { return _skillLevelComparisonActivated; }
            set
            {
                _skillLevelComparisonActivated = value;
                OnPropertyChanged();
            }
        }

        public bool UseOnSummonPet
        {
            get { return _useOnSummonPet; }
            set
            {
                _useOnSummonPet = value; 
                OnPropertyChanged();
            }
        }

        public bool UseOnPlayer
        {
            get { return _useOnPlayer; }
            set
            {
                _useOnPlayer = value; 
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public L2PlayerData Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
