using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;

namespace Ronin.Protocols.HighFive.Requests
{
    public class RequestDispel : ActionRequest
    {
        private int _objectId, _buffId, _level;

        public RequestDispel(int objectId, int buffId, int level)
        {
            this._objectId = objectId;
            this._buffId = buffId;
            this._level = level;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId(H5PacketIds.ClientPrimary.Extended);
            packet.SetSubId(H5PacketIds.ClientSecondary.RequestDispel);
            packet.Append(_objectId);
            packet.Append(_buffId);
            packet.Append(_level);
        }
    }
}
