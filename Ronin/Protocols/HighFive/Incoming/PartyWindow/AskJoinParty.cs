using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming.PartyWindow
{
    public class AskJoinParty : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id= H5PacketIds.ServerPrimary.AskJoinParty;

        public AskJoinParty(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            data.LastPartyInviteStamp = Environment.TickCount;
            data.LastPartyInviter = reader.ReadString();
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
