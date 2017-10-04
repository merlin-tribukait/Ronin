using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive
{
    public abstract class H5OutgoingPacket : OutgoingPacket
    {
        public H5OutgoingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public abstract H5PacketIds.ClientPrimary Id { get; }
        public virtual H5PacketIds.ClientSecondary SubId { get { return 0; } }
    }
}
