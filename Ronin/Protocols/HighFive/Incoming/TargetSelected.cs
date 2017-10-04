﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class TargetSelected : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.TargetSelected;

        public TargetSelected(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int targeter = reader.ReadInt();
            int target = reader.ReadInt();
            if (targeter == data.MainHero.ObjectId)
            {
                data.MainHero.TargetObjectId = target;
                data.MainHero.TargetStamp = Environment.TickCount;
            }
            else if (data.Players.ContainsKey(targeter))
            {
                data.Players[targeter].TargetObjectId = target;
                data.Players[targeter].TargetStamp = Environment.TickCount;
            }
            else if (
                data.Players.Any(
                    player =>
                        player.Value.PlayerSummons.Count > 0 &&
                        player.Value.PlayerSummons.Any(summ => summ.ObjectId == targeter)))
            {
                data.Players.First(player => player.Value.PlayerSummons.First().ObjectId == targeter)
                    .Value.PlayerSummons.First(summ => summ.ObjectId == targeter)
                    .TargetObjectId = target;
                data.Players.First(player => player.Value.PlayerSummons.First().ObjectId == targeter)
                    .Value.PlayerSummons.First(summ => summ.ObjectId == targeter)
                    .TargetStamp = Environment.TickCount;
            }
            else if (data.Npcs.ContainsKey(targeter))
            {
                data.Npcs[targeter].TargetObjectId = target;
                data.Npcs[targeter].TargetStamp = Environment.TickCount;
            }

        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
