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
    public class ConfirmDialog : ActionRequest
    {
        private int _dialogId;
        private bool _answer;

        public ConfirmDialog(int dialogId, bool answer)
        {
            _dialogId = dialogId;
            _answer = answer;
        }

        public override void Build(L2PlayerData data)
        {
            packet.SetId(H5PacketIds.ClientPrimary.ConfirmDialog);
            packet.Append(_dialogId);
            packet.Append(_answer ? 1 : 0);
            //LogHelper.GetLogger().Debug(_answer);
            packet.Append(data.LastDialogToken);
            //LogHelper.GetLogger().Debug(data.LastDialogToken);
        }
    }
}
