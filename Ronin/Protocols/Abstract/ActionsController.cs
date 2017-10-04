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
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class ActionsController
    {
        protected L2PlayerData data;
        protected Client tcpClient;

        protected ActionsController(L2PlayerData data, Client tcpClient)
        {
            this.data = data;
            this.tcpClient = tcpClient;
        }

        public abstract void MoveToRaw(int x, int y, int z);

        public virtual void StopMove()
        {
            data.MainHero.MoveToStartStamp -= 150;
            var loc = data.MainHero.Loc;
            MoveToRaw(loc.X, loc.Y, loc.Z);
        }

        public abstract void OpenInventory();

        public abstract void TargetByObjectIdRaw(int objId);

        public virtual bool TargetByObjectId(int objId)
        {
            if (this.data.MainHero.TargetObjectId != objId)
                TargetByObjectIdRaw(objId);

            int counter = 0;
            //LogHelper.GetLogger().Debug("T " + objId);
            while (this.data.MainHero.TargetObjectId != objId && counter < 20)
            {
                //if(counter%5 == 0)
                //    TargetByObjectIdRaw(objId);

                Thread.Sleep(100);
                counter++;
            }

            //if (counter == 30)
            //{
            //    LogHelper.GetLogger().Debug("Badboy " + objId);
            //}
            //else
            //{
            //    LogHelper.GetLogger().Debug("Goodboy " + objId);
            //}
            //var instance = data.AllUnits.First(unit => unit.ObjectId == objId);
            //LogHelper.GetLogger().Debug($"{instance.UnitId}");
            //LogHelper.GetLogger().Debug($"{instance.X}");
            //LogHelper.GetLogger().Debug($"{instance.Y}");
            //LogHelper.GetLogger().Debug($"{instance.Z}");
            //LogHelper.GetLogger().Debug($"MoveToStartStamp {instance.MoveToStartStamp}");
            //LogHelper.GetLogger().Debug($"LastUnitAttackedObjectId {instance.LastUnitAttackedObjectId}");
            //LogHelper.GetLogger().Debug($"IsDead {instance.IsDead}");
            //LogHelper.GetLogger().Debug($"TargetObjectId {instance.TargetObjectId}");
            //LogHelper.GetLogger().Debug($"Title {instance.Title}");

            if (counter == 20)
            {
                LogHelper.GetLogger().Debug("Unsuccessful target.");
            }

            return counter < 20;
        }

        public virtual void TargetClosestByUnitId(int unitId)
        {
            int min = int.MaxValue;
            Npc npc = null;

            foreach (var npcAround in data.SurroundingNpcs)
            {
                if (npcAround.UnitId == unitId && npcAround.RangeTo(data.MainHero) < min)
                {
                    npc = npcAround;
                    min = (int)npcAround.RangeTo(data.MainHero);
                }
            }

            TargetByObjectId(npc.ObjectId);
        }

        public abstract void CancelTarget(bool cancelCast);

        public virtual void Interact()
        {
            if (data.MainHero.TargetObjectId == 0)
                return;

            TargetByObjectIdRaw(data.MainHero.TargetObjectId);
        }

        public virtual void Attack()
        {
            Interact();
        }

        public virtual void Pickup(int objectId)
        {
            TargetByObjectIdRaw(objectId);
        }

        public abstract void UseSkill(int skillId, bool useCtrl, bool useShift);

        public abstract void LeaveParty();

        public abstract void UseItem(int objectId, bool ctrlPress);

        public abstract bool PartyInvite(string name, PartyType partyType);

        public abstract void PartyAcceptResponse(bool accept);

        public abstract void AnswerRessurectDialog(bool answer);

        public virtual void SendCustomPacketToServer(PacketBuilder packet)
        {
            tcpClient.SendPacketToServer(packet.GetBytes());
        }
    }
}
