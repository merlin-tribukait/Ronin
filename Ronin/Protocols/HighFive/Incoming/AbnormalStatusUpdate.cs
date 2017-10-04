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
    public class AbnormalStatusUpdate : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.AbnormalStatusUpdate;

        public AbnormalStatusUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int buffCount = reader.ReadShort();
            data.MainHero.Buffs.Clear();
            for (int i = 0; i < buffCount; i++)
            {
                int buffId = reader.ReadInt();
                int buffLevel = reader.ReadShort();
                int timeLeft = reader.ReadInt();
                var buff = new Buff(buffId,buffLevel,timeLeft);
                data.MainHero.Buffs.Add(buffId,buff);
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
