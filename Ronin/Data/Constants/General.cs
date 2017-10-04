using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Constants
{
    public enum GameState
    {
        AccountLogin,
        CharacterSelection,
        InGame
    }

    public enum NukeType
    {
        Skill,
        Item,
        PetSkill
    }

    public enum TargetType
    {
        Self,
        Target
    }

    public enum FilterType
    {
        Inclusive,
        Exclusive
    }

    public enum CombatTargetType
    {
        Off,
        AroundChar,
        AroundPoint
    }

    public enum AssistType
    {
        AttackInstant,
        WaitForFirstAttack,
        WaitCustomDelay,
        WaitRandomDelay
    }

    public enum FollowType
    {
        PartyLeader,
        PlayerList
    }

    public enum NukeConditions
    {
        TargetHPBelowPercent,
        TargetHPOverPercent,
        TargetMPBelowPercent,
        TargetMPOverPercent,
        TargetIsDead,
        TargetIsSpoiled
    }

    public enum PartyType
    {
        FindersKeepers,
        Random,
        RandomIncludingSpoil,
        ByTurn,
        ByTurnIncludingSpoil
    }
}
