using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class Packet
    {
        protected PacketReader reader;
        public bool DropPacket = false;

        public Packet(PacketReader reader, bool fromServer)
        {
            this.reader = reader;
        }

        public abstract void Parse(L2PlayerData data);
    }
}
