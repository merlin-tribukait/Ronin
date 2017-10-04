using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Protocols.HighFive;
using Ronin.Protocols.Interlude;

namespace Ronin.Protocols
{
    public static class ProtocolFactory
    {
        private static PacketFactory placeholder = null;

        public static PacketFactory CreatePacketFactory(int version)
        {
            switch (version)
            {
                case 273:
                    return new H5PacketFactory();

                case 268:
                    return new H5PacketFactory();//might not work properly

                case 746:
                    return new ILPacketFactory();
            }

            throw new Exception("Unsupported client version.");
        }
    }
}
