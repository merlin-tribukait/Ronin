using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class EtcStatusUpdate : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.EtcStatusUpdate;

        public EtcStatusUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadInt(); //writeD(_activeChar.getCharges());
            reader.ReadInt();//writeD(_activeChar.getWeightPenalty());
            reader.ReadInt();//writeD((_activeChar.isInRefusalMode() || _activeChar.isChatBanned()) ? 1 : 0);
            reader.ReadInt();//writeD(_activeChar.isInsideZone(ZoneId.DANGER_AREA) ? 1 : 0);
            reader.ReadInt();//writeD((_activeChar.getExpertiseWeaponPenalty() || _activeChar.getExpertiseArmorPenalty() > 0) ? 1 : 0);
            reader.ReadInt();//writeD(_activeChar.isAffected(L2EffectFlag.CHARM_OF_COURAGE) ? 1 : 0);
            data.MainHero.DeathPenaltyLevel = reader.ReadInt();//writeD(_activeChar.getDeathPenaltyBuffLevel());
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
