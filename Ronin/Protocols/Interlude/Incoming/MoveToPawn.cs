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
    public class MoveToPawn : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.MoveToPawn;

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
            double dist = data.MainHero.RangeTo(new Locatable(followerX, followerY, followerZ));


            if (data.AllUnits.Any(unit => unit.ObjectId == followerObjId))
            {
                var instance = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == followerObjId);

                if(instance==null)
                    return;

                instance.X = followerX;
                instance.Y = followerY;
                instance.Z = followerZ;

                instance.IsMoving = false;
                instance.MoveToStartStamp = Environment.TickCount;

                if (instance is Npc)
                {
                    ((Npc)instance).AddStamp = Environment.TickCount;
                }

                if (data.AllUnits.Any(unit => unit.ObjectId == unitToFollowObjId))
                {
                    var instanceToFollow = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == unitToFollowObjId);
                    if(instanceToFollow == null)
                        return;

                    instance.IsFollowing = true;
                    instance.UnitToFollow = instanceToFollow;

                    if (instanceToFollow is Player && instance is Npc)
                        instance.TargetObjectId = instanceToFollow.ObjectId;
                }

                if (data.MainHero.ObjectId == unitToFollowObjId)
                {
                    instance.IsFollowing = true;
                    instance.UnitToFollow = data.MainHero;

                    if (instance is Npc)
                        instance.TargetObjectId = unitToFollowObjId;
                }


            }

            if (data.MainHero.ObjectId == followerObjId)
            {
                data.MainHero.X = followerX;
                data.MainHero.Y = followerY;
                data.MainHero.Z = followerZ;

                data.MainHero.IsMoving = false;
                data.MainHero.MoveToStartStamp = Environment.TickCount;

                if (data.AllUnits.Any(unit => unit.ObjectId == unitToFollowObjId))
                {
                    data.MainHero.IsFollowing = true;
                    data.MainHero.UnitToFollow = data.AllUnits.First(unit => unit.ObjectId == unitToFollowObjId);
                }
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
