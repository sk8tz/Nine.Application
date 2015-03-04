namespace Nine.Application
{
    using System.Linq;
    using UIKit;

    public static class ApplicationView
    {
        public static UIViewController Current
        {
            get
            {
                // https://github.com/xamarin/Xamarin.Mobile/blob/master/MonoTouch/Xamarin.Mobile/Media/MediaPicker.cs
                var window = UIApplication.SharedApplication.KeyWindow;
                if (window == null) return null;
                var viewController = window.RootViewController;

                if (viewController == null)
                {
                    window = UIApplication.SharedApplication.Windows.OrderByDescending(w => w.WindowLevel).FirstOrDefault(w => w.RootViewController != null);
                    if (window == null) return null;
                    viewController = window.RootViewController;
                }
                while (viewController.PresentedViewController != null)
                    viewController = viewController.PresentedViewController;
                return viewController;
            }
        }
    }
}
