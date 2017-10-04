using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class Revive : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id= ILPacketIds.ServerPrimary.Revive;

        public Revive(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objectId = reader.ReadInt();

            if (data.AllUnits.Any(unit => unit.ObjectId == objectId))
                data.AllUnits.First(unit => unit.ObjectId == objectId).IsDead = false;
            else if (objectId == data.MainHero.ObjectId)
            {
                data.MainHero.IsDead = false;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
