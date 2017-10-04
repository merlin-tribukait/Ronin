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
    public class CharInfo : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.CharInfo;

        public CharInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int x = reader.ReadInt();
            int y = reader.ReadInt();
            int z = reader.ReadInt();
            reader.ReadInt(); //vehicle object id
            int objectId = reader.ReadInt();
            Player player;
            if (data.Players.ContainsKey(objectId))
            {
                player = data.Players[objectId];
            }
            else
            {
                player = new Player();
                data.Players.Add(objectId, player);
            }

            player.X = x;
            player.Y = y;
            player.Z = z;
            player.ObjectId = objectId;
            player.Name = reader.ReadString();
            player.Race = (L2Race)reader.ReadInt();
            player.Sex = (Sex)reader.ReadInt();
            reader.ReadInt(); // base class?

            reader.ReadInt(); //Underwear
            player.Gear_Helmet = reader.ReadInt();
            player.Gear_Weapon = reader.ReadInt();
            player.Gear_ShieldSigil = reader.ReadInt();
            player.Gear_Gloves = reader.ReadInt();
            player.Gear_UpperBody = reader.ReadInt();
            player.Gear_LowerBody = reader.ReadInt();
            player.Gear_Boots = reader.ReadInt();
            player.Gear_Cloak = reader.ReadInt();
            player.Gear_WeaponTwoHand = reader.ReadInt();
            reader.ReadInt();//Hair Accessory (top)
            reader.ReadInt();//Hair Accessory (bottom)
            reader.ReadInt();//Right Bracelet
            reader.ReadInt();//Left Bracelet
            reader.ReadInt();//Talisman (1)
            reader.ReadInt();//Talisman (2)
            reader.ReadInt();//Talisman (3)
            reader.ReadInt();//Talisman (4)
            reader.ReadInt();//Talisman (5)
            reader.ReadInt(); //Talisman (6)
            reader.ReadInt(); //Belt
            reader.ReadShort(); //Underwear augmentation effect (1)
            reader.ReadShort(); //Underwear augmentation effect (2)
            reader.ReadShort(); //Headgear augmentation effect (1)
            reader.ReadShort(); //Headgear augmentation effect (2)
            reader.ReadShort(); //Weapon augmentation effect (1)
            reader.ReadShort(); //Weapon augmentation effect (2)
            reader.ReadShort(); //Shield [Sigil] augmentation effect (1)
            reader.ReadShort(); //Shield [Sigil] augmentation effect (2)
            reader.ReadShort();//Gloves augmentation effect (1)
            reader.ReadShort();//Gloves augmentation effect (2)
            reader.ReadShort();//Upper Body augmentation effect (1)
            reader.ReadShort();//Upper Body augmentation effect (2)
            reader.ReadShort();//Lower Body augmentation effect (1)
            reader.ReadShort();//Lower Body augmentation effect (2)
            reader.ReadShort();//Boots augmentation effect (1)
            reader.ReadShort();//Boots augmentation effect (2)
            reader.ReadShort();//Cloak augmentation effect (1)
            reader.ReadShort();//Cloak augmentation effect (2)
            reader.ReadShort();//Weapon / Two Handed augmentation effect (1)
            reader.ReadShort();//Weapon / Two Handed augmentation effect (2)
            reader.ReadShort();//Hair Accessory (top) augmentation effect (1)
            reader.ReadShort();//Hair Accessory (top) augmentation effect (2)
            reader.ReadShort();//Hair Accessory (bottom) augmentation effect (1)
            reader.ReadShort();//Hair Accessory (bottom) augmentation effect (2)
            reader.ReadShort();//Right Bracelet augmentation effect (1)
            reader.ReadShort();//Right Bracelet augmentation effect (2)
            reader.ReadShort();//Left Bracelet augmentation effect (1)
            reader.ReadShort();//Left Bracelet augmentation effect (2)
            reader.ReadShort();//Talisman (1) augmentation effect (1)
            reader.ReadShort();//Talisman (1) augmentation effect (2)
            reader.ReadShort();//Talisman (2) augmentation effect (1)
            reader.ReadShort();//Talisman (2) augmentation effect (2)
            reader.ReadShort();//Talisman (3) augmentation effect (1)
            reader.ReadShort();//Talisman (3) augmentation effect (2)
            reader.ReadShort();//Talisman (4) augmentation effect (1)
            reader.ReadShort();//Talisman (4) augmentation effect (2)
            reader.ReadShort();//Talisman (5) augmentation effect (1)
            reader.ReadShort();//Talisman (5) augmentation effect (2)
            reader.ReadShort();//Talisman (6) augmentation effect (1)
            reader.ReadShort();//Talisman (6) augmentation effect (2)
            reader.ReadShort();//Belt augmentation effect (1)
            reader.ReadShort();//Belt augmentation effect (2)
            var resa = reader.ReadInt();//Talisman slots
            reader.ReadInt();//Cloak style
            player.PvpFlag = reader.ReadInt();
            player.Karma = reader.ReadInt();
            player.CastingSpeed = reader.ReadInt();
            player.AttackSpeed = reader.ReadInt();
            reader.ReadInt(); //unk
            player.RunningSpeed = reader.ReadInt();
            player.WalkingSpeed = reader.ReadInt();
            player.RunningSpeedWater = reader.ReadInt();
            player.WalkingSpeedWater = reader.ReadInt();
            player.RunningSpeedMounted = reader.ReadInt();
            player.WalkingSpeedMounted = reader.ReadInt();
            player.RunningSpeedFlyMounted = reader.ReadInt();
            player.WalkingSpeedFlyMounted = reader.ReadInt();
            player.MovementSpeedMultiplier = reader.ReadDouble();
            player.AttackSpeedMultiplier = reader.ReadDouble();
            reader.ReadDouble();//Collision radius
            reader.ReadDouble(); //Collision height
            reader.ReadInt(); //Hair style
            reader.ReadInt(); //Hair color
            reader.ReadInt(); //Face
            player.Title = reader.ReadString();
            reader.ReadInt(); //Pledge id
            reader.ReadInt(); //Pledge crest id
            reader.ReadInt(); //Alliance id
            reader.ReadInt(); //Alliance crest id
            reader.ReadByte(); //Waiting mode?
            player.IsRunning = reader.ReadBool();
            reader.ReadByte(); //In combat
            player.IsDead = reader.ReadBool();
            reader.ReadByte(); //Builder?
            reader.ReadByte(); //mount type
            reader.ReadByte(); //private store
            var cubicCount = reader.ReadShort(); //cubic count
            for (int i = 0; i < cubicCount; i++)
            {
                reader.ReadShort();
            }
            reader.ReadByte(); //Looking for party
            reader.ReadInt(); //Abnormal effect
            reader.ReadByte(); //Environment
            reader.ReadShort(); //Evaluation score
            reader.ReadInt(); //Mount
            player.Class = (L2Class)reader.ReadInt();
            reader.ReadInt();//unk
            reader.ReadByte(); //Weapon enchant glow
            reader.ReadByte(); //Duel team
            reader.ReadInt(); //Pledge insignia ID
            player.IsNoble = reader.ReadBool();
            player.IsHero = reader.ReadBool();
            reader.ReadBool(); //is fishing
            reader.ReadInt();// fishing lure X
            reader.ReadInt();// fishing lure Y
            reader.ReadInt();// fishing lure Z
            reader.ReadInt(); //Name color
            reader.ReadInt(); //Heading
            reader.ReadInt(); //social class
            reader.ReadInt(); //pledge unit
            reader.ReadInt(); //title color
            reader.ReadInt(); //cursed weapon level
            reader.ReadInt(); //Pledge reputation
            reader.ReadInt(); //Transformation
            reader.ReadInt(); //Agathion
            reader.ReadInt();//unk
            reader.ReadInt();//special effects
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
