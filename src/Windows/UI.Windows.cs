namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    public partial class AppUI : IAppUI
    {
        public Task<bool> Confirm(string title, string message, string yes, string no = null, CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(MessageBox.Show(message, title, no != null ? MessageBoxButton.OKCancel : MessageBoxButton.OK) == MessageBoxResult.OK);
        }

        public Task<bool> Notify(string title, string message, IDictionary<string, string> args = null, CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(false);
        }

        public Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<string> Input(string title, string defaultText, string yes, bool password, CancellationToken cancellation = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<Stream> CaptureScreenshot()
        {
            throw new NotImplementedException();
        }

        public void Toast(string title, string message)
        {
            MessageBox.Show(message, title);
        }

        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public void RateMe() { }
        public void Browse(string url) { }
    }
}
