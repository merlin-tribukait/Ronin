using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;

namespace Ronin.Protocols.HighFive.Requests
{
    public class OpenInventory : ActionRequest
    {
        public override void Build(L2PlayerData data)
        {
            packet.SetId(H5PacketIds.ClientPrimary.RequestItemsList);
        }
    }
}
