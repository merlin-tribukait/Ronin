using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Protocols.HighFive;
using Ronin.Utilities;

namespace Ronin.Data.Structures
{
    public class Attack : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.Attack;

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
                if (instance == null)
                    return;

                instance.X = attackerX;
                instance.Y = attackerY;
                instance.Z = attackerZ;
                instance.LastUnitAttackedObjectId = targetObjId;

                if (instance is Npc)
                    instance.TargetObjectId = targetObjId;
            }

            //Add to loot the monsters that were attacked by me or a party member.
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
                data.MainHero.IsFollowing = false;
                data.MainHero.IsMoving = false;
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
