using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class DropItem : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.DropItem;

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
            item.ItemQuantity = reader.ReadInt();
            data.DroppedItems.Add(item.ObjectId, item);
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
