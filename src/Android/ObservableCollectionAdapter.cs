namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Android.App;
    using Android.Views;
    using Android.Widget;

    public class ObservableCollectionAdapter<T> : BaseAdapter<T>
    {
        private readonly IReadOnlyList<T> items;
        private readonly int resource;

        public ObservableCollectionAdapter(Activity context, int resource, IReadOnlyList<T> items, Action<View> newView, Action<View, T> prepareView)
        {
            this.Context = context;
            this.resource = resource;
            this.items = items;
            this.prepareView = prepareView;
            this.newView = newView;

            ((INotifyCollectionChanged)items).CollectionChanged += this.OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public override T this[int position]
        {
            get { return this.items[position]; }
        }

        protected Activity Context { get; private set; }

        public override int Count
        {
            get { return this.items.Count; }
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        private Dictionary<View, T> initializedViews = new Dictionary<View, T>();

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView != null)
            {
                T oldItem;
                if (this.initializedViews.TryGetValue(convertView, out oldItem))
                {
                    var oldObservable = oldItem as INotifyPropertyChanged;
                    if (oldObservable != null)
                    {
                        oldObservable.PropertyChanged -= this.OnItemChanged;
                    }
                }
            }

            View view = convertView;
            if (view == null)
            {
                view = this.Context.LayoutInflater.Inflate(resource, parent, false);
                if (this.newView != null)
                {
                    this.newView(view);
                }
            }

            T item = this[position];
            this.initializedViews[view] = item;
            view.SetDataContext(item);
            if (this.prepareView != null)
            {
                this.prepareView(view, item);
            }

            var observable = item as INotifyPropertyChanged;
            if (observable != null)
            {
                observable.PropertyChanged += this.OnItemChanged;
            }

            return view;
        }

        private Action<View> newView;
        private Action<View, T> prepareView;
    }
}
