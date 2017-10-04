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
using Ronin.Protocols;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class NukeHandler : LogicHandler
    {
        private int _nukeTypeToAdd;
        public int NukeTypeToAdd
        {
            get { return _nukeTypeToAdd; }
            set
            {
                _nukeTypeToAdd = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NukesToAdd));
            }
        }

        private NukeRule _latestRule = null;
        private long _timeStamp = 0;

        public NukeRule GetNukeToCast(GameFigure target)
        {
            //if (Math.Abs(Environment.TickCount - _timeStamp) < 50)
            //{
            //    return _latestRule;
            //}

            _timeStamp = Environment.TickCount;

            NukeRule nuke = null;
            if(target!=null)
            foreach (var nukeRule in NukesToUse)
            {
                    if ((nukeRule.NukeType == NukeType.Item ? !_data.Inventory.Any(item => item.Value.ItemId == nukeRule.NukeId) : false) ||
                (nukeRule.NukeType == NukeType.Skill ? !_data.Skills.Any(skill => skill.Value.SkillId == nukeRule.NukeId) : false))
                        continue;

                    GameFigure selfCondFigure = _data.MainHero;

                if (!nukeRule.Enabled)
                    continue;

                //cond checks
                if (selfCondFigure != null)
                {
                    if (nukeRule.HealthPercentOver > 0 && nukeRule.HealthPercentOver > selfCondFigure.HealthPercent)
                        continue;

                    if (nukeRule.HealthPercentBelow > 0 && nukeRule.HealthPercentBelow < selfCondFigure.HealthPercent)
                        continue;

                    if (nukeRule.ManaPercentOver > 0 && nukeRule.ManaPercentOver > selfCondFigure.ManaPercent)
                        continue;

                    if (nukeRule.ManaPercentBelow > 0 && nukeRule.ManaPercentBelow < selfCondFigure.ManaPercent)
                        continue;

                    if (selfCondFigure is MainHero && nukeRule.CombatPointsPercentOver > 0 && nukeRule.CombatPointsPercentOver > selfCondFigure.CombatPointsPercent)
                        continue;

                    if (selfCondFigure is MainHero && nukeRule.CombatPointsPercentBelow > 0 && nukeRule.CombatPointsPercentBelow < selfCondFigure.CombatPointsPercent)
                        continue;
                }

                if (target != null && target.Health > 0)
                {
                    if (nukeRule.TargetHealthPercentOver > 0 && nukeRule.TargetHealthPercentOver > target.HealthPercent)
                        continue;

                    if (nukeRule.TargetHealthPercentBelow > 0 && nukeRule.TargetHealthPercentBelow < target.HealthPercent)
                        continue;

                    if (nukeRule.TargetManaPercentOver > 0 && nukeRule.TargetManaPercentOver > target.ManaPercent)
                        continue;

                    if (nukeRule.TargetManaPercentBelow > 0 && nukeRule.TargetManaPercentBelow < target.ManaPercent)
                        continue;
                }

                if(nukeRule.TargetIsDead && !target.IsDead)
                    continue;

                if(!nukeRule.TargetIsDead && target.IsDead)
                    continue;

                if(nukeRule.TargetIsSpoiled && (!(target is Npc) || ((Npc)target).IsSweepable == false))
                    continue;

                if (nukeRule.RepeatUntilSuccess && _data.LandedSkills.ContainsKey(nukeRule.NukeId) && _data.LandedSkills[nukeRule.NukeId] == target.ObjectId)
                    continue;

                if (Math.Abs(Environment.TickCount - nukeRule.LastUsed) < nukeRule.Interval * 1000)
                    continue;

                if (nukeRule.MonsterFilterType == FilterType.Inclusive && nukeRule.MonsterFilterStr.Trim().Length > 0 &&
                        !nukeRule.MonsterFilter.Any(mob => mob.Enable && mob.Name.Trim().Equals(ExportedData.NpcIdToName[target.UnitId].Trim(), StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (nukeRule.MonsterFilterType == FilterType.Exclusive && nukeRule.MonsterFilterStr.Trim().Length > 0 &&
                    nukeRule.MonsterFilter.Any(mob => mob.Enable && mob.Name.Trim().Equals(ExportedData.NpcIdToName[target.UnitId].Trim(), StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (nukeRule.AoeActivated)
                    if (nukeRule.AoeMonstersAroundType == TargetType.Self)
                    {
                        int mobsCount = _data.SurroundingMonsters.Count(mob => mob.RangeTo(_data.MainHero) < nukeRule.AoeRange && !mob.IsDead);
                        if (nukeRule.AoeCountComparisonTypeEnum == ComparisonType.Less && mobsCount >= nukeRule.AoeMonsterCount)
                            continue;
                        else if (nukeRule.AoeCountComparisonTypeEnum == ComparisonType.More && mobsCount <= nukeRule.AoeMonsterCount)
                            continue;
                    }
                    else
                    {
                        int mobsCount = _data.SurroundingMonsters.Count(mob => mob.RangeTo(target) < nukeRule.AoeRange && !mob.IsDead);
                        if (nukeRule.AoeCountComparisonTypeEnum == ComparisonType.Less && mobsCount >= nukeRule.AoeMonsterCount)
                            continue;
                        else if (nukeRule.AoeCountComparisonTypeEnum == ComparisonType.More && mobsCount <= nukeRule.AoeMonsterCount)
                            continue;
                    }

                if (nukeRule.NukeType == NukeType.Skill && !_data.Skills[nukeRule.NukeId].CanBeUsed())
                    continue;

                if (nukeRule.NukeType == NukeType.Skill && _data.MainHero.Mana < ExportedData.SkillManaConsumption[nukeRule.NukeId][_data.Skills[nukeRule.NukeId].SkillLevel])
                continue;

                nuke = nukeRule;
                break;
            }

            _latestRule = nuke;
            return nuke;
        }

        [JsonIgnore]
        public bool EngineRunning = true;

        public bool Nuke(GameFigure target)
        {
            var nuke = GetNukeToCast(target);
            if (nuke != null && _data.MainHero.RangeTo(target) >= nuke.MinDistance && _data.PlayerIsCasting == false &&
                _data.MainHero.RangeTo(target) <= nuke.MaxDistance + 100 && (nuke.NukeType == NukeType.Item ? _data.Inventory.Any(item => item.Value.ItemId == nuke.NukeId) : true) &&
                (nuke.NukeType == NukeType.Skill ? _data.Skills.Any(skill => skill.Value.SkillId == nuke.NukeId) : true))
            {
                int timeout = 0;
                while ( ((timeout < 3) || (target.IsDead && _data.SurroundingMonsters.Any(mob => mob.ObjectId == target.ObjectId) && nuke.TargetIsDead)) && EngineRunning)
                {
                    if (!nuke.TargetIsDead && target.IsDead)
                        break;

                    _data.LastUsedNukeId = nuke.NukeId;
                    if (nuke.NukeType == NukeType.Skill)
                        _actionsController.UseSkill(nuke.NukeId, false, false);
                    else if (nuke.NukeType == NukeType.Item)
                        _actionsController.UseItem(_data.Inventory.First(item => item.Value.ItemId == nuke.NukeId).Key, false);


                    Thread.Sleep(500);
                    timeout++;

                    if (_data.AttackFlag && GetNukeToCast(target) != nuke)
                        break;
                }
                nuke.LastUsed = Environment.TickCount - 500;

                return true;
            }

            return false;
        }

        private MultiThreadObservableCollection<NukeRule> _nukesToUse = new MultiThreadObservableCollection<NukeRule>();

        public MultiThreadObservableCollection<NukeRule> NukesToUse
        {
            get
            {
                return _nukesToUse;
            }
            set { _nukesToUse = value; }
        }

        private NukeRule _selectedNuke;

        public NukeRule SelectedNuke
        {
            get { return _selectedNuke; }
            set
            {
                _selectedNuke = value;
                OnPropertyChanged();
            }
        }

        private MultiThreadObservableCollection<dynamic> _nukesToAdd = new MultiThreadObservableCollection<dynamic>();

        [JsonIgnore]
        public MultiThreadObservableCollection<dynamic> NukesToAdd
        {
            get
            {
                _nukesToAdd.Clear();
                NukeType nukeType = (NukeType)NukeTypeToAdd;
                if (Initialiased && _data!=null)
                    switch (nukeType)
                    {
                        case NukeType.Skill:
                            foreach (var skill in _data.Skills.ToArray().OrderBy(skill => skill.Value.SkillName).ToList())
                            {
                                if (!skill.Value.IsPassive && !skill.Value.IsDisabled && !NukesToUse.Any(rule => rule.NukeName.Equals(skill.Value.SkillName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    _nukesToAdd.Add(new { SkillName = skill.Value.SkillName, Id = skill.Value.SkillId });
                                }
                            }

                            break;
                        case NukeType.Item:
                            foreach (var item in _data.Inventory.ToArray().OrderBy(item => item.Value.ItemName).ToList())
                            {
                                _nukesToAdd.Add(new { SkillName = item.Value.ItemName, Id = item.Value.ItemId });
                            }

                            break;
                        case NukeType.PetSkill:
                            break;
                    }


                return _nukesToAdd;
            }
        }

        public NukeHandler()
        {
        }

        public override void Init(L2PlayerData data, ActionsController actionsController)
        {
            base.Init(data, actionsController);

            data.Skills.CollectionChanged +=
               (obj, args) =>
               {
                   OnPropertyChanged(nameof(NukesToAdd));
               };

            NukesToUse.CollectionChanged +=
              (obj, args) =>
              {
                  OnPropertyChanged(nameof(NukesToAdd));
              };


            data.Inventory.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(NukesToAdd));
            };
        }
    }
}
