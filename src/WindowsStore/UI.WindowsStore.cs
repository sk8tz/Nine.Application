namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using NotificationsExtensions.ToastContent;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Graphics.Display;
    using Windows.Graphics.Imaging;
    using Windows.Storage.Streams;
    using Windows.System;
    using Windows.UI.Notifications;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Media.Imaging;

    public partial class AppUI : IAppUI
    {
        public static FrameworkElement LastActionItem { get; set; }

        public async Task<bool> Confirm(string title, string message, string yes, string no, CancellationToken cancellation)
        {
            if (LastActionItem == null)
            {
                var dialog = new MessageDialog(message, title);
                var yesCommand = new UICommand(yes);
                dialog.Commands.Clear();
                dialog.Commands.Add(yesCommand);
                if (!string.IsNullOrEmpty(no)) dialog.Commands.Add(new UICommand(no, x => { }));

                var run = dialog.ShowAsync();
                cancellation.Register(() => run.Cancel());
                return yesCommand == await run;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();
                var menu = new Flyout { Placement = FlyoutPlacementMode.Bottom };
                var button = new Button { Content = yes, HorizontalAlignment = HorizontalAlignment.Center };
                button.Click += (a, b) => { tcs.TrySetResult(true); menu.Hide(); };
                menu.Closed += (a, b) => { tcs.TrySetResult(false); };

                var panel = new StackPanel { Width = 320 };
                panel.Children.Add(new TextBlock { Text = message, FontSize = 16, TextWrapping = TextWrapping.WrapWholeWords });
                panel.Children.Add(new Border { BorderThickness = new Thickness(0), Height = 24 });
                panel.Children.Add(button);

                menu.Content = panel;
                menu.ShowAt(LastActionItem);
                LastActionItem = null;

                cancellation.Register(() => menu.Hide());

                return await tcs.Task;
            }
        }

        public void Toast(string title, string message)
        {
            Notify(title, message, false, CancellationToken.None);
        }

        public Task<bool> Notify(string title, string message, IDictionary<string, string> args, CancellationToken cancellation)
        {
            return Notify(title, message, true, cancellation);
        }

        private Task<bool> Notify(string title, string message, bool playAudio, CancellationToken cancellation)
        {
            var templateContent = new ToastText02();
            templateContent.TextHeading.Text = title;
            templateContent.TextBodyWrap.Text = message;

            if (!playAudio)
            {
                templateContent.Audio.Content = ToastAudioContent.Silent;
            }

            var tcs = new TaskCompletionSource<bool>();
            var notification = templateContent.CreateNotification();
            notification.Activated += (sender, e) => { tcs.TrySetResult(true); };
            notification.Failed += (sender, e) => { tcs.TrySetResult(false); };
            notification.Dismissed += (sender, e) => { tcs.TrySetResult(false); };

            ToastNotificationManager.CreateToastNotifier().Show(notification);
            return tcs.Task;
        }

        public async Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation)
        {
            if (LastActionItem != null)
            {
                var i = 0;
                var tcs = new TaskCompletionSource<int?>();

                var menu = new MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
                foreach (var item in items)
                {
                    var local = i;
                    MenuFlyoutItem menuItem;
                    if (selectedIndex != null)
                    {
                        menu.Items.Add(menuItem = new ToggleMenuFlyoutItem { Text = item, IsChecked = selectedIndex == i });
                    }
                    else
                    {
                        menu.Items.Add(menuItem = new MenuFlyoutItem { Text = item });
                    }
                    menuItem.Click += (a, b) => { tcs.TrySetResult(local); };
                    menu.Closed += (a, b) => { LastActionItem = null; tcs.TrySetResult(null); };
                    i++;
                }

                menu.ShowAt(LastActionItem);
                cancellation.Register(() => menu.Hide());
                return await tcs.Task;
            }

            return null;
        }

        public Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation)
        {
            var tcs = new TaskCompletionSource<string>();
            var input = new TextBox { FontSize = 16, AcceptsReturn = false };
            var menu = new Flyout { Placement = FlyoutPlacementMode.Bottom };
            var button = new Button { Content = yes, HorizontalAlignment = HorizontalAlignment.Center };
            button.Click += (a, b) => { tcs.TrySetResult(input.Text); menu.Hide(); };
            input.KeyDown += (s, e) => { if (e.Key == VirtualKey.Enter) { tcs.TrySetResult(input.Text); menu.Hide(); } };
            menu.Closed += (a, b) => { tcs.TrySetResult(null); };

            var panel = new StackPanel { Width = 320 };
            if (!string.IsNullOrEmpty(title))
            {
                panel.Children.Add(new TextBlock { Text = title, FontSize = 20, TextWrapping = TextWrapping.NoWrap });
                panel.Children.Add(new Border { BorderThickness = new Thickness(0), Height = 24 });
            }
            panel.Children.Add(input);
            panel.Children.Add(new Border { BorderThickness = new Thickness(0), Height = 24 });
            panel.Children.Add(button);

            input.Text = defaultText ?? "";
            input.SelectAll();

            menu.Content = panel;
            menu.ShowAt(LastActionItem);
            LastActionItem = null;
            cancellation.Register(() => menu.Hide());
            return tcs.Task;
        }

        public void CopyToClipboard(string text)
        {
            var content = new DataPackage();
            content.SetText(text);
            Clipboard.SetContent(content);
        }

        public Task<Stream> CaptureScreenshot()
        {
            return CaptureScreenshot(Window.Current.Content);
        }

        public async Task<Stream> CaptureScreenshot(UIElement element)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var ms = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)renderTargetBitmap.PixelWidth,
                (uint)renderTargetBitmap.PixelHeight,
                DisplayInformation.GetForCurrentView().LogicalDpi,
                DisplayInformation.GetForCurrentView().LogicalDpi,
                pixelBuffer.ToArray());
            await encoder.FlushAsync();
            return ms.AsStreamForRead();
        }

        public async void RateMe()
        {
            var pfn = Package.Current.Id.FamilyName;
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + pfn));
        }

        public async void Browse(string url)
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
