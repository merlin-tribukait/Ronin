using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class StashedItem
    {
        public int ObjectId;
        public int ItemId;
        public long ItemQuantity;
        public bool IsEquipped;
        public bool IsQuestItem;
        private string itemName;

        public StashedItem()
        {

        }

        public StashedItem(int objectId, int itemId, long itemQuantity)
        {
            ObjectId = objectId;
            ItemId = itemId;
            ItemQuantity = itemQuantity;
        }

        public string ItemName
        {
            get
            {
                itemName = ExportedData.ItemIdToName[ItemId];
                return itemName;
            }
            set { itemName = value; }
        }
    }
}
