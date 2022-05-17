using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace GitTank.CustomCollections
{
    public class DispatcherObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
            {
                foreach (var @delegate in collectionChanged.GetInvocationList())
                {
                    var handler = (NotifyCollectionChangedEventHandler)@delegate;
                    if (handler.Target is DispatcherObject dispatcherObject)
                    {
                        var dispatcher = dispatcherObject.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            dispatcher.BeginInvoke(
                                (Action)(() => handler.Invoke(
                                    this,
                                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }

                    handler.Invoke(this, e);
                }
            }
        }
    }
}
