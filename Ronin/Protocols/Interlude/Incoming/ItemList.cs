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
    public class ItemList : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.ItemList;

        public ItemList(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadShort();//show window
            int size = reader.ReadShort();
            data.Inventory.Clear();
            for (int i = 0; i < size; i++)
            {
                StashedItem item = new StashedItem();
                reader.ReadShort();//type1
                item.ObjectId = reader.ReadInt(); //writeD(item.getObjectId());
                item.ItemId = reader.ReadInt();//writeD(item.getdisplayId() > 0 ? item.getdisplayId() : item.getItemId());
                item.ItemQuantity = reader.ReadInt();//writeD (count);
                reader.ReadShort(); //writeH(item.getTemplate().getType2ForPackets());
                reader.ReadShort(); //writeH(item.getCustomType1());
                item.IsEquipped = reader.ReadShort() == 1; //writeH(item.isEquipped() ? 1 : 0);
                reader.ReadInt(); //writeD(item.getBodyPart());
                reader.ReadShort(); //writeH(item.getEnchantLevel());
                reader.ReadShort(); //writeH(item.getCustomType2());
                reader.ReadInt(); //writeD(item.getAugmentationId());
                reader.ReadInt(); //writeD(item.getShadowLifeTime()); mana
                data.Inventory.Add(item.ObjectId, item);
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
