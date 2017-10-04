using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class RestartResponse : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.RestartResponse;

        public RestartResponse(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            if (reader.ReadInt() == 1)
            {
                var bot = MainWindow.ViewModel.Bots.FirstOrDefault(bota => Object.ReferenceEquals(bota.PlayerData, data));
                if (bot != null)
                    bot.SelectedConfiguration = null;

                data.MainHero = new MainHero();
                data.Players.Clear();
                data.Npcs.Clear();
                data.GameState = GameState.CharacterSelection;
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
