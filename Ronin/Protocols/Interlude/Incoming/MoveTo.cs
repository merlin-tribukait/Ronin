using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class MoveTo : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.MoveTo;

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
                var instance = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == objectId);
                if(instance == null)
                    return;

                instance.X = curX;
                instance.Y = curY;
                instance.Z = curZ;

                instance.MoveToStartStamp = Environment.TickCount;

                instance.destX = destX;
                instance.destY = destY;
                instance.destZ = destZ;

                //double dist = data.MainHero.RangeTo(new Locatable(destX, destY, destZ));
                if (data.MainHero.RangeTo(new Locatable(destX, destY, destZ)) < 50 &&
                    data.SurroundingMonsters.Any(mob => mob.ObjectId == objectId)
                    && instance is Npc)
                    instance.TargetObjectId = data.MainHero.ObjectId;

                instance.IsMoving = true;
                instance.IsFollowing = false;

                if (instance is Npc)
                {
                    ((Npc)instance).AddStamp = Environment.TickCount;
                }
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

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
