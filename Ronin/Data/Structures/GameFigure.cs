using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;
using Ronin.Utilities;

namespace Ronin.Data.Structures
{
    public class GameFigure : Locatable
    {
        private int objectId;
        private int unitId;
        private int health;
        private int maxHealth;
        private int mana;
        private int maxMana;
        private int combatPoints;
        private int maxCombatPoints;
        private int level;
        private bool isDead;
        private int targetObjectId;
        private string title;
        public long TargetStamp;
        public int LastUnitAttackedObjectId;

        /// <summary>
        /// Collection containing all the character buffs.
        /// </summary>
        public MultiThreadObservableDictionary<int, Buff> Buffs = new MultiThreadObservableDictionary<int, Buff>();

        public int ObjectId
        {
            get
            {
                return objectId;
            }

            set
            {
                objectId = value;
            }
        }

        public int UnitId
        {
            get
            {
                return unitId;
            }

            set
            {
                unitId = value;
            }
        }

        public int Health
        {
            get { return this.health; }
            set
            {
                this.health = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HealthPercent));
            }
        }

        public int MaxHealth
        {
            get { return this.maxHealth; }
            set
            {
                this.maxHealth = value;
                OnPropertyChanged();
            }
        }

        public int Mana
        {
            get { return this.mana; }
            set
            {
                this.mana = value;
                OnPropertyChanged(nameof(ManaPercent));
                OnPropertyChanged();
            }
        }

        public int MaxMana
        {
            get { return this.maxMana; }
            set
            {
                this.maxMana = value;
                OnPropertyChanged();
            }
        }

        public int CombatPoints
        {
            get { return this.combatPoints; }
            set
            {
                this.combatPoints = value;
                OnPropertyChanged(nameof(CombatPointsPercent));
                OnPropertyChanged();
            }
        }

        public int MaxCombatPoints
        {
            get { return this.maxCombatPoints; }
            set
            {
                this.maxCombatPoints = value;
                OnPropertyChanged();
            }
        }

        public double CombatPointsPercent
        {
            get { return MaxHealth > 0 ? (((double)CombatPoints / MaxCombatPoints) * 100.0) : 0; }
        }

        public double HealthPercent
        {
            get { return MaxHealth > 0 ? (((double)Health / MaxHealth) * 100.0) : 0; }
        }

        public double ManaPercent
        {
            get { return MaxMana > 0 ? (((double)Mana / MaxMana) * 100.0) : 0; }
        }

        public int Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
                OnPropertyChanged();
            }
        }

        public bool IsDead
        {
            get
            {
                if (Health > 0 && MaxHealth > 0)
                    return false;

                return isDead;
            }

            set
            {
                isDead = value;
            }
        }

        public int TargetObjectId
        {
            get
            {
                return targetObjectId;
            }

            set
            {
                targetObjectId = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
    }
}
