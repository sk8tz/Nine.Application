namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Coding4Fun.Toolkit.Controls;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;

    public partial class AppUI : IAppUI
    {
        public class AppBarPromptSelector : AppBarPrompt
        {
            public AppBarPromptSelector(IEnumerable<AppBarPromptAction> appBarPromptAction) : base(appBarPromptAction.ToArray()) { }

            public int SelectedIndex { get; set; }

            public override void OnApplyTemplate()
            {
                base.OnApplyTemplate();

                if (SelectedIndex >= 0 && SelectedIndex < Body.Children.Count)
                {
                    ((AppBarPromptItem)Body.Children[SelectedIndex]).Foreground = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                }
            }
        }

        public Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            // MessagePrompt
            return Task.FromResult(
                MessageBox.Show(message, title, no != null ? MessageBoxButton.OKCancel : MessageBoxButton.OK) == MessageBoxResult.OK);
        }

        public void Toast(string title, string message)
        {
            Notify(title, message, null, CancellationToken.None);
        }

        public Task<bool> Notify(string title, string message, IDictionary<string, string> args, CancellationToken cancellation)
        {
            // TODO: we don't support deep link on windows phone

            var tcs = new TaskCompletionSource<bool>();
            var toast = new ToastPrompt { Title = title, Message = message, TextWrapping = TextWrapping.Wrap };
            toast.Completed += (sender, e) =>
            {
                tcs.TrySetResult(e.PopUpResult == PopUpResult.Ok);
            };
            toast.Show();
            return tcs.Task;
        }

        public async Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            var i = 0;
            var tcs = new TaskCompletionSource<int?>();
            var prompts = new List<AppBarPromptAction>();
            foreach (var item in items)
            {
                var local = i++;
                prompts.Add(new AppBarPromptAction(item, () => tcs.TrySetResult(local)));
            };

            var popup = new AppBarPromptSelector(prompts) { SelectedIndex = selectedIndex ?? 0 };
            popup.Completed += (sender, e) =>
            {
                if (e.PopUpResult != PopUpResult.Ok) tcs.TrySetResult(null);
            };

            // Put a delay here to avoid a display bug
            await Task.Delay(200);
            popup.Show();
            cancellation.Register(() => popup.Hide());
            return await tcs.Task;
        }

        public Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation)
        {
            var tcs = new TaskCompletionSource<string>();
            var prompt = new InputPrompt { Title = title, Value = defaultText };
            prompt.BorderThickness = new Thickness(0);
            prompt.Completed += (a, r) =>
            {
                tcs.TrySetResult(r.PopUpResult == PopUpResult.Ok ? r.Result : null);
            };

            try
            {
                prompt.Show();
                cancellation.Register(() => prompt.Hide());
                return tcs.Task;
            }
            catch
            {
                return Task.FromResult<string>(null);
            }
        }

        public void RateMe()
        {
            new MarketplaceReviewTask().Show();
        }

        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public void Browse(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)) return;
            new WebBrowserTask { Uri = uri }.Show();;
        }

        public Task<Stream> CaptureScreenshot()
        {
            return CaptureScreenshot(Application.Current.RootVisual as FrameworkElement);
        }

        public Task<Stream> CaptureScreenshot(FrameworkElement element)
        {
            var bmp = new WriteableBitmap((int)element.ActualWidth, (int)element.ActualHeight);
            bmp.Render(element, null);
            bmp.Invalidate();

            using (var stream = new MemoryStream())
            {
                bmp.SaveJpeg(stream, bmp.PixelWidth, bmp.PixelHeight, 0, 90);
                stream.Seek(0, SeekOrigin.Begin);
                return Task.FromResult<Stream>(stream);
            }
        }
    }
}
