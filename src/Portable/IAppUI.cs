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

        Task<bool> Notify(string title, string message, IDictionary<string, string> args = null, CancellationToken cancellation = default(CancellationToken));

        Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken));

        Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation = default(CancellationToken));

        Task<Stream> CaptureScreenshot();

        IProgressScope Progress(string message = null);

        void Status(string message, TimeSpan? duration);

        void Toast(string title, string message);

        void CopyToClipboard(string text);

        void RateMe();

        void Browse(string url);
    }

    public interface IProgressScope :
        IProgress<string>,  // The text message that indicates the progress
        IProgress<float?>,  // The percentage of the progress ranging from 0 to 1, or null to indicate this is a indeterminate task
        IDisposable         // When disposed, hide the progress
    { }
}
