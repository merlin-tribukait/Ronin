using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;

namespace Ronin.Data.Structures
{
    public class Player : GameFigure
    {
        public string Name;
        public L2Class Class;
        public L2Race Race;
        public Sex Sex;
        public int Gear_Helmet;
        public int Gear_Weapon;
        public int Gear_ShieldSigil;
        public int Gear_Gloves;
        public int Gear_UpperBody;
        public int Gear_LowerBody;
        public int Gear_Boots;
        public int Gear_Cloak;
        public int Gear_WeaponTwoHand; //it will replicate the Weapon ID if the weapon is two handed.
        public int PvpFlag;
        public int Karma;
        public int CastingSpeed;
        public int AttackSpeed;
        public bool IsSitting;
        public bool IsMyPartyMember;
        public bool IsMyPartyLeader;
        /// <summary>
        /// Collection containing all creatures the player has summoned.
        /// </summary>
        public List<Npc> PlayerSummons = new List<Npc>();
        
        public int RunningSpeedWater;
        public int WalkingSpeedWater;
        public int RunningSpeedMounted;
        public int WalkingSpeedMounted;
        public int RunningSpeedFlyMounted;
        public int WalkingSpeedFlyMounted;

        public double AttackSpeedMultiplier;
        public bool InCombat;
        public bool IsNoble;
        public bool IsHero;
    }
}
