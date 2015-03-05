namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class AppUIMock : IAppUI
    {
        private readonly IAppUI ui;
        private readonly TimeSpan delay;

        private bool confirm;
        private bool notify;
        private string input;
        private int? select;

        public AppUIMock(IAppUI ui, TimeSpan delay = default(TimeSpan))
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));

            this.ui = ui;
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
            await ui.Confirm(title, message, yes, no, DelayCancellation(cancellation));
            await Snapshot();
            return confirm;
        }

        public async Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation = default(CancellationToken))
        {
            await ui.Input(title, defaultText, yes, DelayCancellation(cancellation));
            await Snapshot();
            return input;
        }

        public async Task<bool> Notify(string title, string message, IDictionary<string, string> args = null, CancellationToken cancellation = default(CancellationToken))
        {
            await ui.Notify(title, message, args, DelayCancellation(cancellation));
            await Snapshot();
            return notify;
        }

        public async Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken))
        {
            await ui.Select(title, selectedIndex, items, DelayCancellation(cancellation));
            await Snapshot();
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
            var delayedCts = new CancellationTokenSource(delay);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, delayedCts.Token);
            return cts.Token;
        }

        private Task Snapshot()
        {
            return ui.CaptureScreenshot();
        }
    }
}