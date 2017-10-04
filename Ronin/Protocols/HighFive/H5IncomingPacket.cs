using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive
{
    public abstract class H5IncomingPacket : IncomingPacket
    {
        public H5IncomingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public abstract H5PacketIds.ServerPrimary Id { get; }
        public virtual H5PacketIds.ServerSecondary SubId { get; set; }
    }
}
