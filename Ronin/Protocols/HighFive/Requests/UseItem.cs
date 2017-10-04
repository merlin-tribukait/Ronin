using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;

namespace Ronin.Protocols.HighFive.Requests
{
    public class UseItem : ActionRequest
    {
        private int _objectId;
        private bool _ctrlPress;

        public UseItem(int objectId, bool ctrlPress)
        {
            _objectId = objectId;
            _ctrlPress = ctrlPress;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId(H5PacketIds.ClientPrimary.UseItem);
            packet.Append(_objectId);
            packet.Append(_ctrlPress ? 1 : 0);
        }
    }
}
