using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class PickupRule
    {
        public int ItemId;
        private int quantityMinimum;
        private int quantityMaximum;
        private double healthBelow;
        private double healthOver;
        private double manaBelow;
        private double manaOver;
        private string pickupConditions;
        private string itemName;
        private bool _enable = true;

        public PickupRule(int itemId, double healthBelow = 0, double healthOver = 0, double manaBelow = 0, double manaOver = 0, int quantityMinimum = 0, int quantityMaximum = 0)
        {
            this.ItemId = itemId;
            this.healthBelow = healthBelow;
            this.healthOver = healthOver;
            this.manaBelow = manaBelow;
            this.manaOver = manaOver;
            this.quantityMinimum = quantityMinimum;
            this.quantityMaximum = quantityMaximum;
        }

        public double HealthBelow
        {
            get { return healthBelow; }
            set { healthBelow = Convert.ToInt32(value); }
        }

        public double HealthOver
        {
            get { return healthOver; }
            set { healthOver = Convert.ToInt32(value); }
        }

        public double ManaBelow
        {
            get { return manaBelow; }
            set { manaBelow = Convert.ToInt32(value); }
        }

        public double ManaOver
        {
            get { return manaOver; }
            set { manaOver = Convert.ToInt32(value); }
        }

        public int QuantityMinimum
        {
            get { return this.quantityMinimum; }
            set { this.quantityMinimum = value; }
        }

        public int QuantityMaximum
        {
            get { return this.quantityMaximum; }
            set { this.quantityMaximum = value; }
        }

        public string ItemName
        {
            get { return ExportedData.ItemIdToName[ItemId]; }
            set { itemName = value; }
        }

        public string PickupConditions
        {
            get
            {
                StringBuilder str = new StringBuilder();
                //str.Append("If: ");

                if (this.quantityMinimum > 0)
                {
                    str.Append("Quantity > " + this.quantityMinimum);
                }

                if (this.quantityMaximum > 0)
                {
                    str.Append("Quantity < " + this.quantityMaximum);
                }

                if (healthBelow > 0)
                {
                    str.Append("HP < " + healthBelow + "% ");
                }

                if (healthOver > 0)
                {
                    str.Append("HP > " + healthOver + "% ");
                }

                if (manaBelow > 0)
                {
                    str.Append("MP <" + manaBelow + "% ");
                }

                if (manaOver > 0)
                {
                    str.Append("MP >" + manaOver + "% ");
                }

                this.pickupConditions = str.ToString();
                return this.pickupConditions;
            }
            set { this.pickupConditions = value; }
        }

        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; }
        }

        public bool ConditionsAreMet(L2PlayerData data, DroppedItem item)
        {
            double currentHealthPercent = (data.MainHero.Health / (double)data.MainHero.MaxHealth) * 100;
            double currentManaPercent = (data.MainHero.Mana / (double)data.MainHero.MaxMana) * 100;
            if (this.HealthBelow > 0 && currentHealthPercent > this.healthBelow)
            {
                return false;
            }

            if (this.HealthOver > 0 && currentHealthPercent < this.healthOver)
            {
                return false;
            }

            if (this.ManaBelow > 0 && currentManaPercent > this.manaBelow)
            {
                return false;
            }

            if (this.ManaOver > 0 && currentManaPercent < this.manaOver)
            {
                return false;
            }

            if (quantityMinimum > 0 && item.ItemQuantity < this.quantityMinimum)
            {
                return false;
            }

            if (quantityMaximum > 0 && item.ItemQuantity > this.quantityMaximum)
            {
                return false;
            }

            return true;
        }
    }
}
