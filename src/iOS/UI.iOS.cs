namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using ObjCRuntime;
    using UIKit;
    using Foundation;

    public partial class AppUI : IAppUI
    {
        public virtual Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            var tcs = new TaskCompletionSource<bool>();
            var view = new UIAlertView(title, message, null, no, yes) { TintColor = UIWindow.Appearance.TintColor };
            view.Clicked += (sender, e) => { tcs.SetResult(e.ButtonIndex != view.CancelButtonIndex); };
            view.Show();

            return tcs.Task;
        }

        public virtual void Toast(string title, string message)
        {
            Notify(title, message, null, CancellationToken.None);
        }

        public virtual Task<bool> Notify(string title, string message, IDictionary<string, string> args, CancellationToken cancellation)
        {
            return Task.FromResult(false);
        }

        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            var buttons = items.ToArray();
            var owner = ApplicationView.Current.View;
            var tcs = new TaskCompletionSource<int?>();
            var view = new UIActionSheet(title) { TintColor = UIWindow.Appearance.TintColor };

            foreach (var item in buttons)
            {
                view.AddButton(item);
            }

            if (selectedIndex.HasValue)
            {
                view.CancelButtonIndex = selectedIndex.Value;
            }

            view.Clicked += (sender, e) => { tcs.TrySetResult((int)e.ButtonIndex); };
            view.ShowInView(owner);
            return tcs.Task;
        }

        public virtual Task<string> Input(string title, string defaultText, string yes, bool password, CancellationToken cancellation)
        {
            var tcs = new TaskCompletionSource<string>();
            var view = new UIAlertView(title, "", null, yes) { TintColor = UIWindow.Appearance.TintColor };
            view.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            view.GetTextField(0).Text = defaultText;
            view.Clicked += (sender, e) => { tcs.SetResult(view.GetTextField(0).Text); };
            view.Show();

            return tcs.Task;
        }

        public virtual void RateMe()
        {
            // Suggest to use iRate
        }

        public virtual void CopyToClipboard(string text)
        {
            UIPasteboard.General.String = text;
        }

        public virtual Task<Stream> CaptureScreenshot()
        {
            return CaptureScreenshot(null);
        }

        public virtual Task<Stream> CaptureScreenshot(UIView view)
        {
            // http://stackoverflow.com/questions/13284417/how-to-take-a-screenshot-programmatically-in-ios
            using (var context = UIGraphics.GetCurrentContext())
            {
                using (var image = UIGraphics.GetImageFromCurrentImageContext())
                {
                    view.Layer.RenderInContext(context);
                    UIGraphics.EndImageContext();
                    throw new NotImplementedException();
                }
            }
        }

        public virtual void Browse(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }

        private static string CancelText()
        {
            // TODO: from app itself
            var uiKitClass = GetClassForType(typeof(UIButton));
            if (uiKitClass == null) return "Cancel";
            var uikitBundle = NSBundle.FromClass(uiKitClass);
            if (uikitBundle == null) return "Cancel";
            return uikitBundle.LocalizedString("Cancel", null, null) ?? "Cancel";
        }

        private static Class GetClassForType(Type type)
        {
            var handle = Class.GetHandle(type);
            if (handle != IntPtr.Zero)
                return new Class(handle);
            return null;
        }
    }
}
