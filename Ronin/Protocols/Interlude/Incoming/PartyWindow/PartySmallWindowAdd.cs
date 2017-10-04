using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming.PartyWindow
{
    public class PartySmallWindowAdd : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.PartySmallWindowAdd;

        public PartySmallWindowAdd(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadInt(); //main hero player id
            reader.ReadInt(); //writeD(_party.getDistributionType().getId());// writeD(0x04); ?? //c3
            int objId = reader.ReadInt(); //writeD(_member.getObjectId());
            Player ptMember = data.Players.ContainsKey(objId) ? data.Players[objId] : new Player() { ObjectId = objId }; //writeS(_member.getName());
            if (!data.Players.ContainsKey(objId) && data.MainHero.ObjectId != objId)
                data.Players.Add(objId, ptMember);

            ptMember.ObjectId = objId;
            ptMember.IsMyPartyMember = true;
            ptMember.Name = reader.ReadString();
            ptMember.CombatPoints = reader.ReadInt(); //writeD((int)member.getCurrentCp()); // c4
            ptMember.MaxCombatPoints = reader.ReadInt();//writeD(member.getMaxCp()); // c4

            ptMember.Health = reader.ReadInt();//writeD((int)member.getCurrentHp());
            ptMember.MaxHealth = reader.ReadInt();//writeD(member.getMaxHp());
            ptMember.Mana = reader.ReadInt();//writeD((int)member.getCurrentMp());
            ptMember.MaxMana = reader.ReadInt();//writeD(member.getMaxMp());
            ptMember.Level = reader.ReadInt();//writeD(member.getLevel());
            ptMember.Class = (L2Class)reader.ReadInt();//writeD(member.getClassId().getId());
            reader.ReadInt(); //writeD(0x00); // ?
            reader.ReadInt(); //writeD(0x00); // ?
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
