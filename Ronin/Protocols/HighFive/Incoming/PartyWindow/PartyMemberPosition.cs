using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming.PartyWindow
{
    public class PartyMemberPosition : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.PartyPositionUpdate;

        public PartyMemberPosition(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int locCount = reader.ReadInt();
            for (int i = 0; i < locCount; i++)
            {
                int objId = reader.ReadInt();
                if(!data.Players.ContainsKey(objId))
                    return;

                Player ptMember = data.Players.ContainsKey(objId) ? data.Players[objId] : new Player();
                ptMember.ObjectId = objId;
                ptMember.IsMyPartyMember = true;
                if(!data.Players.ContainsKey(objId) && data.MainHero.ObjectId != objId)
                    data.Players.Add(objId, ptMember);

                if(data.MainHero.ObjectId == objId)
                    continue;

                data.AllUnits.First(unit => unit.ObjectId == objId).X = reader.ReadInt();
                data.AllUnits.First(unit => unit.ObjectId == objId).Y = reader.ReadInt();
                data.AllUnits.First(unit => unit.ObjectId == objId).Z = reader.ReadInt();
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
