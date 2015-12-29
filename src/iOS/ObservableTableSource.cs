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
        private nfloat[] _cachedRowHeights = new nfloat[4];

        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        private int _scrolling;

        private UITableViewCell _offscreenCell;

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
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (prepareView == null) throw new ArgumentNullException(nameof(prepareView));

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
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count == 1)
            {
                var inpc = e.NewItems[0] as INotifyPropertyChanged;
                if (inpc != null)
                {
                    inpc.PropertyChanged += OnItemChanged;
                    _inpcs.Add(inpc);
                }
                if (_scrolling > 0)
                {
                    _table.ReloadData();
                }
                else
                {
                    _table.InsertRows(new [] { NSIndexPath.FromRowSection(e.NewStartingIndex, 0)}, UITableViewRowAnimation.Automatic);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count == 1)
            {
                var inpc = e.OldItems[0] as INotifyPropertyChanged;
                if (_inpcs.Remove(inpc))
                {
                    inpc.PropertyChanged -= OnItemChanged;
                }
                if (_scrolling > 0)
                {
                    _table.ReloadData();
                }
                else
                {
                    _table.DeleteRows(new [] { NSIndexPath.FromRowSection(e.OldStartingIndex, 0) }, UITableViewRowAnimation.Automatic);
                }
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
            _syncContext.Post(_ =>
                {
                    var i = GetItemIndex(sender);
                    if (i < 0) return;

                    if (_cachedRowHeights.Length < _items.Count) 
                    {
                        Array.Resize(ref _cachedRowHeights, _items.Count);
                    }

                    var index = NSIndexPath.FromRowSection(i, 0);
                    var cell = _table.CellAt(index);
                    if (cell == null) return;

                    var existingHeight = _cachedRowHeights[i];
                    _prepareView(cell, _items[i]);

                    if (_dynamicRowHeight)
                    {
                        var newHeight = GetHeightForRow(_table, index);
                        if (newHeight != existingHeight)
                        {
                            // http://stackoverflow.com/questions/19374699/is-there-a-way-to-update-the-height-of-a-single-uitableviewcell-without-recalcu
                            _table.BeginUpdates();
                            _table.EndUpdates();
                        }
                    }
                }, null);
        }

        private int GetItemIndex(object sender)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] == sender)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            _scrolling--;
        }
        public override void DecelerationStarted(UIScrollView scrollView)
        {
            _scrolling++;
        }
        public override void DraggingStarted(UIScrollView scrollView)
        {
            _scrolling++;
        }
        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            _scrolling--;
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

        public override nint RowsInSection(UITableView tableview, nint section) =>  _items.Count;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath.Row >= _items.Count - 1)
            {
                _syncContext.Post(_ =>
                    {
                        try
                        {
                            _scrolling++;
                            _reachedBottom?.Invoke();
                        }
                        finally
                        {
                            _scrolling--;
                        }
                    }, null);
            }

            var item = _items[indexPath.Row];
            var cell = tableView.DequeueReusableCell(_cellIdentifier);

            _prepareView.Invoke(cell, item);

            return cell;
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

            if (_cachedRowHeights.Length < _items.Count) 
            {
                Array.Resize(ref _cachedRowHeights, _items.Count);
            }

            if (_offscreenCell == null)
            {
                _offscreenCell = _table.DequeueReusableCell(_cellIdentifier);
            }

            var cell = _offscreenCell;

            _prepareView.Invoke(cell, _items[indexPath.Row]);

            cell.SetNeedsUpdateConstraints();
            cell.UpdateConstraintsIfNeeded();

            cell.Bounds = new CoreGraphics.CGRect(0, 0, _table.Bounds.Width, _table.Bounds.Height);

            cell.SetNeedsLayout();
            cell.LayoutIfNeeded();

            var height = cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
            height += 1;

            return _cachedRowHeights[indexPath.Row] = height;
        }

        public override bool ShouldShowMenu(UITableView tableView, NSIndexPath rowAtindexPath) => true;

        public override bool CanPerformAction(UITableView tableView, ObjCRuntime.Selector action, NSIndexPath indexPath, NSObject sender) => false;

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            _rowSelected?.Invoke(GetCell(tableView, indexPath), _items[indexPath.Row]);
        }
    }
}
