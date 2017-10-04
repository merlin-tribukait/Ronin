using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class UserInfo: ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.UserInfo;

        public UserInfo(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            data.MainHero.X = reader.ReadInt();//writeD(_activeChar.getX());
            data.MainHero.Y = reader.ReadInt(); //writeD(_activeChar.getY());
            data.MainHero.Z = reader.ReadInt();//writeD(_activeChar.getZ());
            reader.ReadInt(); //writeD(_activeChar.getHeading());
            data.MainHero.ObjectId = reader.ReadInt(); //writeD(_activeChar.getObjectId());

            //String name = _activeChar.getName();
            //if (_activeChar.getPoly().isMorphed())
            //{
            //    NpcTemplate polyObj = NpcTable.getInstance().getTemplate(_activeChar.getPoly().getPolyId());
            //    if (polyObj != null)
            //        name = polyObj.getName();
            //}
            data.MainHero.Name = reader.ReadString();//writeS(name);

            data.MainHero.Race = (L2Race) reader.ReadInt(); //writeD(_activeChar.getRace().ordinal());
            data.MainHero.Sex = (Sex)reader.ReadInt();//writeD(_activeChar.getAppearance().getSex() ? 1 : 0);

            //if (_activeChar.getClassIndex() == 0)
            //    writeD(_activeChar.getClassId().getId());
            //else
            //    writeD(_activeChar.getBaseClass());
            data.MainHero.ClassId = (L2Class) reader.ReadInt();

            data.MainHero.Level = reader.ReadInt(); //writeD(_activeChar.getLevel());
            reader.ReadLong(); //writeQ(_activeChar.getExp());
            reader.ReadInt(); //writeD(_activeChar.getSTR());
            reader.ReadInt();//writeD(_activeChar.getDEX());
            reader.ReadInt();//writeD(_activeChar.getCON());
            reader.ReadInt();//writeD(_activeChar.getINT());
            reader.ReadInt();//writeD(_activeChar.getWIT());
            reader.ReadInt();//writeD(_activeChar.getMEN());
            data.MainHero.MaxHealth = reader.ReadInt();//writeD(_activeChar.getMaxHp());
            data.MainHero.Health = reader.ReadInt();//writeD((int)_activeChar.getCurrentHp());
            data.MainHero.MaxMana = reader.ReadInt();//writeD(_activeChar.getMaxMp());
            data.MainHero.Mana = reader.ReadInt();//writeD((int)_activeChar.getCurrentMp());
            reader.ReadInt(); //writeD(_activeChar.getSp());
            reader.ReadInt(); //writeD(_activeChar.getCurrentLoad());
            reader.ReadInt(); //writeD(_activeChar.getMaxLoad());

            reader.ReadInt();//writeD(_activeChar.getActiveWeaponItem() != null ? 40 : 20); // 20 no weapon, 40 weapon equipped

            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_HAIRALL));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_REAR));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_LEAR));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_NECK));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_RFINGER));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_LFINGER));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_HEAD));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_RHAND));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_LHAND));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_GLOVES));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_CHEST));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_LEGS));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_FEET));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_BACK));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_RHAND));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_HAIR));
            //writeD(_activeChar.getInventory().getPaperdollObjectId(Inventory.PAPERDOLL_FACE));
            reader.SkipBytes(17*4);

            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_HAIRALL));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_REAR));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_LEAR));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_NECK));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_RFINGER));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_LFINGER));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_HEAD));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_RHAND));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_LHAND));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_GLOVES));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_CHEST));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_LEGS));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_FEET));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_BACK));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_RHAND));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_HAIR));
            //writeD(_activeChar.getInventory().getPaperdollItemId(Inventory.PAPERDOLL_FACE));
            reader.SkipBytes(14 * 4);
            data.MainHero.WeaponDisplayId = reader.ReadInt();
            reader.SkipBytes(2 * 4);

            //// c6 new h's
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
            //writeH(0x00);
            //writeH(0x00);
            //writeD(_activeChar.getInventory().getPaperdollAugmentationId(Inventory.PAPERDOLL_RHAND));
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
            //writeD(_activeChar.getInventory().getPaperdollAugmentationId(Inventory.PAPERDOLL_LHAND));
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //writeH(0x00);
            //// end of c6 new h's
            reader.SkipBytes(34*2);

            reader.ReadInt(); //writeD(_activeChar.getPAtk(null));
            reader.ReadInt();//writeD(_activeChar.getPAtkSpd());
            reader.ReadInt();//writeD(_activeChar.getPDef(null));
            reader.ReadInt();//writeD(_activeChar.getEvasionRate(null));
            reader.ReadInt();//writeD(_activeChar.getAccuracy());
            reader.ReadInt();//writeD(_activeChar.getCriticalHit(null, null));
            reader.ReadInt();//writeD(_activeChar.getMAtk(null, null));

            reader.ReadInt();//writeD(_activeChar.getMAtkSpd());
            reader.ReadInt();//writeD(_activeChar.getPAtkSpd());

            reader.ReadInt();//writeD(_activeChar.getMDef(null, null));

            reader.ReadInt();//writeD(_activeChar.getPvpFlag()); // 0-non-pvp 1-pvp = violett name
            reader.ReadInt();//writeD(_activeChar.getKarma());

            data.MainHero.RunningSpeed = reader.ReadInt();//writeD(_runSpd);
            data.MainHero.WalkingSpeed = reader.ReadInt(); //writeD(_walkSpd);
            reader.ReadInt(); //writeD(_runSpd); // swim run speed
            reader.ReadInt(); //writeD(_walkSpd); // swim walk speed
            reader.ReadInt(); //writeD(0);
            reader.ReadInt(); //writeD(0);
            reader.ReadInt(); //writeD(_activeChar.isFlying() ? _runSpd : 0); // fly speed
            reader.ReadInt(); //writeD(_activeChar.isFlying() ? _walkSpd : 0); // fly speed
            data.MainHero.MovementSpeedMultiplier = reader.ReadDouble(); //writeF(_moveMultiplier);
            reader.ReadDouble();//writeF(_activeChar.getAttackSpeedMultiplier());

            //L2Summon pet = _activeChar.getPet();
            //if (_activeChar.getMountType() != 0 && pet != null)
            //{
            //    writeF(pet.getTemplate().getCollisionRadius());
            //    writeF(pet.getTemplate().getCollisionHeight());
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
            reader.ReadInt();//writeD(_activeChar.isGM() ? 1 : 0); // builder level

            reader.ReadString(); //writeS(_activeChar.getPoly().isMorphed() ? "Morphed" : _activeChar.getTitle());

            reader.ReadInt(); //writeD(_activeChar.getClanId());
            reader.ReadInt();//writeD(_activeChar.getClanCrestId());
            reader.ReadInt();//writeD(_activeChar.getAllyId());
            reader.ReadInt();//writeD(_activeChar.getAllyCrestId()); // ally crest id
            //                                      // 0x40 leader rights
            //                                      // siege flags: attacker - 0x180 sword over name, defender - 0x80 shield, 0xC0 crown (|leader), 0x1C0 flag (|leader)
            reader.ReadInt();//writeD(_relation);
            reader.ReadByte();//writeC(_activeChar.getMountType()); // mount type
            reader.ReadByte();//writeC(_activeChar.getPrivateStoreType().getId());
            reader.ReadByte();//writeC(_activeChar.hasDwarvenCraft() ? 1 : 0);
            reader.ReadInt();//writeD(_activeChar.getPkKills());
            reader.ReadInt();//writeD(_activeChar.getPvpKills());

            //writeH(_activeChar.getCubics().size());
            //for (int id : _activeChar.getCubics().keySet())
            //    writeH(id);
            var cubicCount = reader.ReadShort();
            for (int i = 0; i < cubicCount; i++)
            {
                reader.ReadShort();
            }

            reader.ReadByte(); //writeC(_activeChar.isInPartyMatchRoom() ? 1 : 0);

            //if (_activeChar.getAppearance().getInvisible() && _activeChar.isGM())
            //    writeD(_activeChar.getAbnormalEffect() | AbnormalEffect.STEALTH.getMask());
            //else
            //    writeD(_activeChar.getAbnormalEffect());
            //writeC(0x00);
            reader.ReadInt();
            reader.ReadByte();

            reader.ReadInt(); //writeD(_activeChar.getClanPrivileges());

            reader.ReadShort(); //writeH(_activeChar.getRecomLeft()); // c2 recommendations remaining
            reader.ReadShort();//writeH(_activeChar.getRecomHave()); // c2 recommendations received
            reader.ReadInt();//writeD(_activeChar.getMountNpcId() > 0 ? _activeChar.getMountNpcId() + 1000000 : 0);
            reader.ReadShort();//writeH(_activeChar.getInventoryLimit());

            reader.ReadInt(); //base class? //writeD(_activeChar.getClassId().getId());
            reader.ReadInt(); //writeD(0x00); // special effects? circles around player...
            data.MainHero.MaxCombatPoints = reader.ReadInt(); //writeD(_activeChar.getMaxCp());
            data.MainHero.CombatPoints = reader.ReadInt(); //writeD((int)_activeChar.getCurrentCp());
            reader.ReadByte(); //writeC(_activeChar.isMounted() ? 0 : _activeChar.getEnchantEffect());

            //if (_activeChar.getTeam() == 1 || (Config.PLAYER_SPAWN_PROTECTION > 0 && _activeChar.isSpawnProtected()))
            //    writeC(0x01); // team circle around feet 1= Blue, 2 = red
            //else if (_activeChar.getTeam() == 2)
            //    writeC(0x02); // team circle around feet 1= Blue, 2 = red
            //else
            //    writeC(0x00); // team circle around feet 1= Blue, 2 = red
            reader.ReadByte();

            reader.ReadInt(); //writeD(_activeChar.getClanCrestLargeId());
            reader.ReadByte(); //writeC(_activeChar.isNoble() ? 1 : 0); // 0x01: symbol on char menu ctrl+I
            reader.ReadByte();//writeC(_activeChar.isHero() || (_activeChar.isGM() && Config.GM_HERO_AURA) ? 1 : 0); // 0x01: Hero Aura

            reader.ReadByte(); //writeC(_activeChar.isFishing() ? 1 : 0); // Fishing Mode

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

            //// new c5
            data.MainHero.IsRunning = reader.ReadBool();//writeC(_activeChar.isRunning() ? 0x01 : 0x00); // changes the Speed display on Status Window

            reader.ReadInt(); //writeD(_activeChar.getPledgeClass()); // changes the text above CP on Status Window
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
