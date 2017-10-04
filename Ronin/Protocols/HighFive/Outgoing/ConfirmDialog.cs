﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Outgoing
{
    public class ConfirmDialog : H5OutgoingPacket
    {
        private H5PacketIds.ClientPrimary _id = H5PacketIds.ClientPrimary.ConfirmDialog;

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

        public override H5PacketIds.ClientPrimary Id
        {
            get { return _id; }
        }
    }
}