using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class CharInfo : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.CharInfo;

        public CharInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            if(reader.Size < 310)
                return;

            int x = reader.ReadInt(); //writeD(_x);
            int y = reader.ReadInt(); //writeD(_y);
            int z = reader.ReadInt(); //writeD(_z);
            reader.ReadInt(); //writeD(_heading);
            int objectId = reader.ReadInt(); //writeD(_activeChar.getObjectId());
            Player player;
            if (data.Players.ContainsKey(objectId))
            {
                player = data.Players[objectId];
            }
            else
            {
                player = new Player() {ObjectId = objectId};
                data.Players.Add(objectId, player);
            }
            player.X = x;
            player.Y = y;
            player.Z = z;

            player.Name = reader.ReadString();//writeS(_activeChar.getName());
            player.Race = (L2Race) reader.ReadInt(); //writeD(_activeChar.getRace().ordinal());
            player.Sex = (Sex) reader.ReadInt(); //writeD(_activeChar.getAppearance().getSex() ? 1 : 0);

            //if (_activeChar.getClassIndex() == 0)
            //    writeD(_activeChar.getClassId().getId());
            //else
            //    writeD(_activeChar.getBaseClass());
            player.Class = (L2Class) reader.ReadInt();

            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_HAIRALL));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_HEAD));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_RHAND));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_LHAND));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_GLOVES));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_CHEST));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_LEGS));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_FEET));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_BACK));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_RHAND));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_HAIR));
            //writeD(_inv.getPaperdollItemId(Inventory.PAPERDOLL_FACE));
            reader.SkipBytes(12*4);

            //// c6 new h's
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeD(_inv.getPaperdollAugmentationId(Inventory.PAPERDOLL_RHAND));
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeD(_inv.getPaperdollAugmentationId(Inventory.PAPERDOLL_LHAND));
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            reader.SkipBytes(24*2);

            reader.ReadInt(); //writeD(_activeChar.getPvpFlag());
            reader.ReadInt(); //writeD(_activeChar.getKarma());

            reader.ReadInt(); //writeD(_mAtkSpd);
            reader.ReadInt();//writeD(_pAtkSpd);

            reader.ReadInt();//writeD(_activeChar.getPvpFlag());
            reader.ReadInt();//writeD(_activeChar.getKarma());

            player.RunningSpeed = reader.ReadInt();//writeD(_runSpd);
            player.WalkingSpeed = reader.ReadInt();//writeD(_walkSpd);
            reader.ReadInt();//writeD(_runSpd); // swim run speed
            reader.ReadInt();//writeD(_walkSpd); // swim walk speed
            reader.ReadInt();//writeD(_runSpd); // fl run speed
            reader.ReadInt();//writeD(_walkSpd); // fl walk speed
            reader.ReadInt();//writeD(_runSpd); // fly run speed
            reader.ReadInt();//writeD(_walkSpd); // fly walk speed
            reader.ReadDouble();//writeF(_activeChar.getMovementSpeedMultiplier());
            reader.ReadDouble();//writeF(_activeChar.getAttackSpeedMultiplier());

            //if (_activeChar.getMountType() != 0)
            //{
            //    writeF(NpcTable.getInstance().getTemplate(_activeChar.getMountNpcId()).getCollisionRadius());
            //    writeF(NpcTable.getInstance().getTemplate(_activeChar.getMountNpcId()).getCollisionHeight());
            //}
            //else
            //{
            //    writeF(_activeChar.getBaseTemplate().getCollisionRadius());
            //    writeF(_activeChar.getBaseTemplate().getCollisionHeight());
            //}
            reader.ReadDouble();
            reader.ReadDouble();

            reader.ReadInt(); //writeD(_activeChar.getAppearance().getHairStyle());
            reader.ReadInt();//writeD(_activeChar.getAppearance().getHairColor());
            reader.ReadInt();//writeD(_activeChar.getAppearance().getFace());

            //if (gmSeeInvis)
            //    writeS("Invisible");
            //else
            //    writeS(_activeChar.getTitle());
            player.Title = reader.ReadString();

            reader.ReadInt(); //writeD(_activeChar.getClanId());
            reader.ReadInt();//writeD(_activeChar.getClanCrestId());
            reader.ReadInt();//writeD(_activeChar.getAllyId());
            reader.ReadInt();//writeD(_activeChar.getAllyCrestId());

            reader.ReadInt();//writeD(0);

            reader.ReadByte(); //writeC(_activeChar.isSitting() ? 0 : 1); // standing = 1 sitting = 0
            player.IsRunning = reader.ReadBool();//writeC(_activeChar.isRunning() ? 1 : 0); // running = 1 walking = 0
            reader.ReadByte();//writeC(_activeChar.isInCombat() ? 1 : 0);
            reader.ReadByte();//writeC(_activeChar.isAlikeDead() ? 1 : 0);

            //if (gmSeeInvis)
            //    writeC(0);
            //else
            //    writeC(_activeChar.getAppearance().getInvisible() ? 1 : 0); // invisible = 1 visible =0
            reader.ReadByte();

            reader.ReadByte();//writeC(_activeChar.getMountType()); // 1 on strider 2 on wyvern 0 no mount
            reader.ReadByte();//writeC(_activeChar.getPrivateStoreType().getId()); // 1 - sellshop

            //writeH(_activeChar.getCubics().size());
            //for (int id : _activeChar.getCubics().keySet())
            //    writeH(id);

            var cubicCount = reader.ReadShort(); //cubic count
            for (int i = 0; i < cubicCount; i++)
            {
                reader.ReadShort();
            }

            reader.ReadByte(); //writeC(_activeChar.isInPartyMatchRoom() ? 1 : 0);

            //if (gmSeeInvis)
            //    writeD((_activeChar.getAbnormalEffect() | AbnormalEffect.STEALTH.getMask()));
            //else
            //    writeD(_activeChar.getAbnormalEffect());
            reader.ReadInt();

            reader.ReadByte();//writeC(_activeChar.getRecomLeft());
            reader.ReadShort();//writeH(_activeChar.getRecomHave()); // Blue value for name (0 = white, 255 = pure blue)
            reader.ReadInt(); //writeD(_activeChar.getClassId().getId());

            player.MaxCombatPoints = reader.ReadInt();//writeD(_activeChar.getMaxCp());
            player.CombatPoints = reader.ReadInt(); //writeD((int)_activeChar.getCurrentCp());
            reader.ReadByte();//writeC(_activeChar.isMounted() ? 0 : _activeChar.getEnchantEffect());

            //if (_activeChar.getTeam() == 1 || (Config.PLAYER_SPAWN_PROTECTION > 0 && _activeChar.isSpawnProtected()))
            //    writeC(0x01); // team circle around feet 1= Blue, 2 = red
            //else if (_activeChar.getTeam() == 2)
            //    writeC(0x02); // team circle around feet 1= Blue, 2 = red
            //else
            //    writeC(0x00); // team circle around feet 1= Blue, 2 = red
            reader.ReadByte();

            reader.ReadInt(); //writeD(_activeChar.getClanCrestLargeId());
            reader.ReadByte(); //writeC(_activeChar.isNoble() ? 1 : 0); // Symbol on char menu ctrl+I
            reader.ReadByte();//writeC((_activeChar.isHero() || (_activeChar.isGM() && Config.GM_HERO_AURA)) ? 1 : 0); // Hero Aura

            reader.ReadByte();//writeC(_activeChar.isFishing() ? 1 : 0); // 0x01: Fishing Mode (Cant be undone by setting back to 0)

            //Location loc = _activeChar.getFishingLoc();
            //if (loc != null)
            //{
            //    writeD(loc.getX());
            //    writeD(loc.getY());
            //    writeD(loc.getZ());
            //}
            //else
            //{
            //    writeD(0);
            //    writeD(0);
            //    writeD(0);
            //}
            reader.SkipBytes(3*4);

            reader.ReadInt(); //writeD(_activeChar.getAppearance().getNameColor());

            reader.ReadInt(); //writeD(0x00); // isRunning() as in UserInfo?

            reader.ReadInt(); //writeD(_activeChar.getPledgeClass());
            reader.ReadInt(); //writeD(_activeChar.getPledgeType());

            reader.ReadInt(); //writeD(_activeChar.getAppearance().getTitleColor());

            //if (_activeChar.isCursedWeaponEquipped())
            //    writeD(CursedWeaponsManager.getInstance().getCurrentStage(_activeChar.getCursedWeaponEquippedId()) - 1);
            //else
            //    writeD(0x00);
            reader.ReadInt();
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
