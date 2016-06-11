namespace Nine.Application
{
    using Android.Views;

    public static class DataContextExtensions
    {
        class Wrapper : Java.Lang.Object
        {
            public object Value;
        }

        public static T GetDataContext<T>(this View view) where T: class
        {
            var wrapper = view.Tag as Wrapper;
            return wrapper != null ? wrapper.Value as T : null;
        }

        public static void SetDataContext<T>(this View view, T value)
        {
            var wrapper = view.Tag as Wrapper;
            if (wrapper == null)
            {
                view.Tag = new Wrapper { Value = value };
            }
            else
            {
                wrapper.Value = value;
            }
        }
    }
}
