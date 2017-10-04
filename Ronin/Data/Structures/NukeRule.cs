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
    public class NukeRule : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _repeatUntilSuccess;
        private bool _useDefaultRange = true;
        private double _interval = 0.5;
        private int _nukeId;
        private long _minDistance;
        private long _maxDistance;
        private string _nukeName;
        public NukeType NukeType;
        private TargetType _conditionsTargetType;
        private TargetType _aoeMonstersAroundType;
        private int _aoeMonsterCount;
        private int _aoeRange = 300;
        private bool _aoeActivated;
        private int _aoeCountComparisonType = 1;
        private FilterType _monsterFilterType;
        private L2PlayerData _data;
        public long LastUsed;

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> MonsterFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var mosnterSplit = MonsterFilterStr.Trim().Split(new Char[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var mob in mosnterSplit)
                {
                    if(ExportedData.NpcIdToName.Any(pair => pair.Value.Equals(mob,StringComparison.OrdinalIgnoreCase)))
                        list.Add(new UIFormElement { Enable = true, Name = mob});
                }

                foreach (var mob in Data.SurroundingMonsters)
                {
                    if (!list.Any(unit => ((string) unit.Name).Equals(mob.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(new UIFormElement { Enable = false, Name = mob.Name});
                    }
                }

                return list;
            }
        }


        public ComparisonType AoeCountComparisonTypeEnum
        {
            get { return (ComparisonType)_aoeCountComparisonType; }
            set { _aoeCountComparisonType = (int)value; }
        }

        private string _monsterFilterStr ="";

        public string MonsterFilterStr
        {
            get { return _monsterFilterStr; }
            set
            {
                _monsterFilterStr = value;
                OnPropertyChanged();
            }
        }

        public NukeRule(L2PlayerData data)
        {
            this.Data = data;
            PropertyChanged +=
              (obj, args) =>
              {
                  if (args.PropertyName != nameof(NukeConditionsString))
                  {
                      OnPropertyChanged(nameof(NukeConditionsString));
                  }
              };
        }

        public string NukeConditionsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if(HealthPercentOver > 0 || HealthPercentBelow > 0 || ManaPercentOver > 0 || ManaPercentBelow > 0 || CombatPointsPercentOver > 0 || CombatPointsPercentBelow > 0)
                    sb.Append("S");
                sb.Append(HealthPercentOver > 0 ? $" HP > {HealthPercentOver} %" : string.Empty);
                sb.Append(HealthPercentBelow > 0 ? $" HP < {HealthPercentBelow} %" : string.Empty);
                sb.Append(ManaPercentOver > 0 ? $" MP > {ManaPercentOver} %" : string.Empty);
                sb.Append(ManaPercentBelow > 0 ? $" MP < {ManaPercentBelow} %" : string.Empty);
                sb.Append(CombatPointsPercentOver > 0 ? $" CP > {CombatPointsPercentOver} %" : string.Empty);
                sb.Append(CombatPointsPercentBelow > 0 ? $" CP < {CombatPointsPercentBelow} %" : string.Empty);
                sb.Append(RepeatUntilSuccess ? $" Rep: {RepeatUntilSuccess}" : string.Empty);
                sb.Append(AoeMonsterCount > 0 ? $" AOE" : string.Empty);
                sb.Append(MonsterFilterStr.Trim().Length > 0 ? $" Target Filter" : string.Empty);
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

        private int _targetHealthPercentOver;

        public int TargetHealthPercentOver
        {
            get { return _targetHealthPercentOver; }
            set
            {
                _targetHealthPercentOver = value;
                OnPropertyChanged();
            }
        }

        private int _targetHealthPercentBelow;

        public int TargetHealthPercentBelow
        {
            get { return _targetHealthPercentBelow; }
            set
            {
                _targetHealthPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _targetManaPercentBelow;

        public int TargetManaPercentBelow
        {
            get { return _targetManaPercentBelow; }
            set
            {
                _targetManaPercentBelow = value;
                OnPropertyChanged();
            }
        }

        private int _targetManaPercentOver;

        public int TargetManaPercentOver
        {
            get { return _targetManaPercentOver; }
            set
            {
                _targetManaPercentOver = value;
                OnPropertyChanged();
            }
        }

        private bool _targetShouldntHaveUD;

        public bool TargetShouldntHaveUD
        {
            get { return _targetShouldntHaveUD; }
            set
            {
                _targetShouldntHaveUD = value; 
                OnPropertyChanged();
            }
        }

        private bool _targetIsDead;

        public bool TargetIsDead
        {
            get { return _targetIsDead; }
            set
            {
                _targetIsDead = value;
                OnPropertyChanged();
            }
        }

        private bool _targetIsSpoiled;

        public bool TargetIsSpoiled
        {
            get { return _targetIsSpoiled; }
            set
            {
                _targetIsSpoiled = value;
                OnPropertyChanged();
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        public string Name;

        public string NukeName
        {
            get
            {
                switch (NukeType)
                {
                    case NukeType.Item:
                        return Name;
                    case NukeType.Skill:
                        return ExportedData.SkillsIdToName.ContainsKey(NukeId) ? ExportedData.SkillsIdToName[NukeId] : "(ERROR)";;
                }
                return _nukeName;
            }
            set
            {
                _nukeName = value;
                OnPropertyChanged();
            }
        }

        public bool RepeatUntilSuccess
        {
            get { return _repeatUntilSuccess; }
            set
            {
                _repeatUntilSuccess = value;
                OnPropertyChanged();
            }
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
                if(UseDefaultRange)
                    switch (NukeType)
                    {
                        case NukeType.Skill:
                            if (ExportedData.SkillIdToMaxDistance.ContainsKey(NukeId))
                            {
                                if (ExportedData.SkillIdToMaxDistance[NukeId] > 2000)
                                    return _maxDistance = 150;
                                return _maxDistance = ExportedData.SkillIdToMaxDistance[NukeId];
                            }
                            return _maxDistance = 600;
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

        public TargetType AoeMonstersAroundType
        {
            get { return _aoeMonstersAroundType; }
            set { _aoeMonstersAroundType = value; }
        }

        public FilterType MonsterFilterType
        {
            get { return _monsterFilterType; }
            set { _monsterFilterType = value; }
        }

        public int AoeMonsterCount
        {
            get { return _aoeMonsterCount; }
            set
            {
                _aoeMonsterCount = value;
                OnPropertyChanged();
            }
        }

        public int NukeId
        {
            get { return _nukeId; }
            set { _nukeId = value; }
        }

        public int AoeRange
        {
            get { return _aoeRange; }
            set { _aoeRange = value; }
        }

        public int AoeCountComparisonType
        {
            get { return _aoeCountComparisonType; }
            set { _aoeCountComparisonType = value; }
        }

        public bool AoeActivated
        {
            get { return _aoeActivated; }
            set { _aoeActivated = value; }
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
