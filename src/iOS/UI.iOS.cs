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
        public static string AppId { get; set; }
        
        public virtual Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            var syncContext = SynchronizationContext.Current;
            var tcs = new TaskCompletionSource<bool>();
            var view = new UIAlertView(title, message, null, no, yes) { TintColor = UIWindow.Appearance.TintColor };

            cancellation.Register(() => syncContext.Post(_ => view.DismissWithClickedButtonIndex(view.CancelButtonIndex, true), null));

            view.Dismissed += (sender, e) => tcs.TrySetResult(e.ButtonIndex != view.CancelButtonIndex);
            view.Clicked += (sender, e) => tcs.TrySetResult(e.ButtonIndex != view.CancelButtonIndex);
            view.Show();

            return tcs.Task;
        }

        public virtual void Toast(string title, string message)
        {
            Notify(title, message, CancellationToken.None);
        }

        public virtual Task<bool> Notify(string title, string message, CancellationToken cancellation)
        {
            return Task.FromResult(false);
        }

        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            var owner = UIApplication.SharedApplication.KeyWindow;
            if (owner == null) return null;

            var buttons = items.ToArray();
            var syncContext = SynchronizationContext.Current;
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

            cancellation.Register(() => syncContext.Post(_ => view.DismissWithClickedButtonIndex(view.CancelButtonIndex, true), null));

            view.Clicked += (sender, e) => { tcs.TrySetResult((int)e.ButtonIndex); };
            view.ShowInView(owner);
            return tcs.Task;
        }

        public virtual Task<string> Input(string title, string defaultText, string yes, bool password, CancellationToken cancellation)
        {
            var syncContext = SynchronizationContext.Current;
            var tcs = new TaskCompletionSource<string>();
            var view = new UIAlertView(title, "", null, yes) { TintColor = UIWindow.Appearance.TintColor };
            view.AlertViewStyle = password ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput;

            var textField = view.GetTextField(0);
            textField.Text = defaultText;
            view.Dismissed += (sender, e) => tcs.TrySetResult(null);
            view.Clicked += (sender, e) => tcs.TrySetResult(view.GetTextField(0).Text);

            cancellation.Register(() => syncContext.Post(_ => view.DismissWithClickedButtonIndex(view.CancelButtonIndex, true), null));

            view.Show();

            return tcs.Task;
        }

        public virtual void RateMe()
        {
            if (!string.IsNullOrEmpty(AppId))
            {
                // https://github.com/nicklockwood/iRate/blob/master/iRate/iRate.m
                UIApplication.SharedApplication.OpenUrl(new NSUrl($"itms-apps://itunes.apple.com/app/id{AppId}"));
            }
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
    }
}
