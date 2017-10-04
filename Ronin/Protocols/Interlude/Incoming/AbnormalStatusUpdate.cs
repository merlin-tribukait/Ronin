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
    public class AbnormalStatusUpdate : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.AbnormalStatusUpdate;

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
                var buff = new Buff(buffId, buffLevel, timeLeft);
                data.MainHero.Buffs.Add(buffId, buff);
            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
