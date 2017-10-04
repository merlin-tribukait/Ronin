using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Protocols.HighFive.Incoming;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive
{
    public class H5PacketFactory : PacketFactory
    {
        public override void Init()
        {
            var type = typeof(H5IncomingPacket);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            foreach (var type1 in types)
            {
                var packet =
                    ((H5IncomingPacket) Activator.CreateInstance(type1, new PacketReader(new byte[100], true), true));
                if(packet.Id == H5PacketIds.ServerPrimary.Extended)
                    incomingPacketDefinitionsEx.Add((int)packet.SubId,type1);
                else
                {
                    incomingPacketDefinitions.Add((int)packet.Id, type1);
                }
            }

            type = typeof(H5OutgoingPacket);
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            foreach (var type1 in types)
            {
                var packet =
                    ((H5OutgoingPacket)Activator.CreateInstance(type1, new PacketReader(new byte[100], false), false));
                if (packet.Id == H5PacketIds.ClientPrimary.Extended)
                    outgoingPacketDefinitionsEx.Add((int)packet.SubId, type1);
                else
                {
                    outgoingPacketDefinitions.Add((int)packet.Id, type1);
                }
            }
        }

        //public Packet CreatePacket(byte[] dataBytes, bool fromServer)
        //{
        //    PacketReader packetReader = new PacketReader(dataBytes, fromServer);


        //    if (fromServer)
        //    {
        //        H5PacketIds.ServerPrimary opcode = (H5PacketIds.ServerPrimary)packetReader.Id;
        //        switch (opcode)
        //        {
        //            case H5PacketIds.ServerPrimary.MoveToLocation:
        //                return new MoveTo(packetReader,fromServer);
        //            case H5PacketIds.ServerPrimary.CharInfo:
        //                return new CharInfo(packetReader,fromServer);
        //            case H5PacketIds.ServerPrimary.UserInfo:
        //                return new UserInfo(packetReader,fromServer);
        //            case H5PacketIds.ServerPrimary.StatusUpdate:
        //                return new StatusUpdate(packetReader,fromServer);
        //            case H5PacketIds.ServerPrimary.NpcInfo:
        //                return new NpcInfo(packetReader,fromServer);
        //        }
        //    }
        //    else
        //    {
                
        //    }
            
        //    packet = null;
        //    return packet;
        //}
    }
}
