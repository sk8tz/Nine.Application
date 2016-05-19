namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAppUI
    {
        Task<bool> Confirm(string title, string message, string yes, string no = null, CancellationToken cancellation = default(CancellationToken));

        Task<bool> Notify(string title, string message = null, CancellationToken cancellation = default(CancellationToken));

        Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken));

        Task<string> Input(string title, string defaultText, string yes, bool password = false, CancellationToken cancellation = default(CancellationToken));

        Task<Stream> CaptureScreenshot();

        void Toast(string title, string message);

        void CopyToClipboard(string text);

        void RateMe();

        void Browse(string url);
    }
}
