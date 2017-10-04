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
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class PartyHealBuffHandler : LogicHandler
    {
        private PartyHealBuffRule _selectedRule;
        private bool _outOfParty;
        private int _outOfPartyTimer = 5;
        public long OutOfPartyStamp = 0;

        public PartyHealBuffRule SelectedRule
        {
            get { return _selectedRule; }
            set
            {
                _selectedRule = value;
                OnPropertyChanged();
            }
        }

        public bool Cast(PartyHealBuffRule rule)
        {
            if ((rule.NukeType == NukeType.Item ? _data.Inventory.Any(item => item.Value.ItemId == rule.PartyHealBuffId) : true) &&
                (rule.NukeType == NukeType.Skill ? _data.Skills.Any(skill => skill.Value.SkillId == rule.PartyHealBuffId) : true))
                if (rule != null)
                {
                    if (rule.RequiresTarget)
                    {
                        if (rule.ChosenTarget == null || _data.SurroundingPlayers.All(player => player.ObjectId != rule.ChosenTarget.ObjectId))
                        {
                            LogHelper.GetLogger().Debug("Invalid target");
                            return false;
                        }

                        if (_actionsController.TargetByObjectId(rule.ChosenTarget.ObjectId))
                        {
                            if (rule.NukeType == NukeType.Skill)
                                _actionsController.UseSkill(rule.PartyHealBuffId, false, false);
                            else if (rule.NukeType == NukeType.Item)
                                _actionsController.UseItem(
                                    _data.Inventory.First(item => item.Value.ItemId == rule.PartyHealBuffId).Key, false);

                            rule.LastUsed = Environment.TickCount;
                            _data.RequestBuffSkillCastStamp = Environment.TickCount;
                            OutOfPartyStamp = Environment.TickCount;
                            return true;
                        }
                    }
                    else
                    {
                        if (rule.NukeType == NukeType.Skill)
                            _actionsController.UseSkill(rule.PartyHealBuffId, false, false);
                        else if (rule.NukeType == NukeType.Item)
                            _actionsController.UseItem(
                                _data.Inventory.First(item => item.Value.ItemId == rule.PartyHealBuffId).Key, false);

                        _data.RequestBuffSkillCastStamp = Environment.TickCount;
                        rule.LastUsed = Environment.TickCount;
                        OutOfPartyStamp = Environment.TickCount;
                        return true;
                    }
                }

            return false;
        }

        private PartyHealBuffRule _latestRule = null;
        private long _timeStamp = 0;

        public PartyHealBuffRule GetRuleForCast(bool inCombat)
        {
            if (Math.Abs(Environment.TickCount - _timeStamp) < 50)
            {
                return _latestRule;
            }

            _timeStamp = Environment.TickCount;
            PartyHealBuffRule nuke = null;
            foreach (var nukeRule in NukesToUse)
            {
                if ((nukeRule.NukeType == NukeType.Item ? !_data.Inventory.Any(item => item.Value.ItemId == nukeRule.PartyHealBuffId) : false) ||
                (nukeRule.NukeType == NukeType.Skill ? !_data.Skills.Any(skill => skill.Value.SkillId == nukeRule.PartyHealBuffId) : false))
                    continue;

                foreach (var partyUnit in nukeRule.PlayersToBuff)
                {
                    if (partyUnit.Enable)
                    {
                        Player ptMember =
                            _data.PartyMembers.FirstOrDefault(
                                member =>
                                    member.Name.Equals(
                                        partyUnit.Name.Split(new string[] { "'s" }, StringSplitOptions.None)[0]));

                        if(ptMember == null)
                            continue;

                        GameFigure target = (partyUnit.Name.Contains("'s")
                            ? (GameFigure)ptMember.PlayerSummons.FirstOrDefault(
                                summ =>
                                    summ.Name.Trim()
                                        .Equals(
                                            partyUnit.Name.Split(new string[] { "'s" }, StringSplitOptions.None)[1].Trim(),
                                            StringComparison.OrdinalIgnoreCase))
                            : (GameFigure)ptMember);

                        if(target == null)
                            continue;

                        GameFigure condFigure = _data.MainHero;

                        if(_data.SurroundingPlayers.All(player => player.ObjectId != target.ObjectId))
                            continue;

                        if (!nukeRule.Enabled)
                            continue;

                        //cond checks
                        if (condFigure != null)
                        {
                            if (nukeRule.HealthPercentOver > 0 &&
                                nukeRule.HealthPercentOver > condFigure.HealthPercent)
                                continue;

                            if (nukeRule.HealthPercentBelow > 0 &&
                                nukeRule.HealthPercentBelow < condFigure.HealthPercent)
                                continue;

                            if (nukeRule.ManaPercentOver > 0 && nukeRule.ManaPercentOver > condFigure.ManaPercent)
                                continue;

                            if (nukeRule.ManaPercentBelow > 0 && nukeRule.ManaPercentBelow < condFigure.ManaPercent)
                                continue;

                            if (condFigure is MainHero && nukeRule.CombatPointsPercentOver > 0 &&
                                nukeRule.CombatPointsPercentOver > condFigure.CombatPointsPercent)
                                continue;

                            if (condFigure is MainHero && nukeRule.CombatPointsPercentBelow > 0 &&
                                nukeRule.CombatPointsPercentBelow < condFigure.CombatPointsPercent)
                                continue;
                        }

                        condFigure = target;

                        if (condFigure != null)
                        {
                            if (nukeRule.PartyMemberHealthPercentOver > 0 && nukeRule.PartyMemberHealthPercentOver > condFigure.HealthPercent)
                                continue;

                            if (nukeRule.PartyMemberHealthPercentBelow > 0 && nukeRule.PartyMemberHealthPercentBelow < condFigure.HealthPercent)
                                continue;

                            if (nukeRule.PartyMemberManaPercentOver > 0 && nukeRule.PartyMemberManaPercentOver > condFigure.ManaPercent)
                                continue;

                            if (nukeRule.PartyMemberManaPercentBelow > 0 && nukeRule.PartyMemberManaPercentBelow < condFigure.ManaPercent)
                                continue;

                            if (nukeRule.PartyMemberCombatPointsPercentOver > 0 && nukeRule.PartyMemberCombatPointsPercentOver > condFigure.CombatPointsPercent)
                                continue;

                            if (nukeRule.PartyMemberCombatPointsPercentBelow > 0 && nukeRule.PartyMemberCombatPointsPercentBelow < condFigure.CombatPointsPercent)
                                continue;

                            if (nukeRule.PartyMemberIsDead && !condFigure.IsDead)
                                continue;

                            if (!nukeRule.PartyMemberIsDead && condFigure.IsDead)
                                continue;
                        }

                        int health=0, maxHealth=0, ptMembersAroundCount =0;

                        if(nukeRule.AveragePartyMemberHealthPercentOver > 0 || nukeRule.AveragePartyMemberHealthPercentBelow > 0)
                        foreach (var partyMember in _data.PartyMembers)
                        {
                            if (_data.MainHero.RangeTo(partyMember) < 1000)
                            {
                                ptMembersAroundCount++;
                                health += partyMember.Health;
                                maxHealth += partyMember.MaxHealth;
                            }
                        }

                        double avrgPtHealthPercent = ptMembersAroundCount > 0 ? (health/(double)maxHealth)*100.0 : 0;
                        
                        if (nukeRule.AveragePartyMemberHealthPercentOver > 0 && avrgPtHealthPercent > 0 && nukeRule.AveragePartyMemberHealthPercentOver > avrgPtHealthPercent)
                            continue;

                        if (nukeRule.AveragePartyMemberHealthPercentBelow > 0 && avrgPtHealthPercent > 0 && nukeRule.AveragePartyMemberHealthPercentBelow < avrgPtHealthPercent)
                            continue;

                        if(nukeRule.DeadPartyMembersOver > 0 && nukeRule.DeadPartyMembersOver > _data.PartyMembers.Count(member => member.IsDead))
                            continue;

                        if (nukeRule.DeadPartyMembersBelow > 0 && nukeRule.DeadPartyMembersBelow < _data.PartyMembers.Count(member => member.IsDead))
                            continue;

                        if (Math.Abs(Environment.TickCount - nukeRule.LastUsed) < nukeRule.Interval * 1000)
                            continue;

                        if (nukeRule.UseIfBuffIsMissing && nukeRule.SelectedBuffsStr.Trim().Length > 0
                            &&
                            nukeRule.SelectedBuffsFilter.Any(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase))))
                        {
                            var rule = nukeRule.SelectedBuffsFilter.First(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase)));
                            var name = rule.Name;

                            if (nukeRule.TimerCheckActivated)
                            {
                                var seconds =
                                    target.Buffs.First(
                                            buff =>
                                                buff.Value.BuffName.Equals(name,
                                                    StringComparison.OrdinalIgnoreCase))
                                        .Value.SecondsLeft;
                                if (nukeRule.TimerComparisonTypeEnum == ComparisonType.Less &&
                                    seconds > nukeRule.TimerSeconds)
                                    continue;
                                else if (nukeRule.TimerComparisonTypeEnum == ComparisonType.More &&
                                         seconds < nukeRule.TimerSeconds)
                                    continue;
                            }

                            if (nukeRule.SkillLevelComparisonActivated)
                            {
                                var level =
                                    target.Buffs.First(
                                            buff =>
                                                buff.Value.BuffName.Equals(name,
                                                    StringComparison.OrdinalIgnoreCase))
                                        .Value.Level;
                                if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Less &&
                                    level > nukeRule.SkillLevelRequired)
                                    continue;
                                else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.More &&
                                         level < nukeRule.SkillLevelRequired)
                                    continue;
                                else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Equal &&
                                         level != nukeRule.SkillLevelRequired)
                                    continue;
                            }
                        }

                        if (nukeRule.UseIfBuffIsMissing && nukeRule.SelectedBuffsStr.Trim().Length > 0 &&
                            !nukeRule.TimerCheckActivated && !nukeRule.SkillLevelComparisonActivated
                            &&
                            nukeRule.SelectedBuffsFilter.Any(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase))))
                            continue;

                        if (nukeRule.UseIfBuffIsPresent && nukeRule.SelectedBuffsStr.Trim().Length > 0
                            &&
                            nukeRule.SelectedBuffsFilter.Any(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase))))
                        {
                            var rule = nukeRule.SelectedBuffsFilter.First(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase)));
                            var name = rule.Name;

                            if (nukeRule.TimerCheckActivated)
                            {
                                var seconds =
                                    target.Buffs.First(
                                            buff =>
                                                buff.Value.BuffName.Equals(name,
                                                    StringComparison.OrdinalIgnoreCase))
                                        .Value.SecondsLeft;
                                if (nukeRule.TimerComparisonTypeEnum == ComparisonType.Less &&
                                    seconds > nukeRule.TimerSeconds)
                                    continue;
                                else if (nukeRule.TimerComparisonTypeEnum == ComparisonType.More &&
                                         seconds < nukeRule.TimerSeconds)
                                    continue;
                            }

                            if (nukeRule.SkillLevelComparisonActivated)
                            {
                                var level =
                                    target.Buffs.First(
                                            buff =>
                                                buff.Value.BuffName.Equals(name,
                                                    StringComparison.OrdinalIgnoreCase))
                                        .Value.Level;
                                if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Less &&
                                    level > nukeRule.SkillLevelRequired)
                                    continue;
                                else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.More &&
                                         level < nukeRule.SkillLevelRequired)
                                    continue;
                                else if (nukeRule.SkillLevelComparisonTypeEnum == ComparisonType.Equal &&
                                         level != nukeRule.SkillLevelRequired)
                                    continue;
                            }
                        }

                        if (nukeRule.UseIfBuffIsPresent && nukeRule.SelectedBuffsStr.Trim().Length > 0
                            &&
                            !nukeRule.SelectedBuffsFilter.Any(
                                buffRule =>
                                    buffRule.Enable &&
                                    target.Buffs.Any(
                                        buff =>
                                            buff.Value.BuffName.Equals(buffRule.Name,
                                                StringComparison.OrdinalIgnoreCase))))
                            continue;

                        if (nukeRule.NukeType == NukeType.Skill && !_data.Skills[nukeRule.PartyHealBuffId].CanBeUsed())
                            continue;

                        if (!nukeRule.UseInCombat && inCombat)
                            continue;

                        if (nukeRule.NukeType == NukeType.Skill && _data.MainHero.Mana <
                            ExportedData.SkillManaConsumption[nukeRule.PartyHealBuffId][
                                _data.Skills[nukeRule.PartyHealBuffId].SkillLevel] * 0.75)
                            continue;

                        if(nukeRule.RequiresTarget)
                        if(_data.MainHero.RangeTo(target) > nukeRule.MaxDistance)
                            continue;

                        nuke = nukeRule;
                        nuke.ChosenTarget = target;
                        break;
                    }
                }
            }

            _latestRule = nuke;
            return nuke;
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

        private MultiThreadObservableCollection<PartyHealBuffRule> _nukesToUse = new MultiThreadObservableCollection<PartyHealBuffRule>();

        public MultiThreadObservableCollection<PartyHealBuffRule> NukesToUse
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
                if (Initialiased && _data != null)
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

        public bool OutOfParty
        {
            get { return _outOfParty; }
            set { _outOfParty = value; }
        }

        public int OutOfPartyTimer
        {
            get { return _outOfPartyTimer; }
            set { _outOfPartyTimer = value; }
        }
    }
}
