using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class DropItem : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.Dropitem;

        public DropItem(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            DroppedItem item = new DroppedItem();
            item.SourceMobObjectId = reader.ReadInt();
            item.ObjectId = reader.ReadInt();
            item.ItemId = reader.ReadInt();
            item.X = reader.ReadInt();
            item.Y = reader.ReadInt();
            item.Z = reader.ReadInt();
            reader.ReadInt(); //isStackable
            item.ItemQuantity = reader.ReadLong();
            data.DroppedItems.Add(item.ObjectId, item);
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
