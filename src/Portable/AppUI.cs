namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class AppUI : IAppUI
    {
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        class ProgressScope : IProgressScope
        {
            private float? progress;
            private string message = "";

            public AppUI Owner;

            public void Report(string value)
            {
                Owner.PostToSynchronizationContext(() => Owner.Progress(true, message = value ?? "", progress));
            }

            public void Report(float? value)
            {
                if (value != null) value = Math.Min(Math.Max(value.Value, 0), 1);
                Owner.PostToSynchronizationContext(() => Owner.Progress(true, message, progress = value));
            }

            public void Dispose()
            {
                ProgressScope last = null;
                lock (Owner.progressScopes)
                {
                    Owner.progressScopes.Remove(this);
                    last = Owner.progressScopes.LastOrDefault();
                }

                if (last != null)
                {
                    Owner.PostToSynchronizationContext(() => Owner.Progress(true, last.message, last.progress));
                }
                else
                {
                    Owner.PostToSynchronizationContext(() => Owner.Progress(false, "", null));
                }
            }
        }

        public async virtual void Status(string message, TimeSpan? duration)
        {
            using (var progress = Progress())
            {
                progress.Report(message);
                await Task.Delay(duration.HasValue ? duration.Value : TimeSpan.MaxValue);
            }
        }

        private readonly List<ProgressScope> progressScopes = new List<ProgressScope>();

        public virtual IProgressScope Progress(string message = null)
        {
            lock (progressScopes)
            {
                var result = new ProgressScope { Owner = this };
                result.Report(message);
                progressScopes.Add(result);
                return result;
            }
        }

        protected void PostToSynchronizationContext(Action action)
        {
            if (syncContext != null)
            {
                syncContext.Post(x => action(), null);
            }
            else
            {
                action();
            }
        }

        protected virtual void Progress(bool show, string message, float? progress) { }

#if PCL
        public virtual Task<bool> Confirm(string title, string message, string yes, string no = null, CancellationToken cancellation = default(CancellationToken)) => Task.FromResult(false);
        public virtual Task<bool> Notify(string title, string message, IDictionary<string, string> args = null, CancellationToken cancellation = default(CancellationToken)) => Task.FromResult(false);
        public virtual Task<int?> Select(string title, int? selectedIndex, IEnumerable<string> items, CancellationToken cancellation = default(CancellationToken)) => Task.FromResult<int?>(null);
        public virtual Task<string> Input(string title, string defaultText, string yes, CancellationToken cancellation = default(CancellationToken)) => Task.FromResult<string>(null);
        public virtual Task<Stream> CaptureScreenshot() => Task.FromResult<Stream>(null);
        public virtual void Toast(string title, string message) { }
        public virtual void CopyToClipboard(string text) { }
        public virtual void RateMe() { }
        public virtual void Browse(string url) { }
#endif
    }
}
