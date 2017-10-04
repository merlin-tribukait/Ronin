﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;

namespace Ronin.Protocols.Interlude.Requests
{
    public class CancelTarget : ActionRequest
    {
        private bool _cancelCast;

        public CancelTarget(bool cancelCast)
        {
            _cancelCast = cancelCast;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId((int)ILPacketIds.ClientPrimary.CancelTarget);
            packet.Append(_cancelCast ? (short)0 : (short)1);
        }
    }
}
