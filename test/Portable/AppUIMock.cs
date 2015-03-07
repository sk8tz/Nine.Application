namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class AppUIMock : IAppUI
    {
        private readonly IAppUI ui;
        private readonly IMediaLibrary media;
        private readonly IClientInfoProvider clientInfo;
        private readonly TimeSpan delay;
        private readonly string prefix;

        private int ordinal;

        private bool confirm;
        private bool notify;
        private string input;
        private int? select;

        public AppUIMock(IAppUI ui, [CallerMemberName]string prefix = null, IMediaLibrary media = null, TimeSpan delay = default(TimeSpan))
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));

            this.ui = ui;
            this.prefix = prefix;
            this.media = media ?? new MediaLibrary();
            this.clientInfo = new ClientInfoProvider();
            this.delay = (delay == default(TimeSpan) ? TimeSpan.FromSeconds(1) : delay);
        }

        public AppUIMock Yes() { confirm = true; return this; }
        public AppUIMock No() { confirm = false; return this; }
        public AppUIMock Notify(bool value) { notify = value; return this; }
        public AppUIMock Input(string text) { input = text; return this; }
        public AppUIMock Select(int? index) { select = index; return this; }

        public Task<Stream> CaptureScreenshot()
        {
            return ui.CaptureScreenshot();
        }

        public async Task<bool> Confirm(string title, string message, string yes, string no = null, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                await ui.Confirm(title, message, yes, no, DelayCancellation(cancellation));
            }
            catch (TaskCanceledException) { }
            return confirm;
        }

        public async Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                await ui.Input(title, defaultText, yes, DelayCancellation(cancellation));
            }
            catch (TaskCanceledException) { }
            return input;
        }

        public async Task<bool> Notify(string title, string message, IDictionary<string, string> args = null, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                await ui.Notify(title, message, args, DelayCancellation(cancellation));
            }
            catch (TaskCanceledException) { }
            return notify;
        }

        public async Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                await ui.Select(title, selectedIndex, items, DelayCancellation(cancellation));
            }
            catch (TaskCanceledException) { }
            return select;
        }

        public IProgressScope Progress(string message = null)
        {
            return ui.Progress(message);
        }

        public async void RateMe()
        {
            ui.RateMe();
            await Snapshot();
        }

        public async void Status(string message, TimeSpan? duration)
        {
            ui.Status(message, duration);
            await Snapshot();
        }

        public async void Toast(string title, string message)
        {
            ui.Toast(title, message);
            await Snapshot();
        }

        public async void Browse(string url)
        {
            ui.Browse(url);
            await Snapshot();
        }

        public void CopyToClipboard(string text)
        {
            ui.CopyToClipboard(text);
        }

        private CancellationToken DelayCancellation(CancellationToken cancellation)
        {
            var delayedCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, delayedCts.Token);
            Snapshot(delayedCts);
            return cts.Token;
        }

        private async void Snapshot(CancellationTokenSource cts)
        {
            await Task.Delay(delay);
            await Snapshot();
            cts.Cancel();
        }

        private async Task Snapshot()
        {
            var info = await clientInfo.GetAsync();
            var screenshot = await ui.CaptureScreenshot();
            var screenshotName = "UITest/" + info.OperatingSystem + "/" + prefix + "_" + (ordinal++) + ".jpg";
            await media.SaveImageToLibrary(screenshot, screenshotName);
        }
    }
}