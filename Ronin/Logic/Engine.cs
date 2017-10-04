using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using Newtonsoft.Json;
using Ronin.Annotations;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Logic.Handlers;
using Ronin.Protocols;
using Ronin.Utilities;
using Timer = System.Timers.Timer;

namespace Ronin.Logic
{
    public class Engine : INotifyPropertyChanged
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        private L2PlayerData _data;
        private ActionsController _actionsController;
        private Thread _engineThread;
        private bool _running;
        //private Timer _attackTimer;
        //private Timer _targeter;
        private Thread _attackThread;
        private Thread _targeterThread;
        private bool _activeTargeter;
        private bool _activeAttacker = false;
        private bool _initialized;
        private HashSet<int> _blockedMonsters = new HashSet<int>();

        #region Configurations

        private bool _ignoreMonsters;

        [JsonIgnore]
        public bool IgnoreMonsters
        {
            get { return _ignoreMonsters; }
            set
            {
                _ignoreMonsters = value;
                OnPropertyChanged();
            }
        }

        private bool _moveToCenterIfNoMobs;

        [JsonIgnore]
        public bool MoveToCenterIfNoMobs
        {
            get { return _moveToCenterIfNoMobs; }
            set
            {
                _moveToCenterIfNoMobs = value;
                OnPropertyChanged();
            }
        }

        private string _monsterFilterStr = "";

