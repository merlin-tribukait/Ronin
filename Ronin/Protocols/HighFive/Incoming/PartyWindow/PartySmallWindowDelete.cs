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
    public class PartySmallWindowDelete : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.PartySmallWindowDelete;

        public PartySmallWindowDelete(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            string name = reader.ReadString();

            foreach (var pair in data.Players)
            {
                if (pair.Value.IsMyPartyMember && pair.Value.ObjectId == objId)
                {
                    pair.Value.IsMyPartyMember = false;
                    if (!data.SurroundingPlayers.Contains(pair.Value))
                        data.Players.Remove(pair.Key);
                }
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
