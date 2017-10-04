using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Outgoing
{
    public class ConfirmDialog : ILOutgoingPacket
    {
        private ILPacketIds.ClientPrimary _id = ILPacketIds.ClientPrimary.ConfirmDialog;

        public ConfirmDialog(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int messageId = reader.ReadInt();
            if (messageId == 1510)
            {
                var curBot = MainWindow.ViewModel.Bots.FirstOrDefault(bot => Object.ReferenceEquals(data, bot.PlayerData));
                if (curBot == null)
                    return;

                var cutBotEngine = curBot.Engine;
                if (cutBotEngine.Running && cutBotEngine.DialogHandler.AcceptResurrection)
                    DropPacket = true;
            }

            //LogHelper.GetLogger().Debug("CLIENT"+messageId);
            //int _answer = reader.ReadInt();
            //LogHelper.GetLogger().Debug("CLIENT" + _answer);
            //int _requesterId = reader.ReadInt();
            //LogHelper.GetLogger().Debug("CLIENT" + _requesterId);
        }

        public override ILPacketIds.ClientPrimary Id
        {
            get { return _id; }
        }
    }
}
