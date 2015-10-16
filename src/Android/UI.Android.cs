namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content;
    using Android.Graphics;
    using Android.Support.V4.App;
    using Android.Text;
    using Android.Views;
    using Android.Widget;
    using Android.Text.Method;

    [Activity]
    class NotificationRedirectActivity : Activity
    {
        protected override void OnStart()
        {
            base.OnStart();
            Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (AppUI._notificationTcs != null &&
                AppUI._notificationTcs.TrySetResult(true))
            {
                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.Cancel(AppUI.NotificationCode);
            }
        }
    }

    public partial class AppUI : IAppUI
    {
        internal const int NotificationCode = 90002;

        private int lastNotificationId;

        private readonly Func<Context> _contextFactory;

        private DateTime _lastNotificationTime;

        internal static TaskCompletionSource<bool> _notificationTcs;

        public int? SmallIcon { get; set; }
        public int? LargeIcon { get; set; }
        
        public AppUI(Context context) : this(() => context) { }
        public AppUI(Func<Context> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            this._contextFactory = contextFactory;
        }

        public virtual Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            var context = _contextFactory();
            if (context == null) return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>();
            var builder = new AlertDialog.Builder(context)
                .SetMessage(message)
                .SetPositiveButton(yes, new ClickListener((e, i) =>
                {
                    if (e != null) e.Cancel();
                    tcs.TrySetResult(true);
                }))
                .SetOnCancelListener(new ClickListener((e, i) =>
                {
                    if (e != null) e.Cancel();
                    tcs.TrySetResult(false);
                }));

            if (no != null)
            {
                builder.SetNegativeButton(no, new ClickListener((e, i) =>
                {
                    if (e != null) e.Cancel();
                    tcs.TrySetResult(false);
                }));
            }

            if (!string.IsNullOrEmpty(title))
            {
                builder.SetTitle(title);
            }

            var dialog = builder.Create();
            dialog.Show();
            cancellation.Register(() =>
            {
                dialog.Dismiss();
                tcs.TrySetResult(false);
            });
            return tcs.Task;
        }

        public virtual void Toast(string title, string message)
        {
            var context = _contextFactory();
            if (context == null) return;
            if (!string.IsNullOrEmpty(title)) message = title + ": " + message;

            Android.Widget.Toast.MakeText(context, message, ToastLength.Short).Show();
        }

        public virtual Task<bool> Notify(string title, string message, CancellationToken cancellation)
        {
            var context = _contextFactory();
            if (context == null) return Task.FromResult(false);

            title = title ?? "";
            message = message ?? "";

            var packageName = context.PackageName;
            var icon = SmallIcon ?? context.Resources.GetIdentifier("icon", "drawable", packageName);
            var ticker = string.IsNullOrEmpty(title) ? message : title + ": " + message;

            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(icon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetTicker(ticker);

            if (_notificationTcs != null)
            {
                _notificationTcs.TrySetResult(false);
            }
            _notificationTcs = new TaskCompletionSource<bool>();

            var intent = new Intent(context, typeof(NotificationRedirectActivity));

            intent.SetFlags(ActivityFlags.ReorderToFront);

            // NOTE: request code CANNOT be 0 for the notification to bring up the activity.
            var pendingIntent = PendingIntent.GetActivity(context, Environment.TickCount, intent, PendingIntentFlags.UpdateCurrent);
            builder.SetContentIntent(pendingIntent);

            if (DateTime.UtcNow - _lastNotificationTime > TimeSpan.FromSeconds(5))
            {
                _lastNotificationTime = DateTime.UtcNow;
                builder.SetDefaults((int)(NotificationDefaults.Sound));
            }

            var notification = builder.Build();
            if (LargeIcon.HasValue)
            {
                var iconBitmap = BitmapFactory.DecodeResource(context.Resources, LargeIcon.Value);
                var width = (int)context.Resources.GetDimension(Android.Resource.Dimension.NotificationLargeIconWidth);
                var height = (int)context.Resources.GetDimension(Android.Resource.Dimension.NotificationLargeIconHeight);

                if (iconBitmap.Width != width || iconBitmap.Height != height)
                {
                    var largeIconBitmap = Bitmap.CreateScaledBitmap(iconBitmap, width, height, true);
                    iconBitmap.Dispose();
                    iconBitmap = largeIconBitmap;
                }

                // http://stackoverflow.com/questions/13847297/notificationcompat-4-1-setsmallicon-and-setlargeicon
                notification.ContentView.SetImageViewBitmap(Android.Resource.Id.Icon, iconBitmap);
            }

            manager.Notify(NotificationCode, notification);

            var notificationId = ++lastNotificationId;

            cancellation.Register(() =>
            {
                if (notificationId == lastNotificationId)
                {
                    manager.Cancel(NotificationCode);
                }
            });

            return _notificationTcs.Task;
        }

        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            var context = _contextFactory();
            if (context == null) return Task.FromResult<int?>(null);

            var tcs = new TaskCompletionSource<int?>();
            var dialog = new AlertDialog.Builder(context)
                .SetTitle(title)
                .SetSingleChoiceItems(items.ToArray(),
                selectedIndex ?? 0,
                new ClickListener((e, i) =>
                {
                    e.Dismiss();
                    tcs.TrySetResult(i);
                }))
                .SetOnCancelListener(new ClickListener((e, i) =>
                {
                    e.Dismiss();
                    tcs.TrySetResult(null);
                }))
                .Create();

            dialog.Show();
            cancellation.Register(() =>
            {
                dialog.Dismiss();
                tcs.TrySetResult(null);
            });
            return tcs.Task;
        }

        public virtual Task<string> Input(string title, string defaultText, string yes, bool password, CancellationToken cancellation)
        {
            var context = _contextFactory();
            if (context == null) return Task.FromResult("");

            var tcs = new TaskCompletionSource<string>();
            var input = new EditText(context) { Text = defaultText };

            input.SetSingleLine();

            if (password)
            {
                input.SetFilters(new[] { new InputFilterLengthFilter(20) });
                input.InputType = InputTypes.ClassText | InputTypes.TextVariationPassword;
                input.TransformationMethod = PasswordTransformationMethod.Instance;
            }
            else
            {
                input.SetFilters(new[] { new InputFilterLengthFilter(140) });
            }

            var dialog = new AlertDialog.Builder(context)
                .SetTitle(title)
                .SetView(input)
                .SetPositiveButton(yes, new ClickListener((e, i) =>
                {
                    e.Dismiss();
                    tcs.TrySetResult(input.Text);
                }))
                .SetOnCancelListener(new ClickListener((e, i) =>
                {
                    e.Dismiss();
                    tcs.TrySetResult(null);
                }))
                .Create();

            input.EditorAction += (sender, e) =>
            {
                if (e.Event == null || e.Event.KeyCode == Keycode.Enter)
                {
                    var text = input.Text;
                    dialog.Dismiss();
                    tcs.TrySetResult(text);
                }
            };

            input.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    input.SelectAll();
                    dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
                }
            };

            dialog.Show();
            cancellation.Register(() =>
            {
                dialog.Dismiss();
                tcs.TrySetResult(null);
            });
            return tcs.Task;
        }

        public virtual void RateMe()
        {
            var activity = _contextFactory() as Activity;
            if (activity == null) return;

            var uri = Android.Net.Uri.Parse("market://details?id=" + activity.PackageName);
            activity.StartActivity(new Intent(Intent.ActionView, uri));
        }

        public virtual void CopyToClipboard(string text)
        {
            var context = _contextFactory();
            if (context == null) return;

            ((Android.Content.ClipboardManager)context.GetSystemService(Context.ClipboardService)).Text = text;
        }

        public virtual void Browse(string url)
        {
            var context = _contextFactory();
            if (context == null) return;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
        }

        public virtual Task<Stream> CaptureScreenshot()
        {
            var activity = _contextFactory() as Activity;
            if (activity == null) return null;

            return CaptureScreenshot(activity.Window.DecorView.RootView);
        }

        public virtual Task<Stream> CaptureScreenshot(View view)
        {
            if (view == null) return null;

            using (var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888))
            {
                var canvas = new Canvas(bitmap);
                view.Draw(canvas);
                var ms = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return Task.FromResult<Stream>(ms);
            }
        }
    }
}
