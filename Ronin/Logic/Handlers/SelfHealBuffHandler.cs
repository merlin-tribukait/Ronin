using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Protocols;
using Ronin.Protocols.Abstract.Interfaces;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class SelfHealBuffHandler : LogicHandler
    {
        private SelfHealBuffRule _selectedRule;

        public SelfHealBuffRule SelectedRule
        {
            get { return _selectedRule; }
            set
            {
                _selectedRule = value;
                OnPropertyChanged();
            }
        }

        public bool Cast(SelfHealBuffRule rule)
        {
            
                if (rule != null)
                {
                    if (rule.RequiresTarget)
                        if (rule.UseOnPet && _data.MainHero.PlayerSummons.Count > 0)
                        {
                            if (_actionsController.TargetByObjectId(_data.MainHero.PlayerSummons.First().ObjectId))
                            {
                                if (rule.NukeType == NukeType.Skill)
                                    _actionsController.UseSkill(rule.SelfHealBuffId, false, false);
                                else if (rule.NukeType == NukeType.Item)
                                    _actionsController.UseItem(_data.Inventory.First(item => item.Value.ItemId == rule.SelfHealBuffId).Key, false);

                                rule.LastUsed = Environment.TickCount;
                                _data.RequestBuffSkillCastStamp = Environment.TickCount;
                                return true;
                            }
                        }
                        else
                        {
                            if (_actionsController.TargetByObjectId(_data.MainHero.ObjectId))
                            {
                                if (rule.NukeType == NukeType.Skill)
                                    _actionsController.UseSkill(rule.SelfHealBuffId, false, false);
                                else if (rule.NukeType == NukeType.Item)
                                    _actionsController.UseItem(_data.Inventory.First(item => item.Value.ItemId == rule.SelfHealBuffId).Key, false);

                                rule.LastUsed = Environment.TickCount;
                                _data.RequestBuffSkillCastStamp = Environment.TickCount;
                                return true;
                            }
                        }
                    else
                    {
                        if (rule.NukeType == NukeType.Skill)
                            _actionsController.UseSkill(rule.SelfHealBuffId, false, false);
                        else if (rule.NukeType == NukeType.Item)
                            _actionsController.UseItem(_data.Inventory.First(item => item.Value.ItemId == rule.SelfHealBuffId).Key, false);

                        rule.LastUsed = Environment.TickCount;
                        _data.RequestBuffSkillCastStamp = Environment.TickCount;
                        return true;
                    }
                }

            return false;
        }

        private SelfHealBuffRule _latestRule = null;
        private long _timeStamp = 0;

        public SelfHealBuffRule GetRuleForCast(bool inCombat)
        {
            if (Math.Abs(Environment.TickCount - _timeStamp) < 50)
            {
                return _latestRule;
            }

            _timeStamp = Environment.TickCount;

            SelfHealBuffRule nuke = null;
            foreach (var nukeRule in NukesToUse)
            {
                if ((nukeRule.NukeType == NukeType.Item ? !_data.Inventory.Any(item => item.Value.ItemId == nukeRule.SelfHealBuffId) : false) ||
                (nukeRule.NukeType == NukeType.Skill ? !_data.Skills.Any(skill => skill.Value.SkillId == nukeRule.SelfHealBuffId) : false))
                    continue;

                GameFigure condFigure = _data.MainHero;

                if (!nukeRule.Enabled)
                    continue;

                //cond checks
                if (condFigure != null)
                {
                    if (nukeRule.HealthPercentOver > 0 && nukeRule.HealthPercentOver > condFigure.HealthPercent)
                        continue;

                    if (nukeRule.HealthPercentBelow > 0 && nukeRule.HealthPercentBelow < condFigure.HealthPercent)
                        continue;

                    if (nukeRule.ManaPercentOver > 0 && nukeRule.ManaPercentOver > condFigure.ManaPercent)
                        continue;

                    if (nukeRule.ManaPercentBelow > 0 && nukeRule.ManaPercentBelow < condFigure.ManaPercent)
                        continue;

                    if (condFigure is MainHero && nukeRule.CombatPointsPercentOver > 0 && nukeRule.CombatPointsPercentOver > condFigure.CombatPointsPercent)
                        continue;

                    if (condFigure is MainHero && nukeRule.CombatPointsPercentBelow > 0 && nukeRule.CombatPointsPercentBelow < condFigure.CombatPointsPercent)
                        continue;
                }

                condFigure = _data.MainHero.PlayerSummons.Count > 0 ? _data.MainHero.PlayerSummons.First() : null;

                if (condFigure != null)
                {
                    if (nukeRule.PetHealthPercentOver > 0 && nukeRule.PetHealthPercentOver > condFigure.HealthPercent)
                        continue;

                    if (nukeRule.PetHealthPercentBelow > 0 && nukeRule.PetHealthPercentBelow < condFigure.HealthPercent)
                        continue;

                    if (nukeRule.PetManaPercentOver > 0 && nukeRule.PetManaPercentOver > condFigure.ManaPercent)
                        continue;

                    if (nukeRule.PetManaPercentBelow > 0 && nukeRule.PetManaPercentBelow < condFigure.ManaPercent)
                        continue;
                }

                if(nukeRule.PetSummonCountBelow > 0 && _data.MainHero.PlayerSummons.Count >= nukeRule.PetSummonCountBelow)
                    continue;

                if (Math.Abs(Environment.TickCount - nukeRule.LastUsed) < nukeRule.Interval * 1000)
                    continue;

                if(nukeRule.HasDeathPenalty && _data.MainHero.DeathPenaltyLevel == 0)
                    continue;

                if (nukeRule.UseIfBuffIsMissing && nukeRule.SelectedBuffsStr.Trim().Length > 0
                   &&
                   nukeRule.SelectedBuffsFilter.Any(
                       buffRule =>
                           buffRule.Enable &&
                           _data.MainHero.Buffs.Any(
                               buff =>
                                       buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase))))
                {
                    var rule = nukeRule.SelectedBuffsFilter.FirstOrDefault(
                        buffRule =>
                            buffRule.Enable &&
                            _data.MainHero.Buffs.Any(
                                buff =>
                                        buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase)));
                    if(rule == null)
                        continue;

                    var name = rule.Name;

                    if (nukeRule.TimerCheckActivated)
                    {
                        var seconds =
                            _data.MainHero.Buffs.First(
                                    buff => buff.Value.BuffName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                .Value.SecondsLeft;
                        if (nukeRule.TimerComparisonTypeEnum == ComparisonType.Less && seconds > nukeRule.TimerSeconds)
                            continue;
                        else if (nukeRule.TimerComparisonTypeEnum == ComparisonType.More && seconds < nukeRule.TimerSeconds)
                            continue;
                    }

                    if (nukeRule.SkillLevelComparisonActivated)
                    {
                        var level =
                            _data.MainHero.Buffs.First(
                                    buff => buff.Value.BuffName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                .Value.Level;
                        if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Less && level > nukeRule.SkillLevelRequired)
                            continue;
                        else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.More && level < nukeRule.SkillLevelRequired)
                            continue;
                        else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Equal && level != nukeRule.SkillLevelRequired)
                            continue;
                    }
                }

                if (nukeRule.UseIfBuffIsMissing && nukeRule.SelectedBuffsStr.Trim().Length > 0 && !nukeRule.TimerCheckActivated && !nukeRule.SkillLevelComparisonActivated
                    && nukeRule.SelectedBuffsFilter.Any(buffRule => buffRule.Enable && _data.MainHero.Buffs.Any(buff => buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase))))
                    continue;

                if (nukeRule.UseIfBuffIsPresent && nukeRule.SelectedBuffsStr.Trim().Length > 0
                    &&
                    nukeRule.SelectedBuffsFilter.Any(
                        buffRule =>
                            buffRule.Enable &&
                            _data.MainHero.Buffs.Any(
                                buff =>
                                        buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase))))
                {
                    var rule = nukeRule.SelectedBuffsFilter.First(
                        buffRule =>
                            buffRule.Enable &&
                            _data.MainHero.Buffs.Any(
                                buff =>
                                        buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase)));
                    var name = rule.Name;

                    if (nukeRule.TimerCheckActivated)
                    {
                        var seconds =
                            _data.MainHero.Buffs.First(
                                    buff => buff.Value.BuffName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                .Value.SecondsLeft;
                        if (nukeRule.TimerComparisonTypeEnum == ComparisonType.Less && seconds > nukeRule.TimerSeconds)
                            continue;
                        else if (nukeRule.TimerComparisonTypeEnum == ComparisonType.More && seconds < nukeRule.TimerSeconds)
                            continue;
                    }

                    if (nukeRule.SkillLevelComparisonActivated)
                    {
                        var level =
                            _data.MainHero.Buffs.First(
                                    buff => buff.Value.BuffName.Equals(name, StringComparison.OrdinalIgnoreCase))
                                .Value.Level;
                        if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Less && level > nukeRule.SkillLevelRequired)
                            continue;
                        else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.More && level < nukeRule.SkillLevelRequired)
                            continue;
                        else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Equal && level != nukeRule.SkillLevelRequired)
                            continue;
                    }
                }

                if (nukeRule.UseIfBuffIsPresent && nukeRule.SelectedBuffsStr.Trim().Length > 0
                    &&
                    !nukeRule.SelectedBuffsFilter.Any(
                        buffRule =>
                            buffRule.Enable &&
                            _data.MainHero.Buffs.Any(
                                buff =>
                                        buff.Value.BuffName.Equals(buffRule.Name, StringComparison.OrdinalIgnoreCase))))
                    continue;

                if (nukeRule.NukeType == NukeType.Skill && !_data.Skills[nukeRule.SelfHealBuffId].CanBeUsed())
                    continue;

                if (!nukeRule.UseInCombat && inCombat)
                    continue;

                if (nukeRule.NukeType == NukeType.Skill && _data.MainHero.Mana < ExportedData.SkillManaConsumption[nukeRule.SelfHealBuffId][_data.Skills[nukeRule.SelfHealBuffId].SkillLevel])
                    continue;

                nuke = nukeRule;
                break;

            }

            _latestRule = nuke;
            return nuke;
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> SelectedRemovalBuffsFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                var buffsSplit = SelectedRemovalBuffsStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var buff in buffsSplit)
                {
                    if (ExportedData.SkillsIdToName.Any(pair => pair.Value.Equals(buff, StringComparison.OrdinalIgnoreCase)))
                        list.Add(new UIFormElement { Enable = true, Name = buff });
                }

                foreach (var buffKeyPair in _data.MainHero.Buffs)
                {
                    if (!list.Any(unit => ((string)unit.Name).Equals(buffKeyPair.Value.BuffName, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(new UIFormElement { Enable = false, Name = buffKeyPair.Value.BuffName });
                    }
                }

                return list;
            }
        }

        private bool _removeBuffsOnSelf = true;
        private bool _removeBuffsOnPet;

        private string _selectedRemovalBuffsStr = string.Empty;

        public string SelectedRemovalBuffsStr
        {
            get { return _selectedRemovalBuffsStr; }
            set
            {
                _selectedRemovalBuffsStr = value;
                OnPropertyChanged();
            }
        }

        public void RemoveUnwantedBuffs()
        {
            if (_actionsController is IBuffRemover)
                if (RemoveBuffsOnSelf)
                {
                    foreach (var buff in SelectedRemovalBuffsFilter)
                    {
                        if (buff.Enable &&
                            _data.MainHero.Buffs.Any(
                                spell =>
                                    spell.Value.BuffName.Trim()
                                        .Equals(buff.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            var buf = _data.MainHero.Buffs.First(
                                spell =>
                                    spell.Value.BuffName.Trim()
                                        .Equals(buff.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                            ((IBuffRemover)_actionsController).RemoveBuff(_data.MainHero.ObjectId, buf.Value.Id, buf.Value.Level);
                            return;
                        }
                    }
                }
                else if (RemoveBuffsOnPet && _data.MainHero.PlayerSummons.Count > 0)
                {
                    foreach (var buff in SelectedRemovalBuffsFilter)
                    {
                        if (buff.Enable &&
                            _data.MainHero.PlayerSummons.Any(summ => summ.Buffs.Any(
                                spell =>
                                    spell.Value.BuffName.Trim()
                                        .Equals(buff.Name.Trim(), StringComparison.OrdinalIgnoreCase))))
                        {
                            var pet = _data.MainHero.PlayerSummons.First(summ => summ.Buffs.Any(
                                spell =>
                                    spell.Value.BuffName.Trim()
                                        .Equals(buff.Name.Trim(), StringComparison.OrdinalIgnoreCase)));
                            var buf = pet.Buffs.First(
                                spell =>
                                    spell.Value.BuffName.Trim()
                                        .Equals(buff.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                            ((IBuffRemover)_actionsController).RemoveBuff(pet.ObjectId, buf.Value.Id, buf.Value.Level);
                            return;
                        }
                    }
                }
        }

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

        public override void Init(L2PlayerData data, ActionsController actionsController)
        {
            base.Init(data, actionsController);

            data.Skills.CollectionChanged +=
                (obj, args) =>
                {
                    OnPropertyChanged(nameof(NukesToAdd));
                };

            data.Inventory.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(NukesToAdd));
            };
        }

        private MultiThreadObservableCollection<SelfHealBuffRule> _nukesToUse = new MultiThreadObservableCollection<SelfHealBuffRule>();

        public MultiThreadObservableCollection<SelfHealBuffRule> NukesToUse
        {
            get
            {
                return _nukesToUse;
            }
            set { _nukesToUse = value; }
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
                                if (!skill.Value.IsPassive && !skill.Value.IsDisabled)
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

        public bool RemoveBuffsOnSelf
        {
            get { return _removeBuffsOnSelf; }
            set { _removeBuffsOnSelf = value; }
        }

        public bool RemoveBuffsOnPet
        {
            get { return _removeBuffsOnPet; }
            set { _removeBuffsOnPet = value; }
        }
    }
}
