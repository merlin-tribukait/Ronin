using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class StopMove : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.StopMove;

        public StopMove(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objectId = reader.ReadInt();
            int x = reader.ReadInt();
            int y = reader.ReadInt();
            int z = reader.ReadInt();
            if (data.AllUnits.Any(unit => unit.ObjectId == objectId))
            {
                data.AllUnits.First(unit => unit.ObjectId == objectId).X = x;
                data.AllUnits.First(unit => unit.ObjectId == objectId).Y = y;
                data.AllUnits.First(unit => unit.ObjectId == objectId).Z = z;

                data.AllUnits.First(unit => unit.ObjectId == objectId).IsFollowing = false;
                data.AllUnits.First(unit => unit.ObjectId == objectId).IsMoving = false;
            }
            else if (data.MainHero.ObjectId == objectId)
            {
                data.MainHero.X = x;
                data.MainHero.Y = y;
                data.MainHero.Z = z;

                data.MainHero.IsFollowing = false;
                data.MainHero.IsMoving = false;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
