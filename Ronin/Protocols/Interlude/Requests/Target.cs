using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;

namespace Ronin.Protocols.Interlude.Requests
{
    public class Target : ActionRequest
    {
        private int objectId;

        public Target(int objectId)
        {
            this.objectId = objectId;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId((int)ILPacketIds.ClientPrimary.Action);
            packet.Append(objectId);
            packet.Append(data.MainHero.ValidatedX);
            packet.Append(data.MainHero.ValidatedY);
            packet.Append(data.MainHero.ValidatedZ);
            packet.Append((byte)0);
        }
    }
}
