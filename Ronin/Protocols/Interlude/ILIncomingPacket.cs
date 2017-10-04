using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude
{
    public abstract class ILIncomingPacket : IncomingPacket
    {
        public ILIncomingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public abstract ILPacketIds.ServerPrimary Id { get; }
        public virtual ILPacketIds.ServerSecondary SubId { get; set; }
    }
}
