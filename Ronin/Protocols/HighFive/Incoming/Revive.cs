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
    public class Revive : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.Revive;

        public Revive(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objectId = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == objectId))
                data.AllUnits.First(unit => unit.ObjectId == objectId).IsDead = false;
            else if (objectId == data.MainHero.ObjectId)
            {
                data.MainHero.IsDead = false;
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
