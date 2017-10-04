using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming.PartyWindow
{
    public class PartySpelled : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.PartySpelled;

        public PartySpelled(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int unitType = reader.ReadInt();//writeD(_activeChar.isServitor() ? 2 : _activeChar.isPet() ? 1 : 0);
            int objId = reader.ReadInt(); //writeD(_activeChar.getObjectId());
            int buffsCount = reader.ReadInt(); //writeD(_effects.size());

            if(data.MainHero.ObjectId != objId && data.AllUnits.All(unita => unita.ObjectId != objId))
                if(unitType==0)
                    data.Players.Add(objId, new Player() { ObjectId = objId });
                else
                    data.Npcs.Add(objId, new Npc() { ObjectId = objId });

            GameFigure unit = data.MainHero.ObjectId != objId ? data.AllUnits.First(fig => fig.ObjectId == objId) : data.MainHero;
            unit.Buffs.Clear();
            for (int i = 0; i < buffsCount; i++)
            {
                int Id = reader.ReadInt();
                int level = reader.ReadShort();
                int time = reader.ReadInt();
                Buff buff = new Buff(Id,level,time);
                unit.Buffs.Add(Id,buff);
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
