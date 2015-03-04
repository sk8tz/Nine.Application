namespace Nine.Application
{
    using Android.Content;
    using Android.Provider;

    public static class ActivityContext
    {
        private static Context current;

        public static Context Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    Activity = value as BaseActivity;
                    AppUI.ClearNotifications();
                }
            }
        }

        public static BaseActivity Activity { get; private set; }
    }
}
