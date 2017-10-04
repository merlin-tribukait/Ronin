using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class UserInfo : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.UserInfo;

        public UserInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            data.MainHero.X = reader.ReadInt();
            data.MainHero.Y = reader.ReadInt();
            data.MainHero.Z = reader.ReadInt();
            reader.ReadInt(); //vehicle obj id
            data.MainHero.ObjectId = reader.ReadInt();
            data.MainHero.Name = reader.ReadString();
            data.MainHero.Race = (L2Race) reader.ReadInt();
            data.MainHero.Sex = (Sex) reader.ReadInt();
            reader.ReadInt(); //base class ?
            data.MainHero.Level = reader.ReadInt();
            data.MainHero.Experience = reader.ReadLong();
            reader.ReadDouble(); //% for level up ?
            reader.ReadInt();//STR
            reader.ReadInt();//DEX
            reader.ReadInt();//CON
            reader.ReadInt();//INT
            reader.ReadInt();//WIT
            reader.ReadInt();//MEN
            data.MainHero.MaxHealth = reader.ReadInt();
            data.MainHero.Health = reader.ReadInt();
            data.MainHero.MaxMana = reader.ReadInt();
            data.MainHero.Mana = reader.ReadInt();
            data.MainHero.SkillPonts = reader.ReadInt();
            reader.ReadInt();//Current load
            reader.ReadInt(); //Max load
            reader.ReadInt(); // 20 no weapon, 40 weapon equipped
            reader.SkipBytes(21 * 4);//paperdoll obj ids
            //reader.SkipBytes(21 * 4);//paperdoll display ids
            reader.SkipBytes(12*4);
            data.MainHero.WeaponDisplayId = reader.ReadInt();
            reader.SkipBytes(8*4);
            reader.SkipBytes(21 * 4);//paperdoll aug ids
            for (int i = 0; i < 15; i++)
            {
                reader.ReadInt();
            }

            reader.ReadInt(); //talisman slots
            reader.ReadInt();//can equip cloak
            reader.ReadInt();//patk
            reader.ReadInt();//patk spd
            reader.ReadInt();//pdef
            reader.ReadInt();//evasion
            reader.ReadInt();//accuracy
            reader.ReadInt();//crit
            reader.ReadInt();//matk
            reader.ReadInt();//matk spd
            reader.ReadInt();//patk spd
            reader.ReadInt();//mdef
            reader.ReadInt(); //pvp flag
            reader.ReadInt(); //Karma
            data.MainHero.RunningSpeed = reader.ReadInt();
            reader.ReadInt(); //walking speed
            reader.ReadInt(); //swimming run speed
            reader.ReadInt(); //swimming walk speed
            reader.ReadInt(); //fly run speed
            reader.ReadInt(); //fly walk speed
            reader.ReadInt(); //fly run speed
            reader.ReadInt(); //fly walk speed
            data.MainHero.MovementSpeedMultiplier = reader.ReadDouble();
            reader.ReadDouble(); //atk spd multiplier
            reader.ReadDouble(); //collision radius
            reader.ReadDouble(); //collision height
            reader.ReadInt(); //hair style
            reader.ReadInt(); //hair color
            reader.ReadInt(); //face
            reader.ReadInt(); //isGm
            reader.ReadString();//title
            reader.ReadInt(); //clan id
            reader.ReadInt(); //clan crest id
            reader.ReadInt(); //alliance id
            reader.ReadInt(); //alliance crest id
            reader.ReadInt(); //relation
            reader.ReadByte();//mount type
            reader.ReadByte();//private store type
            reader.ReadByte();//has dwarven craft
            reader.ReadInt();//pk kills
            reader.ReadInt();//pvp kills
            var cubicCount = reader.ReadShort();
            for (int i = 0; i < cubicCount; i++)
            {
                reader.ReadShort();
            }

            reader.ReadByte();//is in party looking room
            reader.ReadInt();//abnormal effects
            reader.ReadByte();//environment
            reader.ReadInt();//clan priviliges
            reader.ReadShort(); //recs left
            reader.ReadShort();//recs received
            reader.ReadInt(); //mount npc id
            reader.ReadShort();//inventory limit
            data.MainHero.ClassId = (L2Class) reader.ReadInt();
            reader.ReadInt(); //unk
            data.MainHero.MaxCombatPoints = reader.ReadInt();
            data.MainHero.CombatPoints = reader.ReadInt();
            reader.ReadByte();//enchant glow
            reader.ReadByte();//duel team
            reader.ReadInt();//clan crest large id
            reader.ReadByte();//is noble
            reader.ReadByte();//is hero
            reader.ReadByte();//is fishing
            reader.ReadInt(); //fish lure x
            reader.ReadInt(); //fish lure y
            reader.ReadInt(); //fish lure z
            reader.ReadInt(); //name color
            data.MainHero.IsRunning = reader.ReadBool();//is running
            reader.ReadInt(); //pledge class (Vagabond.. etc)
            reader.ReadInt();//Pledge type
            reader.ReadInt(); //title color
            reader.ReadInt(); //cursed wep level
            reader.ReadInt();//transformation display ID
            reader.ReadShort();//wep element atk type
            reader.ReadShort();//wep element atk value
            reader.ReadShort();//def element value FIRE
            reader.ReadShort();//def element value WATER
            reader.ReadShort();//def element value WIND
            reader.ReadShort();//def element value EARTH
            reader.ReadShort();//def element value HOLY
            reader.ReadShort();//def element value DARK
            reader.ReadInt();//agathion id
            reader.ReadInt(); //fame
            reader.ReadInt();//is minimap allowed
            reader.ReadInt();//vit points
            reader.ReadInt();//abnormal visual special effects
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
