using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;
using Ronin.Data.Constants;

namespace Ronin.Data.Structures
{
    public class MainHero : GameFigure
    {
        private string name;
        private int nameLength;
        private L2Class classId;
        private L2Race race;
        private int invUsedSlots;
        private int invMaxSlots;
        public Sex Sex;
        public long Experience;
        public long SkillPonts;
        public bool IsSitting;
        public int _weaponDisplayId;
        public int DeathPenaltyLevel;

        public int ValidatedX, ValidatedY, ValidatedZ;
        
        /// <summary>
        /// Collection containing all creatures the player has summoned.
        /// </summary>
        public List<Npc> PlayerSummons = new List<Npc>();

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                OnPropertyChanged();
            }
        }

        public int NameLength
        {
            get { return this.nameLength; }
            set { this.nameLength = value; }
        }

        public L2Class ClassId
        {
            get { return this.classId; }
            set
            {
                this.classId = value;
                OnPropertyChanged();
            }
        }

        public L2Race Race
        {
            get { return this.race; }
            set { this.race = value; }
        }

        
        public int InvUsedSlots
        {
            get { return this.invUsedSlots; }
            set { this.invUsedSlots = value; }
        }

        public int InvMaxSlots
        {
            get { return this.invMaxSlots; }
            set { this.invMaxSlots = value; }
        }

        public int WeaponDisplayId
        {
            get { return _weaponDisplayId; }
            set
            {
                _weaponDisplayId = value; 
                OnPropertyChanged();
            }
        }
    }
}
