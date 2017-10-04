using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class NpcInfo : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.NpcInfo;

        public NpcInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            Npc npc = data.Npcs.ContainsKey(objId) ? data.Npcs[objId] : new Npc() { ObjectId = objId};
            //LogHelper.GetLogger().Debug($"add {objId}");
            if (!data.Npcs.ContainsKey(objId))
                data.Npcs.Add(objId, npc);

            npc.AddStamp = Environment.TickCount;
            npc.UnitId = reader.ReadInt() - 1000000; //writeD(_idTemplate + 1000000); // npctype id
            npc.IsMonster = reader.ReadInt() == 1; //writeD(_isAttackable ? 1 : 0);
            npc.X = reader.ReadInt(); //writeD(_x);
            npc.Y = reader.ReadInt();//writeD(_y);
            npc.Z = reader.ReadInt();//writeD(_z);
            reader.ReadInt(); //writeD(_heading);
            reader.ReadInt(); //writeD(0x00);
            reader.ReadInt(); //writeD(_mAtkSpd);
            reader.ReadInt();//writeD(_pAtkSpd);
            npc.RunningSpeed = reader.ReadInt();//writeD(_runSpd);
            npc.WalkingSpeed = reader.ReadInt();//writeD(_walkSpd);
            reader.ReadInt();//writeD(_swimRunSpd/* 0x32 */); // swimspeed
            reader.ReadInt();//writeD(_swimWalkSpd/* 0x32 */); // swimspeed
            reader.ReadInt();//writeD(_flRunSpd);
            reader.ReadInt();//writeD(_flWalkSpd);
            reader.ReadInt();//writeD(_flyRunSpd);
            reader.ReadInt();//writeD(_flyWalkSpd);
            npc.MovementSpeedMultiplier= reader.ReadDouble(); //writeF(1.1/* _activeChar.getProperMultiplier() */);
            //// writeF(1/*_activeChar.getAttackSpeedMultiplier()*/);
            reader.ReadDouble(); //writeF(_pAtkSpd / 277.478340719);
            reader.ReadDouble();//writeF(_collisionRadius);
            reader.ReadDouble();//writeF(_collisionHeight);
            reader.ReadInt(); //writeD(_rhand); // right hand weapon
            reader.ReadInt();//writeD(0);
            reader.ReadInt();//writeD(_lhand); // left hand weapon
            reader.ReadByte(); //writeC(1); // name above char 1=true ... ??
            npc.IsRunning = reader.ReadBool();//writeC(_activeChar.isRunning() ? 1 : 0);
            reader.ReadByte();//writeC(_activeChar.isInCombat() ? 1 : 0);
            reader.ReadByte();//writeC(_activeChar.isAlikeDead() ? 1 : 0);
            npc.IsInvisible = reader.ReadByte();//writeC(_isSummoned ? 2 : 0); // invisible ?? 0=false 1=true 2=summoned (only works if model has a summon animation)
           

            npc.Name = reader.ReadString();//writeS(_name);
            npc.Title = reader.ReadString();//writeS(_title);
            reader.ReadInt();//writeD(0);
            reader.ReadInt();//writeD(0);
            reader.ReadInt();//writeD(0000); // hmm karma ??
            reader.ReadInt();//writeD(_activeChar.getAbnormalEffect()); // C2
            reader.ReadInt();//writeD(0000); // C2
            reader.ReadInt();//writeD(0000); // C2
            reader.ReadInt();//writeD(0000); // C2
            reader.ReadInt();//writeD(0000); // C2
            reader.ReadInt();//writeC(0000); // C2

            reader.ReadByte(); //writeC(0x00); // C3 team circle 1-blue, 2-red
            reader.ReadDouble(); //writeF(_collisionRadius);
            reader.ReadDouble(); //writeF(_collisionHeight);
            reader.ReadInt();//writeD(0x00); // C4
            //reader.ReadInt();//writeD(0x00); // C6
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
