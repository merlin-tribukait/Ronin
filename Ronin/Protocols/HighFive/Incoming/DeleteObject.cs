using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class DeleteObject : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.DeleteObject;

        public DeleteObject(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objectId = reader.ReadInt();
            data.Npcs.Remove(objectId);
            data.DroppedItems.Remove(objectId);
            if (data.Players.ContainsKey(objectId) && data.Players[objectId].IsMyPartyMember == false) //Keep party members in the collection, even when they are not around.
                data.Players.Remove(objectId);

            if (data.Players.Any(player => player.Value.PlayerSummons.Any(playerSumm => playerSumm.ObjectId == objectId)))
            {
                Player playera =
                    data.Players.First(
                        player => player.Value.PlayerSummons.Any(playerSumm => playerSumm.ObjectId == objectId)).Value;
                if (!playera.IsMyPartyMember)
                    playera.PlayerSummons.Remove(playera.PlayerSummons.First(summ => summ.ObjectId == objectId));
            }

            if (data.MainHero.PlayerSummons.Any(summ => summ.ObjectId == objectId))
                data.MainHero.PlayerSummons.Remove(data.MainHero.PlayerSummons.First(summ => summ.ObjectId == objectId));
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
