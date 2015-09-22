namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Android.App;
    using Android.Database;
    using Android.Views;
    using Android.Widget;

    public class ObservableCollectionAdapter<T> : BaseAdapter<T>
    {
        private readonly IReadOnlyList<T> _items;
        private readonly int _resource;
        private readonly INotifyCollectionChanged _incc;

        private readonly Dictionary<View, T> _initializedViews = new Dictionary<View, T>();
        private readonly List<INotifyPropertyChanged> _inpcs = new List<INotifyPropertyChanged>();

        private readonly Activity _context;

        private readonly Action<View> _newView;
        private readonly Action<View, T> _prepareView;

        private int _observeCount;

        public ObservableCollectionAdapter(Activity context, int resource, IReadOnlyList<T> items, Action<View> newView, Action<View, T> prepareView)
        {
            _context = context;
            _resource = resource;
            _items = items;
            _prepareView = prepareView;
            _newView = newView;
            _incc = items as INotifyCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => NotifyDataSetChanged();

        private void OnItemChanged(object sender, EventArgs e) => NotifyDataSetChanged();

        public override T this[int position] => _items[position];

        public override int Count => _items.Count;

        public override long GetItemId(int position) => 0;

        public override void RegisterDataSetObserver(DataSetObserver observer)
        {
            if (_observeCount == 0 && _incc != null) _incc.CollectionChanged += OnCollectionChanged;

            _observeCount++;
        }

        public override void UnregisterDataSetObserver(DataSetObserver observer)
        {
            _observeCount--;

            if (_observeCount == 0)
            {
                if (_incc != null) _incc.CollectionChanged -= OnCollectionChanged;

                foreach (var inpc in _inpcs)
                {
                    inpc.PropertyChanged -= OnItemChanged;
                }

                _inpcs.Clear();
                _initializedViews.Clear();
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView != null)
            {
                T oldItem;
                if (_initializedViews.TryGetValue(convertView, out oldItem))
                {
                    var inpc = oldItem as INotifyPropertyChanged;
                    if (inpc != null) inpc.PropertyChanged -= OnItemChanged;
                }
            }

            View view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(_resource, parent, false);
                _newView?.Invoke(view);
            }

            T item = this[position];
            _initializedViews[view] = item;
            view.SetDataContext(item);
            _prepareView?.Invoke(view, item);

            var observable = item as INotifyPropertyChanged;
            if (observable != null)
            {
                observable.PropertyChanged += OnItemChanged;
                _inpcs.Add(observable);
            }

            return view;
        }
    }
}
