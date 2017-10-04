using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class TeleportToLocation : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.TeleportToLocation;

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

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
