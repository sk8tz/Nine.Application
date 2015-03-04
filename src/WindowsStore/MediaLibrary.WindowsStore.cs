namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.Media.Capture;
    using Windows.Media.MediaProperties;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public partial class MediaLibrary : IMediaLibrary
    {
        public async Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            // TODO: Camera

            var file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            return await file.OpenStreamForReadAsync();
        }

        public async Task SaveImageToLibrary(Stream image, string filename)
        {
            var file = await KnownFolders.PicturesLibrary.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
            using (var stream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
            {
                await image.CopyToAsync(stream);
            }
        }

        private MediaElement player;
        private TaskCompletionSource<bool> playerTcs;

        public Task PlaySound(string uri)
        {
            if (player == null)
            {
                var playerContainer = (Grid)FindFirstChild(Window.Current.Content, x =>
                {
                    var e = x as Grid;
                    return e != null && e.Visibility == Visibility.Visible;
                });

                if (playerContainer == null)
                {
                    throw new InvalidOperationException("Not grid found to place the media element");
                }

                player = new MediaElement();
                playerTcs = new TaskCompletionSource<bool>();

                player.MediaFailed += (sender, e) => { playerTcs.TrySetException(new InvalidOperationException(e.ErrorMessage)); };
                player.MediaEnded += (sender, e) => { playerTcs.TrySetResult(false); };
                playerContainer.Children.Add(player);
            }

            playerTcs.TrySetResult(false);
            playerTcs = new TaskCompletionSource<bool>();
            player.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
            return playerTcs.Task;
        }

        public void StopSound()
        {
            if (player != null)
            {
                playerTcs.TrySetResult(true);
                player.Stop();
            }
        }

        private bool isRecording;
        private MediaCapture recorder;
        private InMemoryRandomAccessStream recorderBuffer;

        public async void BeginCaptureAudio()
        {
            if (recorder != null) recorder.Dispose();

            isRecording = false;

            recorder = new MediaCapture();
            recorder.Failed += (sender, e) => isRecording = false;

            try
            {
                await recorder.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio,
                    MediaCategory = MediaCategory.Communications,
                });
            }
            catch (UnauthorizedAccessException)
            {
                recorder = null;
                return;
            }

            recorderBuffer = new InMemoryRandomAccessStream();
            var record = recorder.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto), recorderBuffer);
            isRecording = true;
            await record;
        }

        public Stream EndCaptureAudio()
        {
            if (recorderBuffer == null || recorderBuffer == null || !isRecording) return null;

            isRecording = false;

            recorder.StopRecordAsync().AsTask().Wait();
            if (recorderBuffer.Size <= 0) return null;
            recorderBuffer.Seek(0);

            return recorderBuffer.AsStreamForRead();
        }

        private static object FindFirstChild(object value, Func<object, bool> predicate)
        {
            var element = value as FrameworkElement;
            if (element == null) return null;

            var count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child == null) continue;

                if (predicate(child)) return child;

                var result = FindFirstChild(child, predicate);
                if (result != null) return result;
            }

            return null;
        }
    }
}
