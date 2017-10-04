using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class IncomingPacket : Packet
    {
        public IncomingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }
    }
}
