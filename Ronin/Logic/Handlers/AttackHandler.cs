using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Protocols;

namespace Ronin.Logic.Handlers
{
    public class AttackHandler : LogicHandler
    {
        private const int BowRangeIncreaseSkillId = 113;
        private const int SnipeRangeIncreaseSkillId = 972;
        private const int CrossBowRangeIncreaseSkillId = 486;
        private const int SharpshootingRangeIncreaseSkillId = 521;


        private int _minimumAttackDistance;
        private int _maximumAttackDistance;
        private bool _useDefaultAttackRangeCalculation = true;
        private bool _useMeleeAttack;

        public override void Init(L2PlayerData data, ActionsController actionsController)
        {
            base.Init(data, actionsController);
            data.MainHero.PropertyChanged +=
                (obj, args) =>
                {
                    if (args.PropertyName == nameof(data.MainHero.WeaponDisplayId) && UseDefaultAttackRangeCalculation)
                    {
                        MaximumAttackDistance = CalculateMaximumAttackRange();
                    }
                };

            data.Skills.CollectionChanged +=
                (obj, args) =>
                {
                    if (UseDefaultAttackRangeCalculation)
                        MaximumAttackDistance = CalculateMaximumAttackRange();
                };

            data.MainHero.Buffs.CollectionChanged +=
                (obj, args) =>
                {
                    if (UseDefaultAttackRangeCalculation)
                        MaximumAttackDistance = CalculateMaximumAttackRange();
                };
        }

        private int CalculateMaximumAttackRange()
        {
            if (ExportedData.BowItemIds.Contains(_data.MainHero.WeaponDisplayId))
            {
                int range = 600;
                if (_data.Skills.ContainsKey(BowRangeIncreaseSkillId))
                    range += _data.Skills[BowRangeIncreaseSkillId].SkillLevel == 1 ? 200 : 400;

                if (_data.MainHero.Buffs.ContainsKey(SnipeRangeIncreaseSkillId))
                    range += 200;
                return range;
            }
            else if (ExportedData.CrossBowItemIds.Contains(_data.MainHero.WeaponDisplayId))
            {
                int range = 400;
                if (_data.Skills.ContainsKey(CrossBowRangeIncreaseSkillId))
                    range += _data.Skills[CrossBowRangeIncreaseSkillId].SkillLevel == 1 ? 200 : 400;

                if (_data.MainHero.Buffs.ContainsKey(SharpshootingRangeIncreaseSkillId))
                    range += (_data.MainHero.Buffs[SharpshootingRangeIncreaseSkillId].Level/2) * 50;
                return range;
            }
            else
            {
                return 50;
            }
        }

        public int MinimumAttackDistance
        {
            get { return _minimumAttackDistance; }
            set
            {
                _minimumAttackDistance = value;
                OnPropertyChanged();
            }
        }

        public int MaximumAttackDistance
        {
            get { return _maximumAttackDistance; }
            set
            {
                _maximumAttackDistance = value;
                OnPropertyChanged();
            }
        }

        public bool UseDefaultAttackRangeCalculation
        {
            get { return _useDefaultAttackRangeCalculation; }
            set
            {
                _useDefaultAttackRangeCalculation = value;
                OnPropertyChanged();

                if(value && _data!= null)
                    MaximumAttackDistance = CalculateMaximumAttackRange();
            }
        }

        public bool UseMeleeAttack
        {
            get { return _useMeleeAttack; }
            set
            {
                _useMeleeAttack = value;
                OnPropertyChanged();
            }
        }

        public bool Attack(GameFigure target)
        {
            if (target != null && UseMeleeAttack && _data.MainHero.TargetObjectId == target.ObjectId && 
                !_data.PlayerIsCasting &&
                _data.MainHero.RangeTo(target) >= MinimumAttackDistance &&
                _data.MainHero.RangeTo(target) <= MaximumAttackDistance + 150)
            {
                _actionsController.Attack();
                return true;
            }

            return false;
        }
    }
}
