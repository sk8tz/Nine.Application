namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using ObjCRuntime;
    using CoreGraphics;
    using UIKit;
    using Foundation;

    public partial class AppUI : IAppUI
    {
        public static string AppId { get; set; }

        private UILocalNotification _lastNotification;
        private UIUserNotificationSettings _notificationSettings;

        private readonly Func<UIViewController> _viewController;
        private readonly Queue<Action> _toasts = new Queue<Action>();

        public AppUI() { }
        public AppUI(Func<UIViewController> viewController) { _viewController = viewController; }

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
            // https://github.com/scalessec/Toast/blob/master/Toast/Toast/UIView%2BToast.m
            var controller = _viewController?.Invoke() ?? ApplicationView.ViewController;
            if (controller == null)
            {
                controller = new UIViewController();
                controller.View = new UIView();
                UIApplication.SharedApplication.KeyWindow.RootViewController = controller;
            }

            var container = controller.View;
            var toast = CreateToastView(container, title, message);
            var show = new Action(() =>
                {
                    toast.Alpha = 0.0f;
                    toast.Center = new CGPoint(container.Frame.Width * 0.5f, container.Frame.Height - toast.Frame.Height * 0.5f - 10.0f);

                    container.AddSubview(toast);

                    var remove = new Action(() =>
                        {
                            toast.RemoveFromSuperview();
                            _toasts.Dequeue();
                            if (_toasts.Count > 0)
                            {
                                _toasts.First()();
                            }
                        });

                    var hide = new Action(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(_toasts.Count > 1 ? 2.5 : 5));
                            UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, () => toast.Alpha = 0, remove);
                        });

                    UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.AllowUserInteraction, () => toast.Alpha = 1.0f, hide);

                });

            _toasts.Enqueue(show);

            if (_toasts.Count == 1)
                show();
        }

        private UIView CreateToastView(UIView container, string title, string message)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
                return null;

            var wrapper = new UIView();
            wrapper.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleBottomMargin;
            wrapper.Layer.CornerRadius = 10.0f;
            wrapper.BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.8f);

            var text = string.Join(": ", new [] { title, message }.Where(str => !string.IsNullOrEmpty(str)));

            var label = new UILabel();
            label.Lines = 2;
            label.Font = UIFont.SystemFontOfSize(12.0f);
            label.TextAlignment = UITextAlignment.Left;
            label.LineBreakMode = UILineBreakMode.TailTruncation;
            label.TextColor = UIColor.White;
            label.BackgroundColor = UIColor.Clear;
            label.Alpha = 1.0f;
            label.Text = text;

            // size the title label according to the length of the text
            var maxSizeTitle = new CGSize(container.Bounds.Size.Width * 0.8f, container.Bounds.Size.Height * 0.8f);
            var expectedSizeTitle = label.SizeThatFits(maxSizeTitle);
            // UILabel can return a size larger than the max size when the number of lines is 1
            expectedSizeTitle = new CGSize(Math.Min(maxSizeTitle.Width, expectedSizeTitle.Width), Math.Min(maxSizeTitle.Height, expectedSizeTitle.Height));

            wrapper.Frame = new CGRect(0, 0, expectedSizeTitle.Width + 20.0f, expectedSizeTitle.Height + 20.0f);

            label.Frame = new CGRect(
                (wrapper.Frame.Width - expectedSizeTitle.Width) * 0.5f, 
                (wrapper.Frame.Height - expectedSizeTitle.Height) * 0.5f, 
                expectedSizeTitle.Width, expectedSizeTitle.Height);

            wrapper.AddSubview(label);

            return wrapper;
        }

        public virtual Task<bool> Notify(string title, string message, CancellationToken cancellation)
        {
            var notification = new UILocalNotification();
            notification.AlertTitle = title;
            notification.AlertBody = message;
            notification.SoundName = UILocalNotification.DefaultSoundName;

            if (_notificationSettings == null)
            {
                _notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(_notificationSettings);
            }

            if (_lastNotification != null)
            {
                UIApplication.SharedApplication.CancelLocalNotification(_lastNotification);
            }

            UIApplication.SharedApplication.PresentLocalNotificationNow(notification);

            var syncContext = SynchronizationContext.Current;
            cancellation.Register(() => syncContext.Post(_ => UIApplication.SharedApplication.CancelLocalNotification(notification), null));

            _lastNotification = notification;

            return Task.FromResult(false);
        }

        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            var owner = _viewController?.Invoke()?.View ?? UIApplication.SharedApplication.KeyWindow;
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
