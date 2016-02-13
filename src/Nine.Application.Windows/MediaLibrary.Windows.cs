namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.Graphics.Imaging;
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
        public AudioEncodingQuality AudioEncodingQuality { get; set; } = AudioEncodingQuality.Medium;

        public async Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue)
        {
            var raw = await PickImageRaw(location);

            if (raw == null) return null;

            using (var stream = await raw.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);

                var size = MediaHelper.Crop((int)decoder.PixelWidth, (int)decoder.PixelHeight, maxSize);
                if (size.Item1 != decoder.PixelWidth || size.Item2 != decoder.PixelHeight)
                {
                    var ms = new InMemoryRandomAccessStream();
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(ms, decoder);
                    encoder.BitmapTransform.ScaledWidth = (uint)size.Item1;
                    encoder.BitmapTransform.ScaledHeight = (uint)size.Item2;
                    await encoder.FlushAsync();
                    return ms.AsStreamForRead();
                }
            }

            return await raw.OpenStreamForReadAsync();
        }

        private async Task<IStorageFile> PickImageRaw(ImageLocation location = ImageLocation.All)
        {
            if (location.HasFlag(ImageLocation.Camera))
            {
                var cameraUI = new CameraCaptureUI();
                return await cameraUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            }
            else
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Clear();
                picker.FileTypeFilter.Add(".bmp");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".jpg");

                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

                return await picker.PickSingleFileAsync();
            }
        }

        public async Task<string> SaveImageToLibrary(Stream image, string filename)
        {
            if (image == null || string.IsNullOrEmpty(filename)) return null;

            var picturesFolder = KnownFolders.PicturesLibrary;
            var folder = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(folder))
            {
                picturesFolder = await picturesFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
            }

            var name = Path.GetFileName(filename);
            var file = await picturesFolder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await image.CopyToAsync(stream);
            }

            return file.Path;
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

        public async Task<bool> BeginCaptureAudio()
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
                return false;
            }

            recorderBuffer = new InMemoryRandomAccessStream();
            recorder.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality), recorderBuffer);
            isRecording = true;
            return true;
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
