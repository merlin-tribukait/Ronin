using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ronin.Data;
using Ronin.Data.Structures;
using Ronin.Utilities;

namespace Ronin.Logic.Handlers
{
    public class UIItemAdd
    {
        private bool _enable;
        private int _id;
        private string _name;

        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }

    public class PickupHandler : LogicHandler
    {
        private int _range = 2000;
        
        public int Range
        {
            get { return _range; }
            set { _range = value;
                OnPropertyChanged();
            }
        }

        private bool _pickupMine = true;

        public bool PickupMine
        {
            get { return _pickupMine; }
            set { _pickupMine = value;
                OnPropertyChanged();
            }
        }

        private bool _dontPickupHerbs;

        public bool DontPickupHerbs
        {
            get { return _dontPickupHerbs; }
            set { _dontPickupHerbs = value;
                OnPropertyChanged();
            }
        }

        private bool _pickupAll;

        public bool PickupAll
        {
            get { return _pickupAll; }
            set
            {
                _pickupAll = value; 
                OnPropertyChanged();
                if (value)
                {
                    PickupInclusive = false;
                    PickupExclusive = false;
                }
            }
        }

        private bool _pickupInclusive;

        public bool PickupInclusive
        {
            get { return _pickupInclusive; }
            set { _pickupInclusive = value;
                OnPropertyChanged();
                if (value)
                {
                    PickupAll = false;
                    PickupExclusive = false;
                }
            }
        }

        private bool _pickupExclusive;

        public bool PickupExclusive
        {
            get { return _pickupExclusive; }
            set { _pickupExclusive = value;
                OnPropertyChanged();
                if (value)
                {
                    PickupInclusive = false;
                    PickupAll = false;
                }
            }
        }

        private MultiThreadObservableCollection<PickupRule> _rulesInUse = new MultiThreadObservableCollection<PickupRule>();

        public MultiThreadObservableCollection<PickupRule> RulesInUse
        {
            get { return _rulesInUse; }
            set { _rulesInUse = value; }
        }

        private PickupRule _selectedPickupRule;

        [JsonIgnore]
        public PickupRule SelectedPickupRule
        {
            get { return _selectedPickupRule; }
            set { _selectedPickupRule = value;
                OnPropertyChanged();
            }
        }

        private string _itemFilter;

        [JsonIgnore]
        public string ItemFilter
        {
            get { return _itemFilter; }
            set { _itemFilter = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public MultiThreadObservableCollection<UIItemAdd> FilteredItems
        {
            get
            {
                var list = new MultiThreadObservableCollection<UIItemAdd>();
                foreach (var itemKeyPair in ExportedData.ItemIdToName)
                {
                    if (!String.IsNullOrEmpty(ItemFilter) && itemKeyPair.Value.ToLower().Contains(ItemFilter.ToLower()))
                    {
                        list.Add(new UIItemAdd() {Enable = false, Id = itemKeyPair.Key, Name = itemKeyPair.Value});
                    }
                }

                return list;
            }
        }

        public bool ShouldPickup()
        {
            DroppedItem itemForPickup = null;
            int minDistance = Range;

            foreach (var droppedItem in _data.DroppedItems)
            {
                if (_data.MainHero.RangeTo(droppedItem.Value) < minDistance
                    && (!PickupMine || _data.MonstersToLoot.Contains(droppedItem.Value.SourceMobObjectId))
                    && (!_blockedDrop.ContainsKey(droppedItem.Key) || DateTime.Now.Subtract(_blockedDrop[droppedItem.Key]).TotalSeconds > 30)
                    && (PickupAll
                        || (PickupInclusive && RulesInUse.Any(rule => rule.Enable && rule.ItemId == droppedItem.Value.ItemId && rule.ConditionsAreMet(_data, droppedItem.Value)))
                        || (PickupExclusive && !RulesInUse.Any(rule => rule.Enable && rule.ItemId == droppedItem.Value.ItemId && rule.ConditionsAreMet(_data, droppedItem.Value))))
                )
                {
                    //itemForPickup = droppedItem.Value;
                    //minDistance = (int) _data.MainHero.RangeTo(itemForPickup);
                    return true;
                }
            }

            return false;
        }

        private DateTime _moveToStamp = DateTime.MinValue;

        private Dictionary<int, DateTime> _blockedDrop = new Dictionary<int, DateTime>();

        public void Pickup()
        {
            DroppedItem itemForPickup = null;
            int minDistance = Range;

            foreach (var droppedItem in _data.DroppedItems)
            {
                if (_data.MainHero.RangeTo(droppedItem.Value) < minDistance
                    && (!PickupMine || _data.MonstersToLoot.Contains(droppedItem.Value.SourceMobObjectId))
                    && (!_blockedDrop.ContainsKey(droppedItem.Key) || DateTime.Now.Subtract(_blockedDrop[droppedItem.Key]).TotalSeconds > 30)
                    && (PickupAll
                        || (PickupInclusive && RulesInUse.Any(rule => rule.Enable && rule.ItemId == droppedItem.Value.ItemId && rule.ConditionsAreMet(_data, droppedItem.Value)))
                        || (PickupExclusive && !RulesInUse.Any(rule => rule.Enable && rule.ItemId == droppedItem.Value.ItemId && rule.ConditionsAreMet(_data, droppedItem.Value))))
                )
                {
                    itemForPickup = droppedItem.Value;
                    minDistance = (int)_data.MainHero.RangeTo(itemForPickup);
                }
            }
            
            if(itemForPickup == null)
                return;

            if (minDistance > 150)
            {
                if (DateTime.Now.Subtract(_moveToStamp).TotalMilliseconds > 500)
                {
                    _actionsController.MoveToRaw(itemForPickup.X, itemForPickup.Y, itemForPickup.Z);
                    _moveToStamp = DateTime.Now;
                }
            }
            else
            {
                _actionsController.Pickup(itemForPickup.ObjectId);
                int timeout = 0;
                while (timeout < 30 && _data.DroppedItems.ContainsKey(itemForPickup.ObjectId))
                {
                    Thread.Sleep(100);

                    if(timeout % 10 == 0)
                        _actionsController.Pickup(itemForPickup.ObjectId);
                }

                if (timeout == 30)
                {
                    if(_blockedDrop.ContainsKey(itemForPickup.ObjectId))
                        _blockedDrop[itemForPickup.ObjectId] = DateTime.Now;
                    else
                        _blockedDrop.Add(itemForPickup.ObjectId, DateTime.Now);
                    
                }
            }
        }
    }
}
