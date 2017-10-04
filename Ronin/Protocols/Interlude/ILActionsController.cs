using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Network;
using Ronin.Protocols.Interlude.Requests;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude
{
    public class ILActionsController : ActionsController
    {
        public ILActionsController(L2PlayerData data, Client tcpClient) : base(data, tcpClient)
        {
            data.MainPlayerLogin += () =>
            {
                PacketBuilder pack = new PacketBuilder();
                pack.SetId((int)ILPacketIds.ClientPrimary.RequestSkillList);
                SendCustomPacketToServer(pack);
            };
        }

        public override void MoveToRaw(int x, int y, int z)
        {
            var action = new MoveTo(x, y, z);
            action.Send(data, tcpClient);
        }

        public override void OpenInventory()
        {
            throw new NotImplementedException();
        }

        public override void TargetByObjectIdRaw(int objId)
        {
            var action = new Target(objId);
            action.Send(data, tcpClient);
        }

        public override void CancelTarget(bool cancelCast)
        {
            var action = new CancelTarget(cancelCast);
            action.Send(data, tcpClient);
        }

        public override void UseSkill(int skillId, bool useCtrl, bool useShift)
        {
            var action = new UseMagicSkill(skillId, useCtrl, useShift);
            action.Send(data, tcpClient);
        }

        public override void LeaveParty()
        {
            throw new NotImplementedException();
        }

        public override void UseItem(int objectId, bool ctrlPress)
        {
            var action = new UseItem(objectId, ctrlPress);
            action.Send(data, tcpClient);
        }

        public override bool PartyInvite(string name, PartyType partyType)
        {
            throw new NotImplementedException();
        }

        public override void PartyAcceptResponse(bool accept)
        {
            throw new NotImplementedException();
        }

        public override void AnswerRessurectDialog(bool answer)
        {
            var action = new ConfirmDialog(1510, answer);
            action.Send(data, tcpClient);
            data.PendingRessurectDialog = false;
        }
    }
}
