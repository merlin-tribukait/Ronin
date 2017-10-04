using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Constants
{
    public static class H5PacketIds
    {
        public enum ClientPrimary : byte
        {
            Logout = 0,
            CharacterCreate = 0x0C,
            CharacterDelete = 0xd,
            ProtocolVersionInit = 0xe,
            MoveTo = 0xf,
            EnterWorld = 0x11,
            CharSelected = 0x12,
            RequestItemsList = 0x14,
            UseItem = 0x19,
            Action = 0x1f,
            RequestLinkHtml = 0x22,
            RequestBypassToServer = 0x23,
            UseSkill = 0x39,
            Appearing = 0x3a,
            SellItems = 0x37,
            BuyItem = 0x40,
            RequestPartyInvite = 0x42,
            RequestPartyAccept = 0x43,
            RequestWithDrawalParty = 0x44,
            CancelTarget = 0x48,
            Say = 0x49,
            RequestAction = 0x56,
            ValidatePosition = 0x59,
            DestroyItem = 0x60,
            AbandonQuest = 0x63,
            ConfirmDialog = 0xc6,
            LearnSkill = 0x7c,
            MultiSellTransaction = 0xb0,
            RequestTutorialQuestionMark = 0x87,
            RequestRestartPoint = 0x7d,
            Command = 0xb3,
            Extended = 0xd0,
        }

        public enum ClientSecondary : short
        {
            ClaimReward = 286,
            CallToAwake = 0xA1,
            RequestToAwake = 0xA2,
            RequestDispel = 0x4b,
        }

        public enum ServerPrimary : byte
        {
            Die = 0,
            Revive = 1,
            SpawnItem = 5,
            DeleteObject = 8,
            CharSelected = 0xb,
            NpcInfo = 0xc,
            ItemList = 0x11,
            Dropitem = 0x16,
            GetItem = 0x17,
            StatusUpdate = 0x18,
            NpcHtmlMessage = 0x19,
            InventoryUpdate = 0x21,
            TeleportToLocation = 0x22,
            TargetSelected = 0x23,
            TargetUnselected = 0x24,
            SittingModeChange = 0x29,
            MyTargetSelected = 0xb9,
            SocialAction = 0x27,
            MoveToLocation = 0x2f,
            CharInfo = 0x31,
            UserInfo = 0x32,
            Attack = 0x33,
            AskJoinParty = 0x39,
            JoinPartyResponse = 0x3A,
            MagicSkillUse = 0x48,
            SkillList = 0x5f,
            StopMoveToPawn = 0x47,
            SystemMessage = 0x62,
            RestartResponse = 0x71,
            MoveToPawn = 0x72,
            GameGuardQuery = 0x74,
            AbnormalStatusUpdate = 0x85, //buffs
            QuestList = 0x86,
            NewSkills = 0x90,
            StaticObject = 0x9f,
            PetInfo = 0xb2,
            PetStatusUpdate = 0xb6,
            PetDelete = 0xb7,
            NetPing = 0xD9,
            ConfirmDialog = 0xF3,
            EtcStatusUpdate = 0xF9,
            PartySpelled = 0xF4, //local party buffs (for summons and pets on the main char)
            PartyPositionUpdate = 0xBA,
            PartySmallWindowAdd = 0x4f,
            PartySmallWindowAll = 0x4e,
            PartySmallWindowDeleteAll = 0x50,
            PartySmallWindowDelete = 0x51,
            PartySmallWindowUpdate = 0x52,

            Extended = 0xFE,
        }

        public enum ServerSecondary : short
        {
            ExPartySpelled = 346, //global party buffs
            ExPartyPetWindowAdd = 0x18,
            ExPartyPetWindowDelete = 0x6b,
            ExNpcHtmlMessage = 0x8e,
            ExShowScreenMessage = 0x39,
            ExAbnormalStatusUpdateFromTarget = 0xe6,
            ReceivedMultiSellList = 0xb8,
            QuestItemList = 0xc6,
            TeleportLocationActivate = 0x4a,
            CompassZoneCode = 0x33,
        }
    }
}
