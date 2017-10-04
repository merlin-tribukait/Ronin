using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class TargetUnselected : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.TargetUnselected;

        public TargetUnselected(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            if (objId == data.MainHero.ObjectId)
                data.MainHero.TargetObjectId = 0;
            else if (data.Players.ContainsKey(objId))
                data.Players[objId].TargetObjectId = 0;
            else if (data.Players.Any(player => player.Value.PlayerSummons.Count > 0 && player.Value.PlayerSummons.First().ObjectId == objId))
                data.Players.First(player => player.Value.PlayerSummons.First().ObjectId == objId)
                    .Value.PlayerSummons.First()
                    .TargetObjectId = 0;
            else if (data.Npcs.ContainsKey(objId))
                data.Npcs[objId].TargetObjectId = 0;
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