        [JsonIgnore]
        public string MonsterFilterStr
        {
            get { return _monsterFilterStr; }
            set
            {
                _monsterFilterStr = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIFormElement> MonsterFilter
        {
            get
            {
                MultiThreadObservableCollection<UIFormElement> list = new MultiThreadObservableCollection<UIFormElement>();
                try
                {
                    var mosnterSplit = MonsterFilterStr.Trim().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var mob in mosnterSplit)
                    {
                        if (ExportedData.NpcIdToName.Any(pair => pair.Value.Equals(mob, StringComparison.OrdinalIgnoreCase)))
                            list.Add(new UIFormElement { Enable = true, Name = mob });
                    }

                    foreach (var mob in _data.SurroundingMonsters)
                    {
                        if (!list.Any(unit => ((string)unit.Name).Equals(mob.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new UIFormElement { Enable = false, Name = mob.Name });
                        }
                    }
                }
                catch (Exception)
                {

                }


                return list;
            }
        }

        private CombatTargetType _combatTargetType = CombatTargetType.Off;

        public CombatTargetType CombatTargetType
        {
            get { return _combatTargetType; }
            set
            {
                _combatTargetType = value;
                OnPropertyChanged();
            }
        }

        private int _range = 2500;

        public int Range
        {
            get { return _range; }
            set
            {
                _range = value;
                OnPropertyChanged();
            }
        }

        private int _pointX, _pointY, _pointZ;

        public int PointX
        {
            get { return _pointX; }
            set
            {
                _pointX = value;
                OnPropertyChanged();
            }
        }

        public int PointY
        {
            get { return _pointY; }
            set
            {
                _pointY = value;
                OnPropertyChanged();
            }
        }

        public int PointZ
        {
            get { return _pointZ; }
            set
            {
                _pointZ = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Handlers

        private AttackHandler _attackHandler;
        private NukeHandler _nukeHandler;
        private SelfHealBuffHandler _selfHealBuffHandler;
        private PartyHealBuffHandler _partyHealBuffHandler;
        private AssistHandler _assistHandler;
        private FollowHandler _followHandler;
        private DialogHandler _dialogHandler;
        private PickupHandler _pickupHandler;


        public AttackHandler AttackHandler
        {
            get { return _attackHandler; }
            set { _attackHandler = value; }
        }

        public PickupHandler PickupHandler
        {
            get { return _pickupHandler; }
            set { _pickupHandler = value; }
        }

        public NukeHandler NukeHandler
        {
            get { return _nukeHandler; }
            set { _nukeHandler = value; }
        }

        public SelfHealBuffHandler SelfHealBuffHandler
        {
            get { return _selfHealBuffHandler; }
            set { _selfHealBuffHandler = value; }
        }

        public PartyHealBuffHandler PartyHealBuffHandler
        {
            get { return _partyHealBuffHandler; }
            set { _partyHealBuffHandler = value; }
        }

        public AssistHandler AssistHandler
        {
            get { return _assistHandler; }
            set { _assistHandler = value; }
        }

        public FollowHandler FollowHandler
        {
            get { return _followHandler; }
            set { _followHandler = value; }
        }

        public DialogHandler DialogHandler
        {
            get { return _dialogHandler; }
            set { _dialogHandler = value; }
        }

        #endregion

        public void Start()
        {
            if (!Initialized || _data.MainHero.Level == 0 || Running)
                return;

            targetMob = null;
            Running = true;
            _engineThread = new Thread(EngineCycle);
            _engineThread.Name = "Engine Thread";
            _engineThread.Start();
        }

        public void Abort()
        {
            //_attackTimer?.Stop();
            //_targeter?.Stop();
            _attackThread?.Abort();
            _targeterThread?.Abort();
            _activeTargeter = false;
            Running = false;
            _engineThread?.Abort();
        }

        public Engine()
        {
            _attackHandler = new AttackHandler();
            _nukeHandler = new NukeHandler();
            _selfHealBuffHandler = new SelfHealBuffHandler();
            PartyHealBuffHandler = new PartyHealBuffHandler();
            AssistHandler = new AssistHandler();
            _followHandler = new FollowHandler();
            _dialogHandler = new DialogHandler();
            _pickupHandler = new PickupHandler();
        }

        public void Init(L2PlayerData data, ActionsController actionsController)
        {
            _data = data;
            _actionsController = actionsController;


            _data.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_data.GameState))
                {
                    if (_data.GameState != GameState.InGame)
                        Abort();

                    if (_data.GameState == GameState.AccountLogin)
                    {
                        _initialized = false;
                        _attackHandler.Initialiased = false;
                        _nukeHandler.Initialiased = false;
                        _selfHealBuffHandler.Initialiased = false;
                        PartyHealBuffHandler.Initialiased = false;
                        AssistHandler.Initialiased = false;
                        _followHandler.Initialiased = false;
                        _dialogHandler.Initialiased = false;
                        _pickupHandler.Initialiased = false;
                    }
                }
            };

            _attackHandler.Init(data, actionsController);
            _nukeHandler.Init(data, actionsController);
            _selfHealBuffHandler.Init(data, actionsController);
            PartyHealBuffHandler.Init(data, actionsController);
            AssistHandler.Init(data, actionsController);
            _followHandler.Init(data, actionsController);
            _dialogHandler.Init(data, actionsController);
            _pickupHandler.Init(data, actionsController);
            _initialized = true;
        }

        private void Attacker()
        {
            _activeAttacker = true;
            while (true)
            {
                while (!_activeAttacker || !Initialized || _data.MainHero.IsDead || !Running)
                {
                    Thread.Sleep(100);

                    if (_data.MainHero.IsDead)
                        DialogHandler.HandleDialogs();
                }

                DialogHandler.HandleDialogs();

                try
                {
                    if (Math.Abs(Environment.TickCount - PartyHealBuffHandler.OutOfPartyStamp) / 1000 >
                    PartyHealBuffHandler.OutOfPartyTimer && PartyHealBuffHandler.OutOfParty
                    && _data.PartyMembers.Count > 0 
                    && PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null 
                    && !_data.PlayerIsCasting
                    && Running)
                    {
                        PartyHealBuffHandler.OutOfPartyStamp = Environment.TickCount;
                        _actionsController.LeaveParty();
                    }

                    SelfHealBuffHandler.RemoveUnwantedBuffs();

                    if (SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null &&
                        !_data.PlayerIsCasting && _activeAttacker)
                    {
                        //log.Debug("selfhealbuff");
                        int count = 0;
                        var rule = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        if (rule != null)
                        {
                            if (rule.UseOnPet && _data.MainHero.PlayerSummons.Count > 0 &&
                                _data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) > rule.MaxDistance &&
                                _data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) < 3000)
                            {
                                //_targeter.Stop();
                                _activeTargeter = false;

                                while (_data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) > rule.MaxDistance &&
                                       count < 200)
                                {
                                    Thread.Sleep(100);
                                    count++;
                                }
                            }

                            if (count < 200)
                            {
                                if (rule.RequiresTarget)
                                {
                                    //_targeter.Stop();
                                    _activeTargeter = false;
                                }

                                SelfHealBuffHandler.Cast(rule);
                            }
                        }
                        if (!_activeTargeter)
                            _activeTargeter = true; //_targeter.Start();
                    }
                    if (PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null &&
                            !_data.PlayerIsCasting && _activeAttacker)
                    {
                        //log.Debug("pt healbuff");
                        int count = 0;
                        var rule = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        if (rule != null)
                        {
                            double distanceToTarget = _data.MainHero.RangeTo(rule.ChosenTarget);
                            if (distanceToTarget > rule.MaxDistance &&
                                distanceToTarget < 3000)
                            {
                                //_targeter.Stop();
                                _activeTargeter = false;

                                while (_data.MainHero.RangeTo(rule.ChosenTarget) > rule.MaxDistance && count < 200 && Running)
                                {
                                    if (count % 10 == 0)
                                        _actionsController.MoveToRaw(rule.ChosenTarget.X, rule.ChosenTarget.Y,
                                            rule.ChosenTarget.Z);

                                    Thread.Sleep(100);
                                    count++;
                                }
                            }

                            if (count < 200 && Running)
                            {
                                //_targeter.Stop();
                                _activeTargeter = false;
                                PartyHealBuffHandler.Cast(rule);
                                //Thread.Sleep(_data.Skills[rule.PartyHealBuffId].CastTime);
                            }
                        }
                        if (!_activeTargeter)
                            _activeTargeter = true; //_targeter.Start();
                    }

                    if (!Running || targetMob == null || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob) == null) || !_activeAttacker || (CombatTargetType == CombatTargetType.Off && !AssistHandler.AssistActivated) ||
                            _data.MainHero.TargetObjectId != targetMob.ObjectId ||
                            _nukeHandler.Nuke(targetMob) || _attackHandler.Attack(targetMob))
                    {
                        //log.Debug("Attack");
                        //log.Debug(targetMob==null);
                        //log.Debug(_activeTargeter);
                        //log.Debug(_targeter.Enabled);
                        if (CombatTargetType == CombatTargetType.Off)
                            _data.AttackFlag = false;

                        if (!_activeTargeter)
                            _activeTargeter = true;
                    }
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {
                    log.Debug(ex);
                }

                Thread.Sleep(100);
            }
        }

        private void Targeter()
        {
            int stampMobObjId = 0;
            int stampMobHealth = 0;
            int stampMobHealthCount = 0;
            long stampMob = 0;
            Locatable moveStamp = new Locatable(0, 0, 0);
            int moveStampStuckCount = 0;

            _activeTargeter = true;

            while (true)
            {
                while (!_activeTargeter || !Running || _data.MainHero.IsDead)// || (CombatTargetType == CombatTargetType.Off && !AssistHandler.AssistActivated && !_followHandler.FollowActivated)
                {
                    Thread.Sleep(100);

                    if(!Running)
                        return;
                }

                try
                {
                    while ((targetMob == null || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob) == null)
                            || (_blockedMonsters.Contains(targetMob.ObjectId) && ClosestAttacker != targetMob)
                            || !_data.Npcs.ContainsKey(targetMob.ObjectId)
                            || SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null
                            || PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null
                            || (_data.PlayerIsCasting) // && _data.AttackFlag == false
                            || (AssistHandler.AssistActivated && AssistHandler.ShouldAssist() && (MobForSweep == null || targetMob != MobForSweep)))

                           && _activeTargeter && Running)
                    {
                        if (!AssistHandler.AssistActivated
                            && (targetMob == null
                                || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob) == null)
                                || !_data.Npcs.ContainsKey(targetMob.ObjectId)
                                || (_blockedMonsters.Contains(targetMob.ObjectId) && ClosestAttacker != targetMob)
                            )
                        )
                        {
                            while (PickupHandler.ShouldPickup() && ClosestAttacker == null 
                                && MobForSweep == null
                                && SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null
                                && PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null
                                && Running)
                            {
                                PickupHandler.Pickup();
                                Thread.Sleep(300);
                            }

                            if (targetMob?.ObjectId != SuitableTarget?.ObjectId)
                                _data.AttackFlag = false;

                            if (SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null
                                && PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null)
                                targetMob = SuitableTarget;

                            if (targetMob == null && CombatTargetType == CombatTargetType.AroundPoint &&
                                MoveToCenterIfNoMobs && _data.MainHero.RangeTo(new Locatable(PointX, PointY, PointZ)) > 50)
                            {
                                _actionsController.MoveToRaw(PointX, PointY, PointZ);
                                Thread.Sleep(500);
                            }
                        }

                        while (_followHandler.FollowActivated && _followHandler.Follow() &&
                               !AssistHandler.AssistActivated && _activeTargeter && Running)
                            Thread.Sleep(100);

                        //log.Debug("No Target");
                        Thread.Sleep(50);
                        

                        


                        var selfnuke = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        var partynuke = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        if ((selfnuke == null || !selfnuke.RequiresTarget)
                            &&
                            (partynuke == null || !partynuke.RequiresTarget)
                            && ((!_data.PlayerIsCasting && Math.Abs(Environment.TickCount - _data.RequestBuffSkillCastStamp) > 500) || _data.LastSkillLaunch == _data.LastUsedNukeId)
                        ) //
                            if (AssistHandler.AssistActivated && MobForSweep == null)
                            {
                                AssistHandler.Assist();
                                if (_data.MainHero.TargetObjectId != 0 &&
                                    _data.SurroundingMonsters.Any(mob => mob.ObjectId == _data.MainHero.TargetObjectId)
                                    && AssistHandler.ActiveAssister?.TargetObjectId == _data.MainHero.TargetObjectId)
                                    targetMob =
                                        _data.AllUnits.First(unit => unit.ObjectId == _data.MainHero.TargetObjectId);
                                else
                                {
                                    targetMob = null;
                                }
                            }

                        if(MobForSweep != null)
                            targetMob = MobForSweep;
                    }

                    if (!AssistHandler.AssistActivated || MobForSweep != null)
                    {
                        if (targetMob != null && (CombatTargetType != CombatTargetType.Off) || MobForSweep != null)
                            if (!_actionsController.TargetByObjectId(targetMob.ObjectId))
                            {
                                _blockedMonsters.Add(targetMob.ObjectId);
                                log.Debug("blocked due to target fail");
                            }
                    }
                    else
                    {
                        var selfnuke = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        var partynuke = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
                        if ((selfnuke == null || !selfnuke.RequiresTarget)
                            &&
                            (partynuke == null || !partynuke.RequiresTarget)
                            && ((!_data.PlayerIsCasting && Math.Abs(Environment.TickCount - _data.RequestBuffSkillCastStamp) > 500) || _data.LastSkillLaunch == _data.LastUsedNukeId)
                        ) ////(_data.PlayerIsCasting && _data.LastSkillLaunch == _data.LastUsedNukeId)
                            AssistHandler.Assist();
                        if (_data.MainHero.TargetObjectId != 0 &&
                            _data.SurroundingMonsters.Any(mob => mob.ObjectId == _data.MainHero.TargetObjectId))
                            targetMob = _data.AllUnits.First(unit => unit.ObjectId == _data.MainHero.TargetObjectId);
                        else
                        {
                            targetMob = null;
                        }
                    }

                    if (targetMob != null && stampMobObjId != targetMob.ObjectId && targetMob.Health > 0)
                    {
                        stampMobObjId = targetMob.ObjectId;
                        stampMobHealth = targetMob.Health;
                        stampMob = 0;
                    }

                    if (targetMob != null && _data.MainHero.TargetObjectId == targetMob.ObjectId && !targetMob.IsDead)
                        if (
                            (_nukeHandler.GetNukeToCast(targetMob) != null &&
                             _data.MainHero.RangeTo(targetMob) >
                             _nukeHandler.GetNukeToCast(targetMob)?.MaxDistance + 100 &&
                             !_data.PlayerIsCasting) ||
                            (_attackHandler.UseMeleeAttack &&
                             targetMob.RangeTo(new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY,
                                 _data.MainHero.ValidatedZ)) > _attackHandler.MaximumAttackDistance + 100 &&
                             !_data.PlayerIsCasting)
                        )
                        {
                            _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
                            Thread.Sleep(1200);
                            if (_nukeHandler.GetNukeToCast(targetMob) == null && !_attackHandler.UseMeleeAttack)
                                _actionsController.StopMove();

                            if (
                                moveStamp.RangeTo(new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY,
                                    _data.MainHero.ValidatedZ)) < 10)
                                moveStampStuckCount++;
                            else
                            {
                                moveStampStuckCount = 0;
                            }

                            //log.Debug(moveStamp);
                            moveStamp = new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY,
                                _data.MainHero.ValidatedZ);

                            if (moveStampStuckCount > 5)
                            {
                                _blockedMonsters.Add(targetMob.ObjectId);
                                log.Debug("blocked due to physical stuck stamp check");
                                moveStampStuckCount = 0;
                            }

                            //if (ClosestAttacker != null && ClosestAttacker != targetMob
                            //    && targetMob.Health == targetMob.MaxHealth
                            //    && targetMob.TargetObjectId != _data.MainHero.ObjectId
                            //    && !AssistHandler.AssistActivated
                            //    && !_data.AttackFlag)
                            //{
                            //    Thread.Sleep(500);
                            //    if (ClosestAttacker != null
                            //    && ClosestAttacker != targetMob
                            //    && targetMob.Health == targetMob.MaxHealth
                            //    && targetMob.TargetObjectId != _data.MainHero.ObjectId
                            //    && !AssistHandler.AssistActivated
                            //    && !_data.AttackFlag)
                            //        targetMob = ClosestAttacker;
                            //}
                        }
                        else if (targetMob.ObjectId == stampMobObjId && !targetMob.IsDead &&
                                 (_nukeHandler.GetNukeToCast(targetMob) != null || _attackHandler.UseMeleeAttack))
                        {
                            moveStampStuckCount = 0;

                            if (stampMob == 0)
                                stampMob = Environment.TickCount;

                            if (Math.Abs(Environment.TickCount - stampMob) > 1000 && !_data.PlayerIsCasting)
                            {
                                if (targetMob.Health >= stampMobHealth && (!_data.AttackFlag || Math.Abs(Environment.TickCount - stampMob) > 3000) && !targetMob.IsDead)
                                {
                                    //_attackTimer.Stop();
                                    _activeAttacker = false;
                                    _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
                                    Thread.Sleep(1000);
                                    _data.AttackFlag = false;
                                    _activeAttacker = true;
                                    Thread.Sleep(1000);
                                    if (targetMob.Health >= stampMobHealth && !_data.AttackFlag)
                                        stampMobHealthCount++;
                                    stampMob = Environment.TickCount;
                                }
                                else
                                {
                                    if (targetMob.Health < stampMobHealth || stampMobObjId!= targetMob.ObjectId)
                                    {
                                        stampMobHealth = targetMob.Health;
                                        stampMobHealthCount = 0;
                                        stampMob = Environment.TickCount;
                                    }
                                }

                                if (stampMobHealthCount > 3 && targetMob.Health >= stampMobHealth && !_data.AttackFlag)
                                {
                                    _blockedMonsters.Add(targetMob.ObjectId);
                                    stampMobHealthCount = 0;
                                    log.Debug("blocked due to hp stamp check");
                                }

                                //log.Debug(stampMobHealthCount);
                            }
                        }

                    //if (targetMob == null || targetMob.IsDead ||
                    //    _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId) ||
                    //    _blockedMonsters.Contains(targetMob.ObjectId))
                    //{
                    //    if(_followHandler.FollowActivated && _followHandler.Follow())
                    //        Thread.Sleep(100);

                    //    if(CombatTargetType != CombatTargetType.Off)
                    //        targetMob = SuitableTarget;
                    //    moveStampStuckCount = 0;
                    //    moveStamp = new Locatable(_data.MainHero.X, _data.MainHero.Y, _data.MainHero.Z);
                    //    stampMobHealthCount = 0;
                    //    _data.AttackFlag = false;
                    //}
                }
                catch (ThreadAbortException)
                {
                    
                }
                catch (Exception exception)
                {
                    log.Debug(exception);
                }
                Thread.Sleep(30);
            }

        }


        GameFigure targetMob = null;

        private void EngineCycle()
        {
            //GameFigure targetMob = null;
            //_attackTimer = new Timer(100);
            //object attackMutex = new object();
            //_attackTimer.Elapsed += (sender, args) =>
            //{
            //    lock (attackMutex)
            //    {
            //        if (_activeAttacker || !Initialized || _data.MainHero.IsDead || !Running)
            //        {
            //            if(_data.MainHero.Level>0)
            //                DialogHandler.HandleDialogs();
            //            return;
            //        }

            //        _activeAttacker = true;
            //    }

            //    try
            //    {
            //        if (Math.Abs(Environment.TickCount - PartyHealBuffHandler.OutOfPartyStamp) / 1000 >
            //        PartyHealBuffHandler.OutOfPartyTimer && PartyHealBuffHandler.OutOfParty
            //        && _data.PartyMembers.Count > 0 &&
            //        PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) == null &&
            //        !_data.PlayerIsCasting 
            //        && Running)
            //        {
            //            PartyHealBuffHandler.OutOfPartyStamp = Environment.TickCount;
            //            _actionsController.LeaveParty();
            //        }

            //        SelfHealBuffHandler.RemoveUnwantedBuffs();

            //        if (SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null &&
            //            !_data.PlayerIsCasting && _attackTimer.Enabled)
            //        {
            //            //log.Debug("selfhealbuff");
            //            int count = 0;
            //            var rule = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            if (rule != null)
            //            {
            //                if (rule.UseOnPet && _data.MainHero.PlayerSummons.Count > 0 &&
            //                    _data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) > rule.MaxDistance &&
            //                    _data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) < 3000)
            //                {
            //                    _targeter.Stop();
            //                    _activeTargeter = false;

            //                    while (_data.MainHero.RangeTo(_data.MainHero.PlayerSummons.First()) > rule.MaxDistance &&
            //                           count < 200)
            //                    {
            //                        Thread.Sleep(100);
            //                        count++;
            //                    }
            //                }

            //                if (count < 200)
            //                {
            //                    if (rule.RequiresTarget)
            //                    {
            //                        _targeter.Stop();
            //                        _activeTargeter = false;
            //                    }

            //                    SelfHealBuffHandler.Cast(rule);
            //                }
            //            }
            //            if (!_activeTargeter)
            //                _targeter.Start();
            //        }
            //         if (PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null &&
            //                 !_data.PlayerIsCasting && _attackTimer.Enabled)
            //        {
            //            //log.Debug("pt healbuff");
            //            int count = 0;
            //            var rule = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            if (rule != null)
            //            {
            //                double distanceToTarget = _data.MainHero.RangeTo(rule.ChosenTarget);
            //                if (distanceToTarget > rule.MaxDistance &&
            //                    distanceToTarget < 3000)
            //                {
            //                    _targeter.Stop();
            //                    _activeTargeter = false;

            //                    while (_data.MainHero.RangeTo(rule.ChosenTarget) > rule.MaxDistance && count < 200 && Running)
            //                    {
            //                        if (count % 10 == 0)
            //                            _actionsController.MoveToRaw(rule.ChosenTarget.X, rule.ChosenTarget.Y,
            //                                rule.ChosenTarget.Z);

            //                        Thread.Sleep(100);
            //                        count++;
            //                    }
            //                }

            //                if (count < 200 && Running)
            //                {
            //                    _targeter.Stop();
            //                    _activeTargeter = false;
            //                    PartyHealBuffHandler.Cast(rule);
            //                    //Thread.Sleep(_data.Skills[rule.PartyHealBuffId].CastTime);
            //                }
            //            }
            //            if (!_activeTargeter)
            //                _targeter.Start();
            //        }

            //         if (!Running || targetMob == null || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob) == null) || !_attackTimer.Enabled || (CombatTargetType == CombatTargetType.Off && !AssistHandler.AssistActivated) ||
            //                 _data.MainHero.TargetObjectId != targetMob.ObjectId ||
            //                 _nukeHandler.Nuke(targetMob) || _attackHandler.Attack(targetMob))
            //        {
            //            //log.Debug("Attack");
            //            //log.Debug(targetMob==null);
            //            //log.Debug(_activeTargeter);
            //            //log.Debug(_targeter.Enabled);
            //            if (CombatTargetType == CombatTargetType.Off)
            //                _data.AttackFlag = false;

            //            if (!_activeTargeter)
            //                _targeter.Start();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Debug(ex);
            //    }

            //    lock (attackMutex) { _activeAttacker = false; }

            //};
            //_attackTimer.Start();

            //int stampMobObjId = 0;
            //int stampMobHealth = 0;
            //int stampMobHealthCount = 0;
            //long stampMob = 0;
            //Locatable moveStamp = new Locatable(0, 0, 0);
            //int moveStampStuckCount = 0;

            //object targetMutex = new object();
            //_activeTargeter = false;

            //_targeter = new Timer(50);
            //_targeter.Elapsed += (sender, args) =>
            //{
            //    lock (targetMutex)
            //    {
            //        if (_activeTargeter || !Running || _data.MainHero.IsDead || (CombatTargetType == CombatTargetType.Off && !AssistHandler.AssistActivated && !_followHandler.FollowActivated)) return;
            //        _activeTargeter = true;
            //    }

            //    try
            //    {
            //        while ((targetMob == null || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob)==null) 
            //                || (_blockedMonsters.Contains(targetMob.ObjectId) && ClosestAttacker!=targetMob) 
            //               || _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId) 
            //               || SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null 
            //               || PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag) != null
            //               || (_data.PlayerIsCasting)// && _data.AttackFlag == false
            //               || ( AssistHandler.AssistActivated && AssistHandler.ShouldAssist())) 

            //               && _targeter.Enabled && Running)
            //        {
            //            while (_followHandler.FollowActivated && _followHandler.Follow() && !AssistHandler.AssistActivated && _targeter.Enabled && Running)
            //                Thread.Sleep(50);

            //            //log.Debug("No Target");
            //            Thread.Sleep(100);
            //            if (targetMob?.ObjectId != SuitableTarget?.ObjectId)
            //                _data.AttackFlag = false;

            //            if(CombatTargetType != CombatTargetType.Off 
            //            && !AssistHandler.AssistActivated 
            //            && (targetMob == null 
            //            || (targetMob.IsDead && NukeHandler.GetNukeToCast(targetMob) == null)
            //            || _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId)
            //            || (_blockedMonsters.Contains(targetMob.ObjectId) && ClosestAttacker != targetMob))
            //            )
            //                targetMob = SuitableTarget;


            //            var selfnuke = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            var partynuke = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            if ((selfnuke == null || !selfnuke.RequiresTarget)
            //               &&
            //               (partynuke == null) || !partynuke.RequiresTarget)
            //            if (AssistHandler.AssistActivated)
            //            {
            //                AssistHandler.Assist();
            //                if (_data.MainHero.TargetObjectId != 0 && _data.SurroundingMonsters.Any(mob => mob.ObjectId == _data.MainHero.TargetObjectId)
            //                    && AssistHandler.ActiveAssister?.TargetObjectId == _data.MainHero.TargetObjectId)
            //                    targetMob = _data.AllUnits.First(unit => unit.ObjectId == _data.MainHero.TargetObjectId);
            //                else
            //                {
            //                    targetMob = null;
            //                }
            //            }
            //        }

            //        if (!AssistHandler.AssistActivated)
            //        {
            //            if(targetMob!= null && CombatTargetType != CombatTargetType.Off)
            //            if (!_actionsController.TargetByObjectId(targetMob.ObjectId))
            //                _blockedMonsters.Add(targetMob.ObjectId);
            //        }
            //        else
            //        {
            //            var selfnuke = SelfHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            var partynuke = PartyHealBuffHandler.GetRuleForCast(ClosestAttacker != null || _data.AttackFlag);
            //            if ( (selfnuke == null || !selfnuke.RequiresTarget)
            //               && 
            //               (partynuke == null) || !partynuke.RequiresTarget)
            //            AssistHandler.Assist();
            //            if (_data.MainHero.TargetObjectId != 0 && _data.SurroundingMonsters.Any(mob => mob.ObjectId == _data.MainHero.TargetObjectId))
            //                targetMob = _data.AllUnits.First(unit => unit.ObjectId == _data.MainHero.TargetObjectId);
            //            else
            //            {
            //                targetMob = null;
            //            }
            //        }

            //        if (targetMob != null && stampMobObjId != targetMob.ObjectId && targetMob.Health > 0)
            //        {
            //            stampMobObjId = targetMob.ObjectId;
            //            stampMobHealth = targetMob.Health;
            //            stampMob = 0;
            //        }

            //        if(targetMob != null && _data.MainHero.TargetObjectId == targetMob.ObjectId && !targetMob.IsDead)
            //        if (
            //            (_nukeHandler.GetNukeToCast(targetMob) != null && _data.MainHero.RangeTo(targetMob) > _nukeHandler.GetNukeToCast(targetMob)?.MaxDistance + 100 &&
            //             !_data.PlayerIsCasting) ||
            //            (_attackHandler.UseMeleeAttack && targetMob.RangeTo(new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY, _data.MainHero.ValidatedZ)) > _attackHandler.MaximumAttackDistance + 50 && !_data.PlayerIsCasting)
            //            )
            //        {
            //            _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
            //            Thread.Sleep(500);
            //            if (moveStamp.RangeTo(new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY, _data.MainHero.ValidatedZ)) < 10)
            //                moveStampStuckCount++;
            //            else
            //            {
            //                moveStampStuckCount = 0;
            //            }

            //            //log.Debug(moveStamp);
            //            moveStamp = new Locatable(_data.MainHero.ValidatedX, _data.MainHero.ValidatedY, _data.MainHero.ValidatedZ);

            //            if (moveStampStuckCount > 5)
            //            {
            //                _blockedMonsters.Add(targetMob.ObjectId);
            //                moveStampStuckCount = 0;
            //            }

            //            if (ClosestAttacker != null && ClosestAttacker != targetMob
            //                && targetMob.Health == targetMob.MaxHealth
            //                && targetMob.TargetObjectId != _data.MainHero.ObjectId 
            //                && !AssistHandler.AssistActivated
            //                && !_data.AttackFlag)
            //            {
            //                Thread.Sleep(100);
            //                if (ClosestAttacker != null 
            //                && ClosestAttacker != targetMob
            //                && targetMob.Health == targetMob.MaxHealth
            //                && targetMob.TargetObjectId != _data.MainHero.ObjectId
            //                && !AssistHandler.AssistActivated
            //                && !_data.AttackFlag)
            //                        targetMob = ClosestAttacker;
            //            }
            //        }
            //        else if (targetMob.ObjectId == stampMobObjId && !targetMob.IsDead && (_nukeHandler.GetNukeToCast(targetMob) != null || _attackHandler.UseMeleeAttack))
            //        {
            //            moveStampStuckCount = 0;

            //            if (stampMob == 0)
            //                stampMob = Environment.TickCount;

            //            if (Math.Abs(Environment.TickCount - stampMob) > 1000 && !_data.PlayerIsCasting)
            //            {
            //                if (targetMob.Health >= stampMobHealth && !_data.AttackFlag)
            //                {
            //                    stampMobHealthCount++;
            //                    _attackTimer.Stop();
            //                    _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
            //                    Thread.Sleep(500);
            //                    _data.AttackFlag = false;
            //                    _attackTimer.Start();
            //                }
            //                else
            //                {
            //                    stampMobHealth = targetMob.Health;
            //                    stampMobHealthCount = 0;
            //                }

            //                if (stampMobHealthCount > 3)
            //                    _blockedMonsters.Add(targetMob.ObjectId);

            //                //log.Debug(stampMobHealthCount);
            //                stampMob = Environment.TickCount;
            //            }
            //        }

            //        //if (targetMob == null || targetMob.IsDead ||
            //        //    _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId) ||
            //        //    _blockedMonsters.Contains(targetMob.ObjectId))
            //        //{
            //        //    if(_followHandler.FollowActivated && _followHandler.Follow())
            //        //        Thread.Sleep(100);

            //        //    if(CombatTargetType != CombatTargetType.Off)
            //        //        targetMob = SuitableTarget;
            //        //    moveStampStuckCount = 0;
            //        //    moveStamp = new Locatable(_data.MainHero.X, _data.MainHero.Y, _data.MainHero.Z);
            //        //    stampMobHealthCount = 0;
            //        //    _data.AttackFlag = false;
            //        //}
            //    }
            //    catch (Exception exception)
            //    {
            //        log.Debug(exception);
            //    }
            //    finally
            //    {
            //        lock (targetMutex) { _activeTargeter = false; }
            //    }

            //};

            //_targeter.Disposed += (sender, args) =>
            //{
            //    _activeTargeter = false;
            //};

            //_targeter.Start();




            //while (Running)
            //{
            //    Thread.Sleep(50);

            //    while (targetMob == null || targetMob.IsDead || _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId))
            //    {
            //        //log.Debug("No Target");
            //        Thread.Sleep(100);
            //        targetMob = SuitableTarget;
            //    }

            //    _actionsController.TargetByObjectId(targetMob.ObjectId);

            //    if (stampMobObjId != targetMob.ObjectId && targetMob.Health>0)
            //    {
            //        stampMobObjId = targetMob.ObjectId;
            //        stampMobHealth = targetMob.Health;
            //        stampMob = 0;
            //    }

            //    if ((_nukeHandler.GetNukeToCast(targetMob) != null && _data.MainHero.RangeTo(targetMob) > _nukeHandler.GetNukeToCast(targetMob).MaxDistance  && !_data.PlayerIsCasting())  ||  
            //        (_attackHandler.UseMeleeAttack && _data.MainHero.RangeTo(targetMob) > _attackHandler.MaximumAttackDistance + 100) && !_data.PlayerIsCasting())
            //    {
            //        _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
            //        Thread.Sleep(500);
            //        if (_data.MainHero.RangeTo(moveStamp) < 10)
            //            moveStampStuckCount++;
            //        else
            //        {
            //            moveStampStuckCount = 0;
            //        }

            //        moveStamp = new Locatable(_data.MainHero.X, _data.MainHero.Y, _data.MainHero.Z);

            //        if(moveStampStuckCount>6)
            //            _blockedMonsters.Add(targetMob.ObjectId);

            //        if (ClosestAttacker!=null && ClosestAttacker != targetMob && targetMob.TargetObjectId != _data.MainHero.ObjectId)
            //            targetMob = ClosestAttacker;
            //    }
            //    else if(targetMob.ObjectId == stampMobObjId)
            //    {
            //        //moveStampStuckCount = 0;

            //        if (stampMob == 0)
            //            stampMob = Environment.TickCount;

            //        if (Math.Abs(Environment.TickCount - stampMob) > 1000 && !_data.PlayerIsCasting())
            //        {
            //            if (targetMob.Health >= stampMobHealth && !_data.AttackFlag)
            //            {
            //                stampMobHealthCount++;
            //                _attackTimer.Stop();
            //                _actionsController.MoveToRaw(targetMob.X, targetMob.Y, targetMob.Z);
            //                Thread.Sleep(500);
            //                _data.AttackFlag = false;
            //                _attackTimer.Start();
            //            }
            //            else
            //            {
            //                stampMobHealth = targetMob.Health;
            //                stampMobHealthCount = 0;
            //            }

            //            if(stampMobHealthCount>3)
            //                _blockedMonsters.Add(targetMob.ObjectId);

            //            stampMob = Environment.TickCount;
            //        }
            //    }

            //    if (targetMob==null || targetMob.IsDead || _data.SurroundingMonsters.All(mob => mob.ObjectId != targetMob.ObjectId) ||
            //        _blockedMonsters.Contains(targetMob.ObjectId))
            //    {
            //        targetMob = SuitableTarget;
            //        moveStampStuckCount = 0;
            //        moveStamp = new Locatable(_data.MainHero.X, _data.MainHero.Y, _data.MainHero.Z);
            //        stampMobHealthCount = 0;
            //        _data.AttackFlag = false;
            //    }
            //}

            _targeterThread = new Thread(Targeter);
            _targeterThread.Start();

            _attackThread = new Thread(Attacker);
            _attackThread.Start();

            while (Running)
            {
                Thread.Sleep(100);
            }

            _attackThread.Abort();
            _targeterThread.Abort();
        }

        private Npc SuitableTarget
        {
            get
            {
                //return _data.Npcs.First(mob => mob.Value.IsMonster).Value;
                if (CombatTargetType == CombatTargetType.Off)
                    return null;

                int min = Range;
                Npc npc = ClosestAttacker;
                Locatable point = CombatTargetType == CombatTargetType.AroundPoint
                    ? new Locatable(PointX, PointY, PointZ)
                    : _data.MainHero;

                if (npc == null)
                    foreach (var mob in _data.SurroundingMonsters)
                    {
                        int zrange = Math.Abs(mob.Z - point.Z);
                        double range = mob.RangeTo(point);
                        if (!mob.IsDead && mob.RangeTo(point) < Range && mob.RangeTo(_data.MainHero) < min && !_blockedMonsters.Contains(mob.ObjectId)
                                && !_data.Players.Any(player => !player.Value.IsMyPartyMember && player.Key == mob.TargetObjectId)
                                && (_data.MainHero.PlayerSummons.Count == 0 || _data.MainHero.PlayerSummons.All(summ => summ.ObjectId != mob.ObjectId))
                                && Math.Abs(mob.Z - _data.MainHero.Z) < 400 &&
                                (!IgnoreMonsters || !MonsterFilter.Any(moba => moba.Enable && mob.Name.Equals(moba.Name, StringComparison.OrdinalIgnoreCase)))
                                )
                        {
                            min = (int)mob.RangeTo(_data.MainHero);
                            npc = mob;
                        }
                    }

                if (npc == null)
                    foreach (var mob in _data.SurroundingMonsters)
                    {
                        int zrange = Math.Abs(mob.Z - point.Z);
                        double range = mob.RangeTo(point);
                        if (!mob.IsDead && mob.RangeTo(point) < Range && mob.RangeTo(_data.MainHero) < min
                                && !_data.Players.Any(player => !player.Value.IsMyPartyMember && player.Key == mob.TargetObjectId)
                                && (_data.MainHero.PlayerSummons.Count == 0 || _data.MainHero.PlayerSummons.All(summ => summ.ObjectId != mob.ObjectId))
                                && Math.Abs(mob.Z - _data.MainHero.Z) < 400 &&
                                (!IgnoreMonsters || !MonsterFilter.Any(moba => moba.Enable && mob.Name.Equals(moba.Name, StringComparison.OrdinalIgnoreCase)))
                                )
                        {
                            min = (int)mob.RangeTo(_data.MainHero);
                            npc = mob;
                        }
                    }

                if (npc == null)
                    foreach (var mob in _data.SurroundingMonstersIncludingInvalidEntries)
                    {
                        int zrange = Math.Abs(mob.Z - point.Z);
                        double range = mob.RangeTo(point);
                        if (!mob.IsDead && mob.RangeTo(point) < Range && mob.RangeTo(_data.MainHero) < min && !_blockedMonsters.Contains(mob.ObjectId)
                                && !_data.Players.Any(player => !player.Value.IsMyPartyMember && player.Key == mob.TargetObjectId)
                                && (_data.MainHero.PlayerSummons.Count == 0 || _data.MainHero.PlayerSummons.All(summ => summ.ObjectId != mob.ObjectId))
                                && Math.Abs(mob.Z - _data.MainHero.Z) < 400 &&
                                (!IgnoreMonsters || !MonsterFilter.Any(moba => moba.Enable && mob.Name.Equals(moba.Name, StringComparison.OrdinalIgnoreCase)))
                                )
                        {
                            min = (int)mob.RangeTo(_data.MainHero);
                            npc = mob;
                        }
                    }

                return npc;
            }
        }

        private Npc _closestAttackerCache;
        private long _closestAttackerStamp;

        private Npc ClosestAttacker
        {
            get
            {
                if (Math.Abs(Environment.TickCount - _closestAttackerStamp) < 15 && false)
                    return _closestAttackerCache;

                int min = int.MaxValue;
                Npc npc = null;

                npc = MobForSweep;

                if (npc == null)
                    foreach (var mob in _data.SurroundingMonsters)
                    {
                        if (!mob.IsDead && mob.RangeTo(_data.MainHero) < min &&
                            (mob.TargetObjectId == _data.MainHero.ObjectId)
                            //|| _data.PartyMembers.Any(ptmember => ptmember.ObjectId == mob.TargetObjectId))
                            )
                        {
                            min = (int)mob.RangeTo(_data.MainHero);
                            npc = mob;
                        }
                    }
                
                _closestAttackerCache = npc;
                _closestAttackerStamp = Environment.TickCount;
                return npc;
            }
        }

        private Npc MobForSweep
        {
            get
            {
                int min = Range;
                Npc npc = null;

                foreach (var mob in _data.SurroundingMonsters)
                {
                    if (mob.IsDead && mob.RangeTo(_data.MainHero) < min
                        && mob.IsSweepable
                        && (_data.MonstersToLoot.Contains(mob.ObjectId) || mob.RangeTo(_data.MainHero) < 500)
                        && NukeHandler.NukesToUse.Any(nuke => nuke.Enabled && nuke.TargetIsSpoiled))
                    {
                        min = (int)mob.RangeTo(_data.MainHero);
                        npc = mob;
                    }
                }

                return npc;
            }
        }

        public bool Running
        {
            get { return _running; }
            set
            {
                _running = value;
                _nukeHandler.EngineRunning = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool Initialized
        {
            get { return _initialized; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
