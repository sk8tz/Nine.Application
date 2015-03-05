﻿namespace Nine.Application
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
    using Android.Views;
    using Android.Widget;

    public partial class AppUI : IAppUI
    {
        private const int NotificationCode = 90002;

        public static readonly Dictionary<string, Type> KnownTypes = new Dictionary<string, Type>();

        public virtual Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            if (ActivityContext.Current == null) return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>();
            var builder = new AlertDialog.Builder(ActivityContext.Current)
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
            cancellation.Register(() => dialog.Hide());
            return tcs.Task;
        }

        public virtual void Toast(string title, string message)
        {
            if (ActivityContext.Current == null) return;
            if (!string.IsNullOrEmpty(title)) message = title + ": " + message;

            Android.Widget.Toast.MakeText(ActivityContext.Current, message, ToastLength.Short).Show();
        }

        public virtual Task<bool> Notify(string title, string message, IDictionary<string, string> args, CancellationToken cancellation)
        {
            if (ActivityContext.Current == null) return Task.FromResult(false);
            if (!string.IsNullOrEmpty(title)) message = title + ": " + message;

            title = title ?? "";
            message = message ?? "";

            var context = ActivityContext.Current;
            var packageName = context.PackageName;
            var icon = context.Resources.GetIdentifier("icon", "drawable", packageName);

            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(icon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetTicker(message);

            if (args != null && args.ContainsKey("type"))
            {
                var intent = new Intent(context, KnownTypes[args["type"]]);
                foreach (var item in args.Where(x => x.Key != "type"))
                {
                    intent.PutExtra(item.Key, item.Value);
                }

                intent.SetFlags(ActivityFlags.ReorderToFront);

                // NOTE: request code CANNOT be 0 for the notification to bring up the activity.
                var pendingIntent = PendingIntent.GetActivity(context, System.Environment.TickCount, intent, PendingIntentFlags.UpdateCurrent);
                builder.SetContentIntent(pendingIntent);
            }

            builder.SetDefaults((int)(NotificationDefaults.Sound | NotificationDefaults.Vibrate));

            manager.Notify(NotificationCode, builder.Build());
            return Task.FromResult(false);
        }

        public static void ClearNotifications()
        {
            if (ActivityContext.Current == null) return;
            var manager = (NotificationManager)ActivityContext.Current.GetSystemService(Context.NotificationService);
            manager.Cancel(NotificationCode);
        }

        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            if (ActivityContext.Current == null) return Task.FromResult<int?>(null);

            var tcs = new TaskCompletionSource<int?>();
            var dialog = new AlertDialog.Builder(ActivityContext.Current)
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
            cancellation.Register(() => dialog.Hide());
            return tcs.Task;
        }

        public virtual Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation)
        {
            if (ActivityContext.Current == null) return Task.FromResult("");

            var tcs = new TaskCompletionSource<string>();
            var input = new EditText(ActivityContext.Current) { Text = defaultText };
            input.SetSingleLine();
            
            var dialog = new AlertDialog.Builder(ActivityContext.Current)
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

            dialog.Show();
            cancellation.Register(() => dialog.Hide());
            return tcs.Task;
        }

        public virtual void RateMe()
        {
            var activity = ActivityContext.Current as Activity;
            if (activity == null) return;

            var uri = Android.Net.Uri.Parse("market://details?id=" + activity.PackageName);
            activity.StartActivity(new Intent(Intent.ActionView, uri));
        }

        public virtual void CopyToClipboard(string text)
        {
            ((ClipboardManager)ActivityContext.Current.GetSystemService(Context.ClipboardService)).Text = text;
        }

        public virtual void Browse(string url)
        {
            if (ActivityContext.Current == null) return;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            ActivityContext.Current.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
        }

        public virtual Task<Stream> CaptureScreenshot()
        {
            var activity = ActivityContext.Current as Activity;
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
                using (var ms = new MemoryStream())
                {
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Task.FromResult<Stream>(ms);
                }
            }
        }
    }
}
