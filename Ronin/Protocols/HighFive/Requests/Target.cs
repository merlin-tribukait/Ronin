using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;

namespace Ronin.Protocols.HighFive.Requests
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
            packet.SetId(H5PacketIds.ClientPrimary.Action);
            packet.Append(objectId);
            packet.Append(data.MainHero.X);
            packet.Append(data.MainHero.Y);
            packet.Append(data.MainHero.Z);
            packet.Append(0);
        }
    }
}
