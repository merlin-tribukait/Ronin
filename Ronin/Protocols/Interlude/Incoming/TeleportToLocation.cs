using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class TeleportToLocation : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.TeleportToLocation;

        public TeleportToLocation(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            int x = reader.ReadInt();
            int y = reader.ReadInt();
            int z = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == objId))
            {
                var unita = data.AllUnits.First(unit => unit.ObjectId == objId);
                unita.X = x;
                unita.Y = y;
                unita.Z = z;
            }
            else if (data.MainHero.ObjectId == objId)
            {
                data.MainHero.X = x;
                data.MainHero.Y = y;
                data.MainHero.Z = z;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
