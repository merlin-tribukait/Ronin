using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Requests
{
    public class PartyInvite : ActionRequest
    {
        private string _name;
        private PartyType _partyType;

        public PartyInvite(string name, PartyType partyType)
        {
            _name = name;
            _partyType = partyType;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId(H5PacketIds.ClientPrimary.RequestPartyInvite);
            packet.Append(_name);
            packet.Append((int)_partyType);
        }
    }
}
