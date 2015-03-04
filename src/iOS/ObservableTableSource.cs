namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Foundation;
    using UIKit;

    public class ObservableTableSource<T> : UITableViewSource
    {
        private readonly string cellIdentifier = "TableCell";

        private UITableView view;
        private IList<T> collection;
        private Dictionary<UITableViewCell, T> initializedViews = new Dictionary<UITableViewCell, T>();

        public Func<T, string, UITableViewCell> CreateView;
        public Action<T, UITableViewCell> Visualize;
        public Func<T, string> GetGroupName;

        public ObservableTableSource(UITableView view, IList<T> items)
        {
            this.view = view;
            this.collection = items;

            ((INotifyCollectionChanged)items).CollectionChanged += OnCollectionChanged;

            this.view.ReloadData();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                view.ReloadData();
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var count = e.NewItems.Count;
                var paths = new NSIndexPath[count];

                for (var i = 0; i < count; i++)
                {
                    paths[i] = NSIndexPath.FromRowSection(e.NewStartingIndex + i, 0);
                }

                view.InsertRows(paths, UITableViewRowAnimation.Automatic);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var count = e.OldItems.Count;
                var paths = new NSIndexPath[count];

                for (var i = 0; i < count; i++)
                {
                    paths[i] = NSIndexPath.FromRowSection(e.OldStartingIndex + i, 0);
                }

                view.DeleteRows(paths, UITableViewRowAnimation.Automatic);
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var index = collection.IndexOf((T)sender);
            if (index >= 0)
            {
                view.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Automatic);
            }
        }
        
        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return collection.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var value = collection[indexPath.Row];
            var cell = tableView.DequeueReusableCell(cellIdentifier) ?? CreateView(value, cellIdentifier);
            Visualize(value, cell);

            T oldItem;
            initializedViews.TryGetValue(cell, out oldItem);

            if (!Equals(oldItem, value))
            {
                var oldObservable = oldItem as INotifyPropertyChanged;
                if (oldObservable != null)
                {
                    oldObservable.PropertyChanged -= this.OnItemPropertyChanged;
                }
                var observable = value as INotifyPropertyChanged;
                if (observable != null)
                {
                    observable.PropertyChanged += this.OnItemPropertyChanged;
                }

                initializedViews[cell] = value;
            }

            return cell;
        }
    }
}