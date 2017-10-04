using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class OutgoingPacket : Packet
    {
        public OutgoingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }
    }
}
