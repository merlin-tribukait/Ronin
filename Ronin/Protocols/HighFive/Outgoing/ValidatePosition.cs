using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Outgoing
{
    public class ValidatePosition : H5OutgoingPacket
    {
        private H5PacketIds.ClientPrimary _id = H5PacketIds.ClientPrimary.ValidatePosition;

        public ValidatePosition(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int curX = reader.ReadInt();
            int curY = reader.ReadInt();
            int curZ = reader.ReadInt();

            var dsitance = data.MainHero.RangeTo(new Locatable(curX, curY, curZ));
            
            data.MainHero.ValidatedX = curX;
            data.MainHero.ValidatedY = curY;
            data.MainHero.ValidatedZ = curZ;
        }

        public override H5PacketIds.ClientPrimary Id
        {
            get { return _id; }
        }
    }
}
