using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude
{
    public abstract class ILOutgoingPacket : OutgoingPacket
    {
        public ILOutgoingPacket(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }


        public abstract ILPacketIds.ClientPrimary Id { get; }
        public virtual ILPacketIds.ClientSecondary SubId { get { return 0; } }
    }
}
