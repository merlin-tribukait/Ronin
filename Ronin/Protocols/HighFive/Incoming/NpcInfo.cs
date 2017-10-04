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
    public class NpcInfo : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.NpcInfo;

        public NpcInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            Npc npc = data.Npcs.ContainsKey(objId) ? data.Npcs[objId] : new Npc();
            npc.ObjectId = objId;
            npc.UnitId = reader.ReadInt() - 1000000;
            npc.IsMonster = reader.ReadInt() == 1;
            npc.X = reader.ReadInt();
            npc.Y = reader.ReadInt();
            npc.Z = reader.ReadInt();
            reader.ReadInt();//heading
            reader.ReadInt(); //unk
            reader.ReadInt();//writeD(_mAtkSpd);
            reader.ReadInt();//writeD(_pAtkSpd);
            npc.RunningSpeed = reader.ReadInt();//writeD(_runSpd);
            npc.WalkingSpeed = reader.ReadInt();//writeD(_walkSpd);
            reader.ReadInt();//writeD(_swimRunSpd);
            reader.ReadInt();//writeD(_swimWalkSpd);
            reader.ReadInt();//writeD(_flyRunSpd);
            reader.ReadInt();//writeD(_flyWalkSpd);
            reader.ReadInt();//writeD(_flyRunSpd);
            reader.ReadInt();//writeD(_flyWalkSpd);
            npc.MovementSpeedMultiplier = reader.ReadDouble();//writeF(_moveMultiplier);
            reader.ReadDouble();//writeF(_npc.getAttackSpeedMultiplier());
            reader.ReadDouble();//writeF(_collisionRadius);
            reader.ReadDouble();//writeF(_collisionHeight);
            reader.ReadInt();//writeD(_rhand); // right hand weapon
            reader.ReadInt();//writeD(_chest);
            reader.ReadInt();//writeD(_lhand); // left hand weapon
            reader.ReadByte();//writeC(1); // name above char 1=true ... ??
            npc.IsRunning = reader.ReadBool();//writeC(_npc.isRunning() ? 1 : 0);
            reader.ReadByte();//writeC(_npc.isInCombat() ? 1 : 0);
            reader.ReadByte();//writeC(_npc.isAlikeDead() ? 1 : 0);
            int shadybyte = reader.ReadByte();//writeC(_isSummoned ? 2 : 0); // invisible ?? 0=false 1=true 2=summoned (only works if model has a summon animation)

            reader.ReadInt();//writeD(-1); // High Five NPCString ID
            var res = reader.ReadString();//writeS(_name);
            reader.ReadInt();//writeD(-1); // High Five NPCString ID
            var resa = reader.ReadString();//writeS(_title);
            reader.ReadInt();//writeD(0x00); // Title color 0=client default
            reader.ReadInt();//writeD(0x00); // pvp flag
            reader.ReadInt();//writeD(0x00); // karma

            reader.ReadInt();//writeD(_npc.isInvisible() ? _npc.getAbnormalVisualEffects() | AbnormalVisualEffect.STEALTH.getMask() : _npc.getAbnormalVisualEffects());
            reader.ReadInt();//writeD(_clanId); // clan id
            reader.ReadInt();//writeD(_clanCrest); // crest id
            reader.ReadInt();//writeD(_allyId); // ally id
            reader.ReadInt();//writeD(_allyCrest); // all crest

            reader.ReadByte();//writeC(_npc.isInsideZone(ZoneId.WATER) ? 1 : _npc.isFlying() ? 2 : 0); // C2
            reader.ReadByte();//writeC(_npc.getTeam().getId());

            reader.ReadDouble();//writeF(_collisionRadius);
            reader.ReadDouble();//writeF(_collisionHeight);
            reader.ReadInt();//writeD(_enchantEffect); // C4
            reader.ReadInt();//writeD(_npc.isFlying() ? 1 : 0); // C6
            reader.ReadInt();//writeD(0x00);
            reader.ReadInt();//writeD(_npc.getColorEffect()); // CT1.5 Pet form and skills, Color effect
            npc.IsMonster = reader.ReadByte() == 1 && npc.IsMonster;//writeC(_npc.isTargetable() ? 0x01 : 0x00);
            reader.ReadByte();//writeC(_npc.isShowName() ? 0x01 : 0x00);
            //reader.ReadInt();//writeD(_npc.getAbnormalVisualEffectSpecial());
            //reader.ReadInt();//writeD(_displayEffect);

            if(!data.Npcs.ContainsKey(objId))
                data.Npcs.Add(objId,npc);
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
