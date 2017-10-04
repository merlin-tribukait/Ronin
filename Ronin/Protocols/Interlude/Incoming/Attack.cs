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
    public class Attack : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.Attack;

        public Attack(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int attackerObjId = reader.ReadInt();

            //hackerino
            if (attackerObjId == data.MainHero.ObjectId)
                data.AttackFlag = true;

            int targetObjId = reader.ReadInt();
            reader.ReadInt();//dmg
            reader.ReadByte();//flags

            int attackerX, attackerY, attackerZ;
            attackerX = reader.ReadInt();
            attackerY = reader.ReadInt();
            attackerZ = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == attackerObjId))
            {
                var instance = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == attackerObjId);
                if(instance == null)
                    return;

                instance.X = attackerX;
                instance.Y = attackerY;
                instance.Z = attackerZ;
                instance.LastUnitAttackedObjectId = targetObjId;

                if (instance is Npc)
                {
                    instance.TargetObjectId = targetObjId;
                    ((Npc) instance).AddStamp = Environment.TickCount;
                }
            }

            //Add to loot the monsters that were attacked by a party member.
            if ((data.PartyMembers.Any(ptmember => ptmember.ObjectId == attackerObjId) || data.MainHero.ObjectId == attackerObjId) &&
                data.Npcs.ContainsKey(targetObjId) && data.Npcs[targetObjId].IsMonster)
            {
                data.MonstersToLoot.Add(targetObjId);
            }

            if ((data.PartyMembers.Any(ptmember => ptmember.PlayerSummons.Any(pet => pet.ObjectId == attackerObjId)) ||
                data.MainHero.PlayerSummons.Any(summ => summ.ObjectId == attackerObjId)) &&
                data.Npcs.ContainsKey(targetObjId) && data.Npcs[targetObjId].IsMonster)
            {
                data.MonstersToLoot.Add(targetObjId);
            }

            short otherMonstersHitCount = reader.ReadShort();

            for (int i = 0; i < otherMonstersHitCount; i++)
            {
                targetObjId = reader.ReadInt();
                reader.ReadInt();//dmg
                reader.ReadByte();//flags

                //Add to loot the monsters that were attacked by a party member.
                if ((data.PartyMembers.Any(ptmember => ptmember.ObjectId == attackerObjId) || data.MainHero.ObjectId == attackerObjId) &&
                    data.Npcs.ContainsKey(targetObjId) && data.Npcs[targetObjId].IsMonster)
                {
                    data.MonstersToLoot.Add(targetObjId);
                }

                if ((data.PartyMembers.Any(ptmember => ptmember.PlayerSummons.Any(pet => pet.ObjectId == attackerObjId)) ||
                    data.MainHero.PlayerSummons.Any(summ => summ.ObjectId == attackerObjId)) &&
                    data.Npcs.ContainsKey(targetObjId) && data.Npcs[targetObjId].IsMonster)
                {
                    data.MonstersToLoot.Add(targetObjId);
                }
            }

            if (data.AllUnits.Any(unit => unit.ObjectId == attackerObjId))
            {
                data.AllUnits.First(unit => unit.ObjectId == attackerObjId).IsFollowing = false;
                data.AllUnits.First(unit => unit.ObjectId == attackerObjId).IsMoving = false;
                //data.AllUnits.First(unit => unit.ObjectId == attackerObjId).TargetObjectId = targetObjId;
            }

            if (data.MainHero.ObjectId == attackerObjId)
            {
                data.MainHero.X = attackerX;
                data.MainHero.Y = attackerY;
                data.MainHero.Z = attackerZ;

                data.MainHero.IsFollowing = false;
                data.MainHero.IsMoving = false;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
