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
    public class MoveToPawn : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.MoveToPawn;

        public MoveToPawn(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int followerObjId = reader.ReadInt();
            int unitToFollowObjId = reader.ReadInt();
            reader.ReadInt(); //Follow distance
            int followerX = reader.ReadInt();
            int followerY = reader.ReadInt();
            int followerZ = reader.ReadInt();
            int unitToFollowX = reader.ReadInt();
            int unitToFollowY = reader.ReadInt();
            int unitToFollowZ = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == followerObjId))
            {
                data.AllUnits.First(unit => unit.ObjectId == followerObjId).X = followerX;
                data.AllUnits.First(unit => unit.ObjectId == followerObjId).Y = followerY;
                data.AllUnits.First(unit => unit.ObjectId == followerObjId).Z = followerZ;

                data.AllUnits.First(unit => unit.ObjectId == followerObjId).IsMoving = false;

                if (data.AllUnits.Any(unit => unit.ObjectId == unitToFollowObjId))
                {
                    data.AllUnits.First(unit => unit.ObjectId == followerObjId).IsFollowing = true;
                    data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId).X = unitToFollowX;
                    data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId).Y = unitToFollowY;
                    data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId).Z = unitToFollowZ;
                    data.AllUnits.First(unit => unit.ObjectId == followerObjId).UnitToFollow = data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId);
                }

                if (data.MainHero.ObjectId == unitToFollowObjId && data.AllUnits.Any(unit => unit.ObjectId == followerObjId))
                {
                    data.AllUnits.First(unit => unit.ObjectId == followerObjId).IsFollowing = true;
                    data.AllUnits.First(unit => unit.ObjectId == followerObjId).UnitToFollow = data.MainHero;
                }
            }

            if (data.MainHero.ObjectId == followerObjId)
            {
                data.MainHero.X = followerX;
                data.MainHero.Y = followerY;
                data.MainHero.Z = followerZ;

                data.MainHero.IsMoving = false;

                if (data.AllUnits.Any(unit => unit.ObjectId == unitToFollowObjId))
                {
                    data.MainHero.MoveToStartStamp = Environment.TickCount;
                    data.MainHero.IsFollowing = true;
                    data.MainHero.UnitToFollow = data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId);
                }
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
