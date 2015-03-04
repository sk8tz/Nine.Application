namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Phone.Tasks;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;

    public partial class MediaLibrary : IMediaLibrary
    {
        public Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue)
        {
            var tcs = new TaskCompletionSource<Stream>();
            var photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = showCamera;
            photoChooserTask.Completed += (a, image) =>
            {
                if (image.Error != null)
                {
                    tcs.TrySetException(image.Error);
                    return;
                }
                if (image.TaskResult != TaskResult.OK)
                {
                    tcs.TrySetResult(null);
                    return;
                }
                tcs.TrySetResult(CompressImage(image.ChosenPhoto, maxSize));
            };
            photoChooserTask.Show();
            return tcs.Task;
        }

        public Task SaveImageToLibrary(Stream image, string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentException("filename");

            using (var library = new Microsoft.Xna.Framework.Media.MediaLibrary())
            {
                library.SavePicture(filename, image);
                return Task.FromResult(0);
            }
        }

        private Stream CompressImage(Stream stream, int maxSize)
        {
            var result = new MemoryStream();
            var bitmap = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            bitmap.SetSource(stream);
            var writable = new WriteableBitmap(bitmap);
            var size = Crop(writable.PixelWidth, writable.PixelHeight, maxSize);
            writable.SaveJpeg(result, size.Item1, size.Item2, 0, 100);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        public void PlayAudio(Stream stream)
        {
            // SoundEffect will dispose the stream.
            var copy = new MemoryStream();
            stream.CopyTo(copy);
            copy.Seek(0, SeekOrigin.Begin);

            var sound = SoundEffect.FromStream(copy);
            var instance = sound.CreateInstance();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };

            EventHandler tick;
            tick = (sender, e) =>
            {
                FrameworkDispatcher.Update();
                if (instance.State == SoundState.Stopped)
                {
                    timer.Stop();
                }
            };

            timer.Tick += tick;
            timer.Start();

            FrameworkDispatcher.Update();
            instance.Play();
        }

        private DispatcherTimer audioCaptureTimer;
        private byte[] audioCaptureBuffer;
        private Stream audioCaptureStream;

        public void BeginCaptureAudio()
        {
            var microphone = Microphone.Default;
            if (microphone.State == MicrophoneState.Started) return;

            if (audioCaptureBuffer == null)
            {
                microphone.BufferDuration = TimeSpan.FromMilliseconds(100);
                audioCaptureBuffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];

                microphone.BufferReady += (sender, e) =>
                {
                    if (audioCaptureStream != null)
                    {
                        microphone.GetData(audioCaptureBuffer);
                        audioCaptureStream.Write(audioCaptureBuffer, 0, audioCaptureBuffer.Length);
                    }
                };

                audioCaptureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
                audioCaptureTimer.Tick += (sender, e) => { FrameworkDispatcher.Update(); };
            }

            audioCaptureStream = new MemoryStream();
            WriteWavHeader(audioCaptureStream, microphone.SampleRate);

            FrameworkDispatcher.Update();
            audioCaptureTimer.Start();
            microphone.Start();
        }

        public Stream EndCaptureAudio()
        {
            var result = audioCaptureStream;
            if (result == null) return null;

            Microphone.Default.Stop();
            UpdateWavHeader(audioCaptureStream);
            audioCaptureTimer.Stop();
            audioCaptureStream = null;
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private MediaElement player;
        private TaskCompletionSource<bool> playerTcs;

        public Task PlaySound(string uri)
        {
            if (player == null)
            {
                var playerContainer = (Grid)FindFirstChild(Application.Current.RootVisual, x =>
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

                player.MediaFailed += (sender, e) => { playerTcs.TrySetException(e.ErrorException); };
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

        private static object FindFirstChild(object value, Func<object, bool> predicate)
        {
            var element = value as System.Windows.FrameworkElement;
            if (element == null) return null;

            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
                if (child == null) continue;

                if (predicate(child)) return child;

                var result = FindFirstChild(child, predicate);
                if (result != null) return result;
            }

            return null;
        }
    }
}
