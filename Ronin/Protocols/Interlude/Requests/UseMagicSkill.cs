using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;

namespace Ronin.Protocols.Interlude.Requests
{
    public class UseMagicSkill : ActionRequest
    {
        private int skillId;
        private bool useCtrl;
        private bool useShift;

        public UseMagicSkill(int skillId, bool useCtrl, bool useShift)
        {
            this.skillId = skillId;
            this.useCtrl = useCtrl;
            this.useShift = useShift;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId((int)ILPacketIds.ClientPrimary.UseMagicSkill);
            packet.Append(skillId);
            packet.Append(useCtrl ? 1 : 0);
            packet.Append((byte)(useShift ? 1 : 0));
        }
    }
}
