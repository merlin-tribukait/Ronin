using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class PacketFactory
    {
        protected Dictionary<int, Type> incomingPacketDefinitions = new Dictionary<int, Type>();
        protected Dictionary<int, Type> incomingPacketDefinitionsEx = new Dictionary<int, Type>();
        protected Dictionary<int, Type> outgoingPacketDefinitions = new Dictionary<int, Type>();
        protected Dictionary<int, Type> outgoingPacketDefinitionsEx = new Dictionary<int, Type>();

        public PacketFactory()
        {
            Init();
        }

        public abstract void Init();

        public Packet CreatePacket(byte[] dataBytes, bool fromServer)
        {
            Packet pack = null;
            Type packetType;
            PacketReader packetReader = new PacketReader(dataBytes, fromServer);

            if (fromServer)
            {
                H5PacketIds.ServerPrimary opcode = (H5PacketIds.ServerPrimary)packetReader.Id;
                if (opcode == H5PacketIds.ServerPrimary.Extended)
                {
                    H5PacketIds.ServerSecondary opcodeEx = (H5PacketIds.ServerSecondary)packetReader.SubId;
                    if (incomingPacketDefinitionsEx.ContainsKey((int)opcodeEx))
                    {
                        return (Packet)Activator.CreateInstance(incomingPacketDefinitionsEx[(int)opcodeEx],packetReader,fromServer);
                    }
                }

                if(incomingPacketDefinitions.ContainsKey((int)opcode))
                    return (Packet)Activator.CreateInstance(incomingPacketDefinitions[(int)opcode], packetReader, fromServer);
            }
            else
            {
                H5PacketIds.ClientPrimary opcode = (H5PacketIds.ClientPrimary)packetReader.Id;
                if (opcode == H5PacketIds.ClientPrimary.Extended)
                {
                    H5PacketIds.ClientSecondary opcodeEx = (H5PacketIds.ClientSecondary)packetReader.SubId;
                    if (outgoingPacketDefinitionsEx.ContainsKey((int)opcodeEx))
                    {
                        return (Packet)Activator.CreateInstance(outgoingPacketDefinitionsEx[(int)opcodeEx], packetReader, fromServer);
                    }
                }

                if (outgoingPacketDefinitions.ContainsKey((int)opcode))
                    return (Packet)Activator.CreateInstance(outgoingPacketDefinitions[(int)opcode], packetReader, fromServer);
            }


            return pack;
        }
    }
}
