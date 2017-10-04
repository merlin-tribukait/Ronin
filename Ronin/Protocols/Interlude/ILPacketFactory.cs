using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude
{
    public class ILPacketFactory : PacketFactory
    {
        public override void Init()
        {
            var type = typeof(ILIncomingPacket);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            foreach (var type1 in types)
            {
                var packet =
                    ((ILIncomingPacket)Activator.CreateInstance(type1, new PacketReader(new byte[100], true), true));
                //if (packet.Id == ILPacketIds.ServerPrimary.Extended)
                //    incomingPacketDefinitionsEx.Add(packet.SubId, type1);
                //else
                //{
                    incomingPacketDefinitions.Add((int)packet.Id, type1);
                //}
            }

            type = typeof(ILOutgoingPacket);
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            foreach (var type1 in types)
            {
                var packet =
                    ((ILOutgoingPacket)Activator.CreateInstance(type1, new PacketReader(new byte[100], false), false));
                //if (packet.Id == H5PacketIds.ClientPrimary.Extended)
                //    outgoingPacketDefinitionsEx.Add(packet.SubId, type1);
                //else
                //{
                    outgoingPacketDefinitions.Add((int)packet.Id, type1);
                //}
            }
        }

    }
}
