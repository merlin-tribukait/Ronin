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
    public class PartySmallWindowAll : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.PartySmallWindowAll;

        public PartySmallWindowAll(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            data.PartyLeaderObjectId = reader.ReadInt();
            data.PartyType = (PartyType)reader.ReadInt();
            int ptMembersCount = reader.ReadInt();

            for (int i = 0; i < ptMembersCount; i++)
            {
                int objId = reader.ReadInt();
                Player ptMember = data.Players.ContainsKey(objId) ? data.Players[objId] : new Player();
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
                reader.ReadInt(); //writeD(0x00);// writeD(0x01); ??
                ptMember.Race = (L2Race)reader.ReadInt(); //writeD(member.getRace().ordinal());
                //reader.ReadInt(); //writeD(0x00); // T2.3
                //reader.ReadInt(); //writeD(0x00); // T2.3

                //ptMember.PlayerSummons.Clear();
                //int summObjId = reader.ReadInt();
                //if (summObjId != 0)
                //{
                //    Npc summ = data.Npcs.ContainsKey(summObjId) ? data.Npcs[summObjId] : new Npc();
                //    summ.ObjectId = summObjId;
                //    summ.UnitId = reader.ReadInt() - 1000000;
                //    summ.IsSummonPet = true;
                //    summ.Name = reader.ReadString();
                //    summ.Health = reader.ReadInt();
                //    summ.MaxHealth = reader.ReadInt();
                //    summ.Mana = reader.ReadInt();
                //    summ.MaxMana = reader.ReadInt();
                //    summ.Level = reader.ReadInt();
                //    ptMember.PlayerSummons.Add(summ);
                //}

                if (!data.Players.ContainsKey(objId) && data.MainHero.ObjectId != objId)
                    data.Players.Add(objId, ptMember);
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
