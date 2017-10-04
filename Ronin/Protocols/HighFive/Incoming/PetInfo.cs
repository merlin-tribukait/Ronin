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
    public class PetInfo : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.PetInfo;

        public PetInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadInt(); //writeD(_type);
            int objId = reader.ReadInt(); //writeD(obj_id);
            Npc npc = data.Npcs.ContainsKey(objId) ? data.Npcs[objId] : new Npc();

            if (!data.Npcs.ContainsKey(objId))
                data.Npcs.Add(objId, npc);

            npc.ObjectId = objId;
            npc.UnitId = reader.ReadInt() - 1000000; //writeD(npc_id + 1000000);
            reader.ReadInt(); //writeD(0); // 1=attackable
            npc.X = reader.ReadInt(); //writeD(_loc.x);
            npc.Y = reader.ReadInt();//writeD(_loc.y);
            npc.Z = reader.ReadInt();//writeD(_loc.z);
            reader.ReadInt(); //writeD(_loc.h);
            reader.ReadInt();//writeD(0);
            reader.ReadInt();//writeD(MAtkSpd);
            reader.ReadInt();//writeD(PAtkSpd);
            npc.RunningSpeed = reader.ReadInt();//writeD(_runSpd);
            reader.ReadInt(); //writeD(_walkSpd);
            reader.ReadInt();//writeD(_swimRunSpd);
            reader.ReadInt();//writeD(_swimWalkSpd);
            reader.ReadInt();//writeD(0); // flRunSpeed
            reader.ReadInt();//writeD(0); // flWalkSpeed
            reader.ReadInt();//writeD(_flyRunSpd);
            reader.ReadInt();//writeD(_flyWalkSpd);
            npc.MovementSpeedMultiplier = reader.ReadDouble();//writeF(1/*_cha.getProperMultiplier()*/);
            reader.ReadDouble();//writeF(1/*_cha.getAttackSpeedMultiplier()*/);
            reader.ReadDouble();//writeF(col_redius);
            reader.ReadDouble();//writeF(col_height);
            reader.ReadInt();//writeD(0); // right hand weapon
            reader.ReadInt();//writeD(0);
            reader.ReadInt();//writeD(0); // left hand weapon
            reader.ReadByte(); //writeC(1); // name above char 1=true ... ??
            reader.ReadByte(); //writeC(runing); // running=1
            reader.ReadByte(); //writeC(incombat); // attacking 1=true
            npc.IsDead = reader.ReadByte() == 1; //writeC(dead); // dead 1=true
            reader.ReadByte();//writeC(_showSpawnAnimation); // invisible ?? 0=false  1=true   2=summoned (only works if model has a summon animation)
            reader.ReadInt();//writeD(-1);
            npc.Name = reader.ReadString(); //writeS(_name);
            reader.ReadInt();//writeD(-1);
            npc.Title = reader.ReadString(); //writeS(title);
            reader.ReadInt();//writeD(1);
            reader.ReadInt();//writeD(pvp_flag); //0=white, 1=purple, 2=purpleblink, if its greater then karma = purple
            reader.ReadInt();//writeD(karma); // hmm karma ??
            reader.ReadInt();//writeD(curFed); // how fed it is
            reader.ReadInt();//writeD(maxFed); //max fed it can be
            npc.Health = reader.ReadInt(); //writeD(curHp); //current hp
            npc.MaxHealth = reader.ReadInt();//writeD(maxHp); // max hp
            npc.Mana = reader.ReadInt();//writeD(curMp); //current mp
            npc.MaxMana = reader.ReadInt();//writeD(maxMp); //max mp
            reader.ReadInt();//writeD(_sp); //sp
            npc.Level = reader.ReadInt();//writeD(level);// lvl
            reader.ReadLong(); //writeQ(exp);
            reader.ReadLong(); //writeQ(exp_this_lvl); // 0%  absolute value
            reader.ReadLong(); //writeQ(exp_next_lvl); // 100% absoulte value
            reader.ReadInt();//writeD(curLoad); //weight
            reader.ReadInt();//writeD(maxLoad); //max weight it can carry
            reader.ReadInt();//writeD(PAtk);//patk
            reader.ReadInt();//writeD(PDef);//pdef
            reader.ReadInt();//writeD(MAtk);//matk
            reader.ReadInt();//writeD(MDef);//mdef
            reader.ReadInt();//writeD(Accuracy);//accuracy
            reader.ReadInt();//writeD(Evasion);//evasion
            reader.ReadInt();//writeD(Crit);//critical
            reader.ReadInt();//writeD(_runSpd);//speed
            reader.ReadInt();//writeD(PAtkSpd);//atkspeed
            reader.ReadInt();//writeD(MAtkSpd);//casting speed
            reader.ReadInt();//writeD(_abnormalEffect); //c2  abnormal visual effect... bleed=1; poison=2; bleed?=4;
            reader.ReadShort();//writeH(rideable);
            reader.ReadByte(); //writeC(0); // c2
            reader.ReadShort(); //writeH(0); // ??
            reader.ReadByte(); //writeC(_team.ordinal()); // team aura (1 = blue, 2 = red)
            reader.ReadInt();//writeD(ss);
            reader.ReadInt();//writeD(sps);
            reader.ReadInt();//writeD(type);
            reader.ReadInt();//writeD(_abnormalEffect2);
            data.MainHero.PlayerSummons.Clear();
            data.MainHero.PlayerSummons.Add(npc);
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
