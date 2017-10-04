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
    public class Die : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.Die;

        public Die(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int objId = reader.ReadInt();
            reader.ReadInt();//writeD(get(RestartType.TO_VILLAGE)); // to nearest village
            reader.ReadInt();//writeD(get(RestartType.TO_CLANHALL)); // to hide away
            reader.ReadInt();//writeD(get(RestartType.TO_CASTLE)); // to castle
            reader.ReadInt();//writeD(get(RestartType.TO_FLAG));// to siege HQ
            bool isSweepable = reader.ReadInt() == 1;
            if (data.AllUnits.Any(unit => unit.ObjectId == objId))
            {
                var instance = data.AllUnits.FirstOrDefault(unit => unit.ObjectId == objId);
                if (instance == null)
                    return;

                instance.IsDead = true;
                if (instance is Npc)
                    ((Npc) instance).IsSweepable = isSweepable;
            }
            else if (data.MainHero.ObjectId == objId)
                data.MainHero.IsDead = true;
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
