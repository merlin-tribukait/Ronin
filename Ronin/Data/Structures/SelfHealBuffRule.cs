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
    public enum ComparisonType
    {
        Less,
        More,
        Equal
    }

    public class UIFormElement
    {
        private bool _enable;
        private string _name;

        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }

    public class SelfHealBuffRule : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _useDefaultRange = true;
        private double _interval = 1; //20min
        private int _selfHealBuffId;
        private long _minDistance;
        private long _maxDistance;
        private string _nukeName;
        public NukeType NukeType;
        private TargetType _conditionsTargetType;
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
        private bool _useOnPet;
        private bool _petIsDead;
        private bool _hasDeathPenalty;
        private L2PlayerData _data;

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

        public SelfHealBuffRule(L2PlayerData data)
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


        private int _petHealthPercentOver;

        public int PetHealthPercentOver
        {
            get { return _petHealthPercentOver; }
            set
            {
                _petHealthPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _petHealthPercentBelow;

        public int PetHealthPercentBelow
        {
            get { return _petHealthPercentBelow; }
            set
            {
                _petHealthPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _petManaPercentBelow;

        public int PetManaPercentBelow
        {
            get { return _petManaPercentBelow; }
            set
            {
                _petManaPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _petManaPercentOver;

        public int PetManaPercentOver
        {
            get { return _petManaPercentOver; }
            set
            {
                _petManaPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _petFoodPercentBelow;

        public int PetFoodPercentBelow
        {
            get { return _petFoodPercentBelow; }
            set
            {
                _petFoodPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _petFoodPercentOver;

        public int PetFoodPercentOver
        {
            get { return _petFoodPercentOver; }
            set
            {
                _petFoodPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _petSummonCountBelow;

        public int PetSummonCountBelow
        {
            get { return _petSummonCountBelow; }
            set
            {
                _petSummonCountBelow = value;
                OnPropertyChanged();
            }
        }

        public TargetType ConditionsTargetType
        {
            get { return _conditionsTargetType; }
            set
            {
                _conditionsTargetType = value;
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
                        return ExportedData.ItemIdToName[SelfHealBuffId];
                    case NukeType.Skill:
                        return ExportedData.SkillsIdToName[SelfHealBuffId];
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
                            if (ExportedData.SkillIdToMaxDistance.ContainsKey(SelfHealBuffId))
                                if (ExportedData.SkillIdToMaxDistance[SelfHealBuffId] > 2000)
                                    return _maxDistance = 150;
                            return _maxDistance = ExportedData.SkillIdToMaxDistance[SelfHealBuffId] + 100;
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

        public int SelfHealBuffId
        {
            get { return _selfHealBuffId; }
            set { _selfHealBuffId = value; }
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
                if (value && Data!=null)
                {
                    UseIfBuffIsPresent = false;

                    if (SelectedBuffsFilter.All( buff => buff.Enable == false))
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

        public bool UseOnPet
        {
            get { return _useOnPet; }
            set
            {
                _useOnPet = value; 
                OnPropertyChanged();
            }
        }

        public bool PetIsDead
        {
            get { return _petIsDead; }
            set { _petIsDead = value; }
        }

        [JsonIgnore]
        public L2PlayerData Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public bool HasDeathPenalty
        {
            get { return _hasDeathPenalty; }
            set { _hasDeathPenalty = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
