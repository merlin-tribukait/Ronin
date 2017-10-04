using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ronin.Annotations;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Data
{
    public class L2PlayerData : INotifyPropertyChanged
    {
        /// <summary>
        /// Structure containing properties of the player.
        /// </summary>
        private MainHero _mainHero = new MainHero();

        private GameState _gameState = GameState.AccountLogin;

        //List containing the object ids of the monsters that need to be looted by the character. (All monsters that were hit either by the user or party member)
        public List<int> MonstersToLoot = new List<int>();

        //A flag indicating whether there is currently open dialog dialog for accepting a resurrection.
        public bool PendingRessurectDialog;

        public int LastDialogToken;

        //Variable containing the name of a player that has offered resurrection last.
        public string LastRessurector;

        // Flag confirming for received HTML dialog.
        public bool ReceivedHTMLFlag;

        // Flag confirming that the skills properties were updated. (such as cooldown, cast time)
        public bool SkillUpdateFlag;

        public bool AttackFlag;

        // Variable returning the last received vendor shoplist id.
        public int ShopListId;

        public PartyType PartyType;

        // Flag confirming player's teleportation.
        public bool AppearFlag;

        public long LastSkillLaunch;
        public int LastCastTime;

        public int PartyLeaderObjectId;

        //avoid confusion of the pve logic that attack skill was used.
        public int LastUsedNukeId;

        public long RequestBuffSkillCastStamp;

        public string LastPartyInviter;
        public long LastPartyInviteStamp;

        public bool? PendingInvite;

        public bool PlayerIsCasting
        {
            get { return (Math.Abs(Environment.TickCount - LastSkillLaunch) < LastCastTime); }
        }

        /// <summary>
        /// Collection containing all successfully landed skills.
        /// </summary>
        public MultiThreadObservableDictionary<int, int> LandedSkills = new MultiThreadObservableDictionary<int, int>();

        /// <summary>
        /// Collection containing all the npcs. (including monsters)
        /// </summary>
        public MultiThreadObservableDictionary<int, Npc> Npcs = new MultiThreadObservableDictionary<int, Npc>();

        /// <summary>
        /// Collection containing all the surrounding players.
        /// </summary>
        public MultiThreadObservableDictionary<int, Player> Players = new MultiThreadObservableDictionary<int, Player>();

        public List<Player> PartyMembers
        {
            get { return this.Players.Values.ToList().Where(player => player.IsMyPartyMember).ToList(); }
        }

        public List<Player> SurroundingPlayers
        {
            get { return this.Players.Values.ToList().Where(player => player.RangeTo(MainHero) < 5000).ToList(); }
        }

        private List<Npc> surroundingMonstersCache = new List<Npc>();
        private DateTime surroundingMonstersCacheStamp = DateTime.MinValue;

        public List<Npc> SurroundingMonsters
        {
            get
            {
                if (DateTime.Now.Subtract(surroundingMonstersCacheStamp).TotalMilliseconds < 500)
                    return surroundingMonstersCache;

                surroundingMonstersCacheStamp = DateTime.Now;
                //return list;
                surroundingMonstersCache = this.Npcs.Values.ToList().Where(mob => mob.IsMonster && !mob.IsPet && !mob.IsSummon && (mob.IsInvisible == 0 || (mob.IsInvisible == 1 && Math.Abs(Environment.TickCount - mob.AddStamp) < 300000)
                                                                                                                    || mob.IsInvisible == 2)).ToList();
                return surroundingMonstersCache;
            }// && Math.Abs(Environment.TickCount - mob.AddStamp) < 60000
        }

        public List<Npc> SurroundingMonstersIncludingInvalidEntries
        {
            get
            {
                return this.Npcs.Values.ToList().Where(mob => mob.IsMonster && !mob.IsPet && !mob.IsSummon && (mob.IsInvisible == 0 || (mob.IsInvisible == 1)
                                                                                                                  || mob.IsInvisible == 2)).ToList();
            }// && Math.Abs(Environment.TickCount - mob.AddStamp) < 60000
        }

        public List<Npc> SurroundingNpcs
        {
            get { return this.Npcs.Values.ToList().Where(mob => mob.RangeTo(MainHero) < 5000).ToList(); }
        }

        public List<GameFigure> AllUnits
        {
            get
            {
                List<GameFigure> gameFigures = new List<GameFigure>();
                foreach (var npc in Npcs)
                {
                    gameFigures.Add(npc.Value);
                }

                foreach (var keyValuePair in Players)
                {
                    gameFigures.Add(keyValuePair.Value);
                }

                //gameFigures.Add(MainHero);

                return gameFigures;
            }
        }

        /// <summary>
        /// Structure containing properties of the player.
        /// </summary>
        public MainHero MainHero
        {
            get { return _mainHero; }
            set
            {
                _mainHero = value;
                OnPropertyChanged();

                _mainHero.PropertyChanged += (sender, args) =>
                {
                    if(args.PropertyName == nameof(_mainHero.Name) && _mainHero.Name != null && GameState!= GameState.InGame)
                        MainPlayerLogin?.Invoke();
                };
            }
        }

        public GameState GameState
        {
            get { return _gameState; }
            set
            {
                _gameState = value; 
                //LogHelper.GetLogger().Debug(value);
                OnPropertyChanged();
            }
        }

        public delegate void OnLoginEventHandler();

        public event OnLoginEventHandler MainPlayerLogin;

        /// <summary>
        /// Collection containing all taken quests.
        /// </summary>
        public MultiThreadObservableDictionary<int, Quest> Quests = new MultiThreadObservableDictionary<int, Quest>();

        /// <summary>
        /// Collection containing all learned skills.
        /// </summary>
        public MultiThreadObservableDictionary<int, Skill> Skills = new MultiThreadObservableDictionary<int, Skill>();

        /// <summary>
        /// Collection containing skills that could be learned by the player.
        /// </summary>
        //public MultiThreadObservableDictionary<int, NewSkill> NewSkills = new MultiThreadObservableDictionary<int, NewSkill>();

        /// <summary>
        /// Collection containing all items in the player's inventory.
        /// </summary>
        public MultiThreadObservableDictionary<int, StashedItem> Inventory = new MultiThreadObservableDictionary<int, StashedItem>();

        /// <summary>
        /// Collection containing all items on the ground.
        /// </summary>
        public MultiThreadObservableDictionary<int, DroppedItem> DroppedItems = new MultiThreadObservableDictionary<int, DroppedItem>();

        /// <summary>
        /// List containing all the clickable links from the last received HTML. Accessible by index.
        /// </summary>
        public List<string> HtmlLinksByIndex = new List<string>();

        public Dictionary<string, string> HtmlLinks = new Dictionary<string, string>();

        public bool QuestBitFlagAtPos(int questId, int flagPos)
        {
            if (Quests.ContainsKey(questId) == false)
            {
                throw new Exception("Missing quest.");
            }

            int flags = Quests[questId].StateFlags;
            if (flagPos > 0)
                flags >>= (flagPos - 1);

            return (flags & 1) == 1;
        }

        public L2PlayerData()
        {
            MainPlayerLogin += () =>
            {
                GameState = GameState.InGame;
            };


            _mainHero.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_mainHero.Name) && _mainHero.Name!= null && GameState != GameState.InGame)
                    MainPlayerLogin?.Invoke();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
