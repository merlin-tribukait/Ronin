﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Requests
{
    public class MoveTo : ActionRequest
    {
        private int x, y, z;

        public MoveTo(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override void Build(L2PlayerData data)
        {
            PacketBuilder pack = packet;
            pack.SetId((int)ILPacketIds.ClientPrimary.RequestMoveTo);
            pack.Append(x);
            pack.Append(y);
            pack.Append(z);
            pack.Append(data.MainHero.ValidatedX);
            pack.Append(data.MainHero.ValidatedY);
            pack.Append(data.MainHero.ValidatedZ);
            pack.Append(1);
        }
    }
}