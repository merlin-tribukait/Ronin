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
    public class SkillList : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.SkillList;

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
                data.Skills.Add(skill.SkillId, skill);
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
