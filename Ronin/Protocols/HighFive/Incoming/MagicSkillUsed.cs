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
    public class MagicSkillUsed : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.MagicSkillUse;

        public MagicSkillUsed(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objID = reader.ReadInt();
            int targetId = reader.ReadInt();
            int skillId = reader.ReadInt();
            int skillLevel = reader.ReadInt();
            int castTime = reader.ReadInt();
            int cooldown = reader.ReadInt();
            int casterX = reader.ReadInt();
            int casterY = reader.ReadInt();
            int casterZ = reader.ReadInt();
            short unkCount = reader.ReadShort();
            reader.SkipBytes(unkCount*2);
            short hittedTargetLocationsCount = reader.ReadShort();
            reader.SkipBytes(hittedTargetLocationsCount*12);
            int targetX = reader.ReadInt();
            int targetY = reader.ReadInt();
            int targetZ = reader.ReadInt();
            if (objID == data.MainHero.ObjectId)
            {
                data.LandedSkills.Remove(skillId);
                data.LandedSkills.Add(skillId, targetId);
                if (data.Skills.ContainsKey(skillId))
                {
                    data.Skills[skillId].CastTime = castTime;
                    data.Skills[skillId].Cooldown = cooldown;
                    data.Skills[skillId].LastLaunched = Environment.TickCount;
                    data.LastSkillLaunch = Environment.TickCount;
                    data.LastCastTime = castTime;
                    if (data.LastUsedNukeId == skillId)
                    {
                        //bot has attacked successfully
                        data.AttackFlag = true;
                    }
                    else
                    {
                        //delay the buff skill so bot can wait for buff apply confirmation
                        data.Skills[skillId].LastLaunched += 150;
                    }
                }
            }

            //Add to loot the monsters that were attacked by me or a party member.
            if ((data.PartyMembers.Any(ptmember => ptmember.ObjectId == objID) || data.MainHero.ObjectId == objID) &&
                data.Npcs.ContainsKey(targetId) && data.Npcs[targetId].IsMonster)
            {
                data.MonstersToLoot.Add(targetId);
            }

            if ((data.PartyMembers.Any(ptmember => ptmember.PlayerSummons.Any(pet => pet.ObjectId == objID)) ||
                data.MainHero.PlayerSummons.Any(summ => summ.ObjectId == objID)) &&
                data.Npcs.ContainsKey(targetId) && data.Npcs[targetId].IsMonster)
            {
                data.MonstersToLoot.Add(targetId);
            }

            if (data.AllUnits.Any(unit => unit.ObjectId == objID))
            {
                if (data.AllUnits.First(unit => unit.ObjectId == objID) is Npc)
                    data.AllUnits.First(unit => unit.ObjectId == objID).TargetObjectId = targetId;

                if(data.AllUnits.First(unit => unit.ObjectId == objID).ObjectId != targetId)
                    data.AllUnits.First(unit => unit.ObjectId == objID).LastUnitAttackedObjectId = targetId;

                data.AllUnits.First(unit => unit.ObjectId == objID).IsFollowing = false;
                data.AllUnits.First(unit => unit.ObjectId == objID).IsMoving = false;
            }

            if (data.AllUnits.Any(unit => unit.ObjectId == targetId))
            {
                data.AllUnits.First(unit => unit.ObjectId == targetId).X = targetX;
                data.AllUnits.First(unit => unit.ObjectId == targetId).Y = targetY;
                data.AllUnits.First(unit => unit.ObjectId == targetId).Z = targetZ;
            }

            if (data.MainHero.ObjectId == objID)
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
