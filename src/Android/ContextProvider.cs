namespace Nine.Application
{
    using System;
    using Android.Content;

    public static class ContextProvider
    {
        public static Func<Context> Current { get; set; }
    }
}
