using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class DroppedItem : Locatable
    {
        public int SourceMobObjectId;
        public int ObjectId;
        public int ItemId;
        public long ItemQuantity;
        public bool IsEquipped;

        //public string ItemName
        //{
        //    get
        //    {
        //        return ExportedData.ItemIdToName[ItemId];
        //    }
        //}
    }
}
