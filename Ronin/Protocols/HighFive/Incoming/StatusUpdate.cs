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
    public class StatusUpdate : H5IncomingPacket
    {
        public const int LEVEL = 0x01;
        public const int EXP = 0x02;
        public const int STR = 0x03;
        public const int DEX = 0x04;
        public const int CON = 0x05;
        public const int INT = 0x06;
        public const int WIT = 0x07;
        public const int MEN = 0x08;

        public const int CUR_HP = 0x09;
        public const int MAX_HP = 0x0a;
        public const int CUR_MP = 0x0b;
        public const int MAX_MP = 0x0c;

        public const int SP = 0x0d;
        public const int CUR_LOAD = 0x0e;
        public const int MAX_LOAD = 0x0f;

        public const int P_ATK = 0x11;
        public const int ATK_SPD = 0x12;
        public const int P_DEF = 0x13;
        public const int EVASION = 0x14;
        public const int ACCURACY = 0x15;
        public const int CRITICAL = 0x16;
        public const int M_ATK = 0x17;
        public const int CAST_SPD = 0x18;
        public const int M_DEF = 0x19;
        public const int PVP_FLAG = 0x1a;
        public const int KARMA = 0x1b;

        public const int CUR_CP = 0x21;
        public const int MAX_CP = 0x22;

        public StatusUpdate(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }


        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.StatusUpdate;

        public override void Parse(L2PlayerData data)
        {
            GameFigure unit = null;

            int objId = reader.ReadInt();
            if (data.MainHero.ObjectId == objId)
                unit = data.MainHero;
            else if (data.Players.ContainsKey(objId))
                unit = data.Players[objId];
            else if (data.Npcs.ContainsKey(objId))
                unit = data.Npcs[objId];

            if (unit == null)
                return;

            int atributeCount = reader.ReadInt();
            for (int i = 0; i < atributeCount; i++)
            {
                int atriType = reader.ReadInt();
                int atriValue = reader.ReadInt();
                switch (atriType)
                {
                    case CUR_HP:
                        unit.Health = atriValue;
                        break;
                    case MAX_HP:
                        unit.MaxHealth = atriValue;
                        break;
                    case CUR_MP:
                        unit.Mana = atriValue;
                        break;
                    case MAX_MP:
                        unit.MaxMana = atriValue;
                        break;
                    case CUR_CP:
                        unit.CombatPoints = atriValue;
                        break;
                    case MAX_CP:
                        unit.MaxCombatPoints = atriValue;
                        break;
                    case LEVEL:
                        unit.Level = atriValue;
                        break;
                }
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
