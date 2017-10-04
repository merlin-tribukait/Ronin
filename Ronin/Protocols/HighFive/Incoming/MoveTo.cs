using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class MoveTo : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.MoveToLocation;

        public MoveTo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objectId = reader.ReadInt();
            int destX, destY, destZ, curX, curY, curZ;
            destX = reader.ReadInt();
            destY = reader.ReadInt();
            destZ = reader.ReadInt();
            curX = reader.ReadInt();
            curY = reader.ReadInt();
            curZ = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == objectId))
            {
                data.AllUnits.First(unit => unit.ObjectId == objectId).X = curX;
                data.AllUnits.First(unit => unit.ObjectId == objectId).Y = curY;
                data.AllUnits.First(unit => unit.ObjectId == objectId).Z = curZ;

                data.AllUnits.First(unit => unit.ObjectId == objectId).MoveToStartStamp = Environment.TickCount;
                
                data.AllUnits.First(unit => unit.ObjectId == objectId).destX = destX;
                data.AllUnits.First(unit => unit.ObjectId == objectId).destY = destY;
                data.AllUnits.First(unit => unit.ObjectId == objectId).destZ = destZ;

                double dist = data.MainHero.RangeTo(new Locatable(destX, destY, destZ));
                if (data.MainHero.RangeTo(new Locatable(destX, destY, destZ)) < 50 &&
                    data.SurroundingMonsters.Any(mob => mob.ObjectId == objectId) && data.AllUnits.First(unit => unit.ObjectId == objectId) is Npc)
                    data.AllUnits.First(unit => unit.ObjectId == objectId).TargetObjectId = data.MainHero.ObjectId;

                data.AllUnits.First(unit => unit.ObjectId == objectId).IsMoving = true;
                data.AllUnits.First(unit => unit.ObjectId == objectId).IsFollowing = false;
            }

            if (data.MainHero.ObjectId == objectId)
            {
                data.MainHero.X = curX;
                data.MainHero.Y = curY;
                data.MainHero.Z = curZ;

                data.MainHero.MoveToStartStamp = Environment.TickCount;
                data.MainHero.IsMoving = true;
                data.MainHero.IsFollowing = false;

                data.MainHero.destX = destX;
                data.MainHero.destY = destY;
                data.MainHero.destZ = destZ;
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
