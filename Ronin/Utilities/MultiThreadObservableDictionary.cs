using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ronin.Utilities
{
    public class MultiThreadObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        public void Remove(TKey key)
        {
            TValue value;//placeholder
            while (this.ContainsKey(key) && !this.TryRemove(key, out value))
            {
                Thread.Sleep(10);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, key, 0));
        }

        public void Add(TKey key, TValue value)
        {
            while (!this.ContainsKey(key) && !this.TryAdd(key, value))
            {
                Thread.Sleep(10);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, key, 0));
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, key, 0));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
    }
}
