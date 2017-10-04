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
    public class PetStatusUpdate : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.PetStatusUpdate;

        public PetStatusUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadInt(); //writeD(type);
            int objId = reader.ReadInt(); //writeD(obj_id);
            var pet = data.MainHero.PlayerSummons.FirstOrDefault();
            if(pet == null)
                return;

            pet.X = reader.ReadInt(); //writeD(_loc.x);
            pet.Y = reader.ReadInt();//writeD(_loc.y);
            pet.Z = reader.ReadInt();//writeD(_loc.z);
            pet.Title = reader.ReadString();//writeS(title);
            reader.ReadInt();//writeD(curFed);
            reader.ReadInt();//writeD(maxFed);
            pet.Health = reader.ReadInt();//writeD(curHp);
            pet.MaxHealth = reader.ReadInt();//writeD(maxHp);
            pet.Mana = reader.ReadInt();//writeD(curMp);
            pet.MaxMana = reader.ReadInt();//writeD(maxMp);
            pet.Level = reader.ReadInt();//writeD(level);
            reader.ReadLong(); //writeQ(exp);
            reader.ReadLong();//writeQ(exp_this_lvl);// 0% absolute value
            reader.ReadLong();//writeQ(exp_next_lvl);// 100% absolute value
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
