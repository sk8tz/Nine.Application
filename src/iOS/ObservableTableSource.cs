namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Threading;
    using Foundation;
    using UIKit;

    public class ObservableTableSource<T> : UITableViewSource where T : class
    {
        private readonly bool _dynamicRowHeight;
        private readonly UITableView _table;
        private readonly IReadOnlyList<T> _items;
        private readonly string _cellIdentifier;
        private readonly INotifyCollectionChanged _incc;
        private readonly List<INotifyPropertyChanged> _inpcs = new List<INotifyPropertyChanged>();

        private readonly Action<UITableViewCell, T> _prepareView;
        private readonly Action _reachedBottom;
        private readonly Action<UITableViewCell, T> _rowSelected;

        private readonly Func<T, float> _estimateHeight;

        private UITableViewCell _offscreenCell;
        private int _lastSeenCount;

        public T this[int index] => _items[index];

        public ObservableTableSource(
            UITableView table,
            string cellIdentifier,
            IReadOnlyList<T> items,
            Action<UITableViewCell, T> prepareView,
            Action<UITableViewCell, T> rowSelected = null,
            Action reachedBottom = null,
            Func<T, float> estimateHeight = null,
            bool dynamicRowHeight = false)
        {
            _items = items;
            _table = table;
            _cellIdentifier = cellIdentifier;
            _prepareView = prepareView;
            _rowSelected = rowSelected;
            _reachedBottom = reachedBottom;
            _dynamicRowHeight = dynamicRowHeight;
            _estimateHeight = estimateHeight;
            _incc = items as INotifyCollectionChanged;

            if (_incc != null)
                _incc.CollectionChanged += OnCollectionChanged;

            foreach (var item in _items)
            {
                var inpc = item as INotifyPropertyChanged;
                if (inpc != null)
                {
                    inpc.PropertyChanged += OnItemChanged;
                    _inpcs.Add(inpc);
                }
            }

            _table.ReloadData();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count == 1 && _lastSeenCount == _items.Count - 1)
            {
                var inpc = e.NewItems[0] as INotifyPropertyChanged;
                if (inpc != null)
                {
                    inpc.PropertyChanged += OnItemChanged;
                    _inpcs.Add(inpc);
                }
                _table.InsertRows(new [] { NSIndexPath.FromRowSection(e.NewStartingIndex, 0) }, UITableViewRowAnimation.Automatic);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count == 1 && _lastSeenCount == _items.Count + 1)
            {
                var inpc = e.OldItems[0] as INotifyPropertyChanged;
                if (_inpcs.Remove(inpc))
                {
                    inpc.PropertyChanged -= OnItemChanged;
                }
                _table.DeleteRows(new [] { NSIndexPath.FromRowSection(e.OldStartingIndex, 0) }, UITableViewRowAnimation.Automatic);
            }
            else
            {
                foreach (var inpc in _inpcs)
                {
                    inpc.PropertyChanged -= OnItemChanged;
                }
                _inpcs.Clear();

                foreach (var item in _items)
                {
                    var inpc = item as INotifyPropertyChanged;
                    if (inpc != null)
                    {
                        inpc.PropertyChanged += OnItemChanged;
                        _inpcs.Add(inpc);
                    }
                }

                _table.ReloadData();
            }
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            foreach (var i in _table.IndexPathsForVisibleRows)
            {
                if (_items[i.Row] == sender)
                {
                    if (_lastSeenCount == _items.Count)
                    {
                        _table.ReloadRows(new[]{ i }, UITableViewRowAnimation.None);
                    }
                    else
                    {
                        _table.ReloadData();
                    }
                    break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_incc != null)
                _incc.CollectionChanged -= OnCollectionChanged;

            foreach (var inpc in _inpcs)
            {
                inpc.PropertyChanged -= OnItemChanged;
            }
            _inpcs.Clear();

            _table.ReloadData();
        }

        public override nint NumberOfSections(UITableView tableView) => 1;

        public override nint RowsInSection(UITableView tableview, nint section) => _lastSeenCount = _items.Count;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var visibleThreshold = 5;
            
            if (indexPath.Row >= _items.Count - visibleThreshold)
            {
                _reachedBottom?.Invoke();
            }

            var item = _items[indexPath.Row];
            var view = tableView.DequeueReusableCell(_cellIdentifier);

            _prepareView?.Invoke(view, item);

            return view;
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            if (_estimateHeight != null)
            {
                return _estimateHeight(_items[indexPath.Row]);
            }
            return UITableView.AutomaticDimension;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (!_dynamicRowHeight)
            {
                return _table.RowHeight;
            }

            if (_offscreenCell == null)
            {
                _offscreenCell = _table.DequeueReusableCell(_cellIdentifier);
            }

            var cell = _offscreenCell;

            _prepareView?.Invoke(cell, _items[indexPath.Row]);

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            cell.Bounds = new CoreGraphics.CGRect(0, 0, _table.Bounds.Width, _table.Bounds.Height);

            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            var height = cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
            height += 1;

            return height;
        }

        public override bool ShouldShowMenu(UITableView tableView, NSIndexPath rowAtindexPath) => true;

        public override bool CanPerformAction(UITableView tableView, ObjCRuntime.Selector action, NSIndexPath indexPath, NSObject sender) => false;

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            _rowSelected?.Invoke(GetCell(tableView, indexPath), _items[indexPath.Row]);
        }
    }
}
