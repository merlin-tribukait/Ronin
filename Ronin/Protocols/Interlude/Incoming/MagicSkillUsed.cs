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
    public class MagicSkillUsed : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.MagicSkillUse;

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
            //int targetX = reader.ReadInt();
            //int targetY = reader.ReadInt();
            //int targetZ = reader.ReadInt();
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

            //it still leaves a bug with skills for shots summon
            if (!ExportedData.SkillsIdToName.ContainsKey(skillId) 
                || ExportedData.SkillsIdToName[skillId].ToLower().Contains("soulshot") 
                || ExportedData.SkillsIdToName[skillId].ToLower().Contains("spiritshot")
                || ExportedData.SkillsIdToName[skillId].Equals("Accuracy", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].ToLower().Contains("stance")
                || ExportedData.SkillsIdToName[skillId].Equals("Arcane Power", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Fist Fury", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("True Berserker", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Hard March", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Polearm Accuracy", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("War Frenzy", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Transfer Pain", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Arcane Wisdom", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Guard Stance", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Shield Fortress", StringComparison.OrdinalIgnoreCase)
                || ExportedData.SkillsIdToName[skillId].Equals("Fortitude", StringComparison.OrdinalIgnoreCase)
                )
                return;

            if (data.AllUnits.Any(unit => unit.ObjectId == objID))
            {
                var instance = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == objID);

                if(instance == null)
                    return;

                if (instance is Npc)
                    instance.TargetObjectId = targetId;

                if (instance.ObjectId != targetId)
                    instance.LastUnitAttackedObjectId = targetId;

                instance.IsFollowing = false;
                instance.IsMoving = false;

                instance.X = casterX;
                instance.Y = casterY;
                instance.Z = casterZ;
            }

            if (data.MainHero.ObjectId == objID)
            {
                data.MainHero.IsFollowing = false;
                data.MainHero.IsMoving = false;

                data.MainHero.X = casterX;
                data.MainHero.Y = casterY;
                data.MainHero.Z = casterZ;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
