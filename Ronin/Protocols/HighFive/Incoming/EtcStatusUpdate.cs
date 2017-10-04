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
    public class EtcStatusUpdate : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.EtcStatusUpdate;

        public EtcStatusUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            reader.ReadInt(); //writeD(_activeChar.getCharges()); // 1-7 increase force, lvl
            reader.ReadInt();//writeD(_activeChar.getWeightPenalty()); // 1-4 weight penalty, lvl (1=50%, 2=66.6%, 3=80%, 4=100%)
            reader.ReadInt();//writeD((_activeChar.getMessageRefusal() || _activeChar.isChatBanned() || _activeChar.isSilenceMode()) ? 1 : 0); // 1 = block all chat
            reader.ReadInt();//writeD(_activeChar.isInsideZone(ZoneId.DANGER_AREA) ? 1 : 0); // 1 = danger area
            reader.ReadInt();//writeD(_activeChar.getExpertiseWeaponPenalty()); // Weapon Grade Penalty [1-4]
            reader.ReadInt();//writeD(_activeChar.getExpertiseArmorPenalty()); // Armor Grade Penalty [1-4]
            reader.ReadInt();//writeD(_activeChar.hasCharmOfCourage() ? 1 : 0); // 1 = charm of courage (allows resurrection on the same spot upon death on the siege battlefield)
            data.MainHero.DeathPenaltyLevel = reader.ReadInt();//writeD(_activeChar.getDeathPenaltyBuffLevel()); // 1-15 death penalty, lvl (combat ability decreased due to death)
            reader.ReadInt();//writeD(_activeChar.getChargedSouls());
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
