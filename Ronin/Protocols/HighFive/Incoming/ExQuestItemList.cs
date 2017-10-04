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
    public class ExQuestItemList : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.Extended;
        private H5PacketIds.ServerSecondary _subId = H5PacketIds.ServerSecondary.QuestItemList;

        public override H5PacketIds.ServerSecondary SubId
        {
            get { return _subId; }
            set { _subId = value; }
        }

        public ExQuestItemList(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int size = reader.ReadShort();

            foreach (var inventoryValue in data.Inventory.Values)
            {
                if(inventoryValue.IsQuestItem)
                    data.Inventory.Remove(inventoryValue.ObjectId);
            }

            for (int i = 0; i < size; i++)
            {
                int objId = reader.ReadInt();
                StashedItem item = new StashedItem();
                item.IsQuestItem = true;
                item.ObjectId = objId; //writeD(item.getObjectId());
                item.ItemId = reader.ReadInt();//writeD(item.getdisplayId() > 0 ? item.getdisplayId() : item.getItemId());
                reader.ReadInt(); //writeD(item.getEquipSlot());
                item.ItemQuantity = reader.ReadLong();//writeQ(count);
                reader.ReadShort(); //writeH(item.getTemplate().getType2ForPackets());
                reader.ReadShort(); //writeH(item.getCustomType1());
                item.IsEquipped = reader.ReadShort() == 1; //writeH(item.isEquipped() ? 1 : 0);
                reader.ReadInt(); //writeD(item.getBodyPart());
                reader.ReadShort(); //writeH(item.getEnchantLevel());
                reader.ReadShort(); //writeH(item.getCustomType2());
                reader.ReadInt(); //writeD(item.getAugmentationId());
                reader.ReadInt(); //writeD(item.getShadowLifeTime());
                reader.ReadInt(); //writeD(item.getTemporalLifeTime());
                reader.ReadShort(); //writeH(item.getAttackElement().getId());
                reader.ReadShort();//writeH(item.getAttackElementValue());
                reader.ReadShort();//writeH(item.getDefenceFire());
                reader.ReadShort();//writeH(item.getDefenceWater());
                reader.ReadShort();//writeH(item.getDefenceWind());
                reader.ReadShort();//writeH(item.getDefenceEarth());
                reader.ReadShort();//writeH(item.getDefenceHoly());
                reader.ReadShort();//writeH(item.getDefenceUnholy());
                reader.ReadShort();//writeH(item.getEnchantOptions()[0]);
                reader.ReadShort();//writeH(item.getEnchantOptions()[1]);
                reader.ReadShort();//writeH(item.getEnchantOptions()[2]);
                
                data.Inventory.Add(item.ObjectId, item);
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
