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
using Ronin.Protocols.Abstract.Interfaces;
using Ronin.Protocols.HighFive.Requests;

namespace Ronin.Protocols.HighFive
{
    public class H5ActionsController : ActionsController, IBuffRemover
    {
        public H5ActionsController(L2PlayerData data, Client tcpClient) : base(data, tcpClient)
        {
        }

        public override void MoveToRaw(int x, int y, int z)
        {
            var action = new MoveTo(x,y,z);
            action.Send(data,tcpClient);
        }

        public override void OpenInventory()
        {
            var action = new OpenInventory();
            action.Send(data, tcpClient);
        }

        public override void TargetByObjectIdRaw(int objId)
        {
            var action = new Target(objId);
            action.Send(data, tcpClient);
        }

        public override bool TargetByObjectId(int objId)
        {
            if (this.data.MainHero.TargetObjectId != objId)
                TargetByObjectIdRaw(objId);

            int counter = 0;
            while (this.data.MainHero.TargetObjectId != objId && counter < 50)
            {
                Thread.Sleep(100);
                counter++;
            }

            return counter < 50;
        }

        public override void TargetClosestByUnitId(int unitId)
        {
            int min = int.MaxValue;
            Npc npc = null;

            foreach (var npcAround in data.SurroundingNpcs)
            {
                if (npcAround.UnitId == unitId && npcAround.RangeTo(data.MainHero) < min)
                {
                    npc = npcAround;
                    min = (int) npcAround.RangeTo(data.MainHero);
                }
            }
            
            TargetByObjectId(npc.ObjectId);
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
            var action = new RequestWithDrawalParty();
            action.Send(data,tcpClient);
        }

        public override void UseItem(int objectId, bool ctrlPress)
        {
            var action = new UseItem(objectId, ctrlPress);
            action.Send(data,tcpClient);
        }

        public override bool PartyInvite(string name, PartyType partyType)
        {
            if(data.PartyMembers.Count >0 && data.PartyLeaderObjectId != data.MainHero.ObjectId)
                return false;

            var action = new PartyInvite(name, partyType);
            action.Send(data, tcpClient);

            int timeout = 0;
            data.PendingInvite = null;
            while (data.PendingInvite==null && timeout < 10)
            {
                timeout++;
                Thread.Sleep(100);
            }

            return data.PendingInvite ?? false;
        }

        public override void PartyAcceptResponse(bool accept)
        {
            if(Math.Abs(Environment.TickCount - data.LastPartyInviteStamp) > 10000 || data.LastPartyInviter == null)
                return;

            var action = new PartyAcceptResponse(accept);
            action.Send(data, tcpClient);
            data.LastPartyInviteStamp = 0;
        }

        public override void AnswerRessurectDialog(bool answer)
        {
            var action = new ConfirmDialog(1510, answer);
            action.Send(data, tcpClient);
            data.PendingRessurectDialog = false;
        }

        public void RemoveBuff(int objectId, int buffId, int buffLevel)
        {
            var action = new RequestDispel(objectId,buffId, buffLevel);
            action.Send(data, tcpClient);
        }
    }
}
