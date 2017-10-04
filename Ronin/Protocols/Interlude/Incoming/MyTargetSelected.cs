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
    public class MyTargetSelected : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.MyTargetSelect;

        public MyTargetSelected(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            data.MainHero.TargetObjectId = reader.ReadInt();
            //if (data.Npcs.ContainsKey(data.MainHero.TargetObjectId))
            //{
            //    LogHelper.GetLogger().Debug(data.AllUnits.First(mob => mob.ObjectId == data.MainHero.TargetObjectId).UnitId);
            //    LogHelper.GetLogger().Debug(((Npc)data.AllUnits.First(mob => mob.ObjectId == data.MainHero.TargetObjectId)).IsInvisible);
            //    LogHelper.GetLogger().Debug(data.AllUnits.First(mob => mob.ObjectId == data.MainHero.TargetObjectId).RangeTo(data.MainHero));
            //    LogHelper.GetLogger().Debug(Environment.TickCount - ((Npc)data.AllUnits.First(mob => mob.ObjectId == data.MainHero.TargetObjectId)).AddStamp);
            //    LogHelper.GetLogger().Debug(data.SurroundingMonsters.Any(mob => mob.ObjectId == data.MainHero.TargetObjectId));
            //}
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
