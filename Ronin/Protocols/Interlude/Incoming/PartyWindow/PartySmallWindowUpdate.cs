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
    public class PartySmallWindowUpdate : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.PartySmallWindowUpdate;

        public PartySmallWindowUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            Player ptMember = data.Players.ContainsKey(objId) ? data.Players[objId] : new Player() { ObjectId = objId };
            if (!data.Players.ContainsKey(objId))
                data.Players.Add(objId, ptMember);

            ptMember.IsMyPartyMember = true;
            ptMember.ObjectId = objId;
            ptMember.Name = reader.ReadString();
            ptMember.CombatPoints = reader.ReadInt(); //writeD((int)member.getCurrentCp()); // c4
            ptMember.MaxCombatPoints = reader.ReadInt();//writeD(member.getMaxCp()); // c4

            ptMember.Health = reader.ReadInt();//writeD((int)member.getCurrentHp());
            ptMember.MaxHealth = reader.ReadInt();//writeD(member.getMaxHp());
            ptMember.Mana = reader.ReadInt();//writeD((int)member.getCurrentMp());
            ptMember.MaxMana = reader.ReadInt();//writeD(member.getMaxMp());
            ptMember.Level = reader.ReadInt();//writeD(member.getLevel());
            ptMember.Class = (L2Class)reader.ReadInt();//writeD(member.getClassId().getId());
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
