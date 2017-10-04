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
    public class TargetUnselected : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.TargetUnselected;

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

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
