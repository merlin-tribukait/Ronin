using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Outgoing
{
    public class ValidatePosition : ILOutgoingPacket
    {
        private ILPacketIds.ClientPrimary _id = ILPacketIds.ClientPrimary.ValidatePosition;

        public ValidatePosition(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int curX = reader.ReadInt();
            int curY = reader.ReadInt();
            int curZ = reader.ReadInt();
            
            data.MainHero.ValidatedX = curX;
            data.MainHero.ValidatedY = curY;
            data.MainHero.ValidatedZ = curZ;

            //data.MainHero.X = curX;
            //data.MainHero.Y = curY;
            //data.MainHero.Z = curZ;
        }

        public override ILPacketIds.ClientPrimary Id
        {
            get { return _id; }
        }
    }
}
