namespace TalkToSomeone
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

#if WINDOWS_PHONE
    using System.Windows;
    using System.Windows.Controls;
#else
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
#endif

    public static class ItemsSource
    {
        public static void Bind<T>(this FrameworkElement itemsPanel, IList<T> items, Action<FrameworkElement> createView, Action<T, FrameworkElement> updateView)
        {
            //itemsPanel.ItemTemplateSelector
        }
    }
}
