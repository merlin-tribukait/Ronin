using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin
{
    public class NpcAround
    {
        private int _objId;
        private int _unitId;
        private string _name;
        private double _range;
        private int _target;
        private int _visibility;
        private long _updated;

        public NpcAround(int objId, int unitId, string name, double range, int target, int visibility, long updated)
        {
            _objId = objId;
            _unitId = unitId;
            _name = name;
            _range = range;
            _target = target;
            _visibility = visibility;
            _updated = updated;
        }

        public int ObjectId
        {
            get { return _objId; }
            set { _objId = value; }
        }

        public int UnitId
        {
            get { return _unitId; }
            set { _unitId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public double Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public int TargetObjectId
        {
            get { return _target; }
            set { _target = value; }
        }

        public int Visibility
        {
            get { return _visibility; }
            set { _visibility = value; }
        }

        public long LastUpdate
        {
            get { return _updated; }
            set { _updated = value; }
        }

        public string Target
        {
            get
            {
                if (MainWindow.ViewModel.SelectedBot == null)
                    return "";

                var botdata = MainWindow.ViewModel.SelectedBot.PlayerData;

                StringBuilder sb = new StringBuilder();

                if (TargetObjectId == 0)
                    sb.Append("NULL ");
                else if (TargetObjectId == botdata.MainHero.ObjectId)
                    sb.Append("MainHero ");
                else if (botdata.Players.Any(player => player.Key == TargetObjectId))
                    sb.Append("Player ");
                else if (botdata.Npcs.Any(player => player.Key == TargetObjectId))
                    sb.Append("Npc ");

                sb.Append($"({TargetObjectId})");

                return sb.ToString();
            }
        }
    }
    /// <summary>
    /// Interaction logic for DynamicNpcsAround.xaml
    /// </summary>
    public partial class DynamicNpcsAround
    {
        private readonly MultiThreadObservableCollection<NpcAround> _list = new MultiThreadObservableCollection<NpcAround>();

        public DynamicNpcsAround(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
            lb.ItemsSource = MonsterAroundDynamic;

            var updateTimer = new Timer(500);
            updateTimer.Elapsed += (sender, args) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MonsterAroundDynamic.Update();
                });
            };
            updateTimer.Start();

            this.Closed += (sender, args) =>
            {
                updateTimer.Stop();
            };
        }

        public MultiThreadObservableCollection<NpcAround> MonsterAroundDynamic
        {
            get
            {
                var curMonstersAround = ((ViewModel) this.DataContext).SelectedBot.PlayerData.Npcs;
                //clean those not present anymore
                foreach (var npcAround in _list.ToList())
                {
                    if (
                        curMonstersAround.All(
                            mob => mob.Value.ObjectId != npcAround.ObjectId))
                        _list.Remove(npcAround);
                }

                //add the new ones
                foreach (var npcAround in curMonstersAround)
                {
                    if (
                        _list.All(
                            mob => mob.ObjectId != npcAround.Value.ObjectId))
                        _list.Add(new NpcAround(npcAround.Value.ObjectId, npcAround.Value.UnitId, npcAround.Value.Name,
                            npcAround.Value.RangeTo(((ViewModel)this.DataContext).SelectedBot.PlayerData.MainHero), npcAround.Value.TargetObjectId, npcAround.Value.IsInvisible,
                            (Environment.TickCount - npcAround.Value.AddStamp)/1));
                }

                //update the existing ones
                foreach (var npcAround in _list.ToList())
                {
                    var instance = curMonstersAround.FirstOrDefault(mob => mob.Value.ObjectId == npcAround.ObjectId);
                    if( instance.Key==0)
                        continue;

                    if (instance.Value.IsMoving || instance.Value.IsFollowing || instance.Value.TargetObjectId != 0 ||
                        ((ViewModel) this.DataContext).SelectedBot.PlayerData.MainHero.IsMoving ||
                        ((ViewModel) this.DataContext).SelectedBot.PlayerData.MainHero.IsFollowing)
                    {
                        npcAround.Range = instance.Value.RangeTo(((ViewModel) this.DataContext).SelectedBot.PlayerData.MainHero);
                        npcAround.TargetObjectId = instance.Value.TargetObjectId;
                        npcAround.Visibility = instance.Value.IsInvisible;
                        npcAround.LastUpdate = (Environment.TickCount - instance.Value.AddStamp) /1
                                               ;
                    }
                }

                //sort
                var sortedCol = new MultiThreadObservableCollection<NpcAround>(_list.OrderBy(mob => mob.Range));

                _list.Clear();
                foreach (var npcAround in sortedCol)
                {
                    _list.Add(npcAround);
                }

                return _list;
            }
        }
    }
}
