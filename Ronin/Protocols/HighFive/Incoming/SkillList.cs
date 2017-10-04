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
    public class SkillList : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.SkillList;

        public SkillList(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int skillCount = reader.ReadInt();
            data.Skills.Clear();
            for (int i = 0; i < skillCount; i++)
            {
                var skill = new Skill();
                skill.IsPassive = reader.ReadInt() == 1;
                skill.SkillLevel = reader.ReadInt();
                skill.SkillId = reader.ReadInt();
                skill.IsDisabled = reader.ReadBool();
                skill.IsEnchanted = reader.ReadBool();
                data.Skills.Add(skill.SkillId,skill);
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
