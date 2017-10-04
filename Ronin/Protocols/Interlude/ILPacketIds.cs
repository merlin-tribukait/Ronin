using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Protocols.Interlude
{
    public static class ILPacketIds
    {
        public enum ClientPrimary : byte
        {
            RequestMoveTo = 0x1,
            Action = 0x4,
            UseItem = 0x14,
            UseMagicSkill = 0x2f,
            CancelTarget = 0x37,
            RequestSkillList = 0x3f,
            ValidatePosition = 0x48,
            ConfirmDialog = 0xc5,
        }

        public enum ClientSecondary : short
        {
        }

        public enum ServerPrimary : byte
        {
            MoveTo = 0x1,
            CharInfo = 0x3,
            UserInfo = 0x4,
            Attack = 0x05,
            Die = 0x6,
            Revive = 0x7,
            DropItem = 0xc,
            StatusUpdate = 0xE,
            DeleteObject = 0x12,
            NpcInfo = 0x16,
            ItemList = 0x1b,
            InventoryUpdate = 0x27,
            TeleportToLocation = 0x28,
            TargetSelected = 0x29,
            TargetUnselected = 0x2a,
            StopMove = 0x47,
            MagicSkillUse = 0x48,
            PartySmallWindowAdd = 0x4f,
            PartySmallWindowAll = 0x4e,
            PartySmallWindowDeleteAll = 0x50,
            PartySmallWindowDelete = 0x51,
            PartySmallWindowUpdate = 0x52,
            SkillList = 0x58,
            RestartResponse = 0x5f,
            MoveToPawn = 0x60,
            SystemMessage = 0x64,
            AbnormalStatusUpdate = 0x7f,
            MyTargetSelect = 0xa6,
            PartyMemberPositions = 0xa7,
            PetInfo = 0xb1,
            Dialog = 0xed,
            PartySpelled = 0xee,
            EtcStatusUpdate = 0xF3,
        }

        public enum ServerSecondary : short
        {
        }
    }
}
