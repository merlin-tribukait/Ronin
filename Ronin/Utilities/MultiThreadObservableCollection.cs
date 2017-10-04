using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ronin.Utilities
{
    public class MultiThreadObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            try
            {
                if (CollectionChanged != null)
                    foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                    {
                        DispatcherObject dispObj = nh.Target as DispatcherObject;
                        if (dispObj != null)
                        {
                            Dispatcher dispatcher = dispObj.Dispatcher;
                            if (dispatcher != null && !dispatcher.CheckAccess())
                            {
                                dispatcher.BeginInvoke(
                                    (Action)(() => nh.Invoke(this,
                                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                    DispatcherPriority.DataBind);
                                continue;
                            }
                        }
                        nh.Invoke(this, e);
                    }
            }
            catch (Exception exception)
            {
                log.Debug(exception);
            }

        }

        public MultiThreadObservableCollection() : base()
        {
            
        }

        public MultiThreadObservableCollection(IOrderedEnumerable<T> col) : base(col)
        {
        }

        public void Update()
        {
            NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            try
            {
                if (CollectionChanged != null)
                    foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                    {
                        DispatcherObject dispObj = nh.Target as DispatcherObject;
                        if (dispObj != null)
                        {
                            Dispatcher dispatcher = dispObj.Dispatcher;
                            if (dispatcher != null && !dispatcher.CheckAccess())
                            {
                                dispatcher.BeginInvoke(
                                    (Action)(() => nh.Invoke(this,
                                       new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                    DispatcherPriority.DataBind);
                                continue;
                            }
                        }
                        nh.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
            }
            catch (Exception)
            {

            }
        }
    }
}
