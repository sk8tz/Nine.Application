namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content;
    using Android.Graphics;
    using Android.Media;
    using Android.OS;
    using Android.Provider;
    using Stream = System.IO.Stream;

    public partial class MediaLibrary : IMediaLibrary
    {
        private const int ImagePickerCode = 90001;

        private static TaskCompletionSource<Stream> imageChooserTcs;

        private int _maxSize;
        private Android.Net.Uri _imagePath;
        private Android.Net.Uri _compressedPath;

        private readonly Func<Context> contextFactory;

        public MediaLibrary() : this(ContextProvider.Current) { }
        public MediaLibrary(Context context) : this(() => context) { }
        public MediaLibrary(Func<Context> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            this.contextFactory = contextFactory;
        }

        public Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue)
        {
            var activity = contextFactory() as Activity;
            if (activity == null) return Task.FromResult<Stream>(null);

            var root = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory + "/temp");
            root.Mkdirs();

            _imagePath = Android.Net.Uri.FromFile(new Java.IO.File(root, Guid.NewGuid().ToString("N") + ".jpg"));
            _compressedPath = Android.Net.Uri.FromFile(new Java.IO.File(root, Guid.NewGuid().ToString("N") + ".jpg"));

            _maxSize = maxSize;

            // File system.
            var galleryIntent = new Intent();
            galleryIntent.SetType("image/*");
            galleryIntent.SetAction(Intent.ActionGetContent);

            var chooserIntent = Intent.CreateChooser(galleryIntent, "");

            // Camera.
            if (showCamera)
            {
                var cameraIntents = new List<Intent>();
                var captureIntent = new Intent(MediaStore.ActionImageCapture);
                foreach (var res in activity.PackageManager.QueryIntentActivities(captureIntent, 0))
                {
                    var intent = new Intent(captureIntent);
                    intent.SetComponent(new ComponentName(res.ActivityInfo.PackageName, res.ActivityInfo.Name));
                    intent.SetPackage(res.ActivityInfo.PackageName);
                    intent.PutExtra(MediaStore.ExtraOutput, _imagePath);
                    cameraIntents.Add(intent);
                }
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, cameraIntents.Cast<IParcelable>().ToArray());
            }

            imageChooserTcs = new TaskCompletionSource<Stream>();
            activity.StartActivityForResult(chooserIntent, ImagePickerCode);
            return imageChooserTcs.Task;
        }

        public void SetActivityResult(int requestCode, Result resultCode, Intent data)
        {
            var context = contextFactory();
            if (context == null) return;

            if (requestCode == ImagePickerCode)
            {
                if (imageChooserTcs == null) return;
                if (resultCode != Result.Ok)
                {
                    imageChooserTcs.TrySetResult(null); return;
                }

                var selectedImageUri = data?.Data ?? _imagePath;
                if (selectedImageUri == null)
                {
                    imageChooserTcs.TrySetResult(null); return;
                }

                try
                {
                    using (var input = context.ContentResolver.OpenInputStream(selectedImageUri))
                    using (var output = context.ContentResolver.OpenOutputStream(_compressedPath, "w"))
                    using (var bitmap = BitmapFactory.DecodeStream(input))
                    {
                        var size = Crop(bitmap.Width, bitmap.Height, _maxSize);
                        using (var resized = Bitmap.CreateScaledBitmap(bitmap, size.Item1, size.Item2, true))
                        {
                            if (!resized.Compress(Bitmap.CompressFormat.Jpeg, 80, output))
                            {
                                imageChooserTcs.TrySetResult(null);
                                return;
                            }
                        }
                    }

                    var compressed = _compressedPath;

                    imageChooserTcs.TrySetResult(new DelegateStream(
                        () => context.ContentResolver.OpenInputStream(compressed),
                        () => new Java.IO.File(compressed.Path).Delete()));
                }
                catch
                {
                    imageChooserTcs.TrySetResult(null);
                }
                finally
                {
                    if (_imagePath == selectedImageUri)
                    {
                        new Java.IO.File(selectedImageUri.Path).Delete();
                    }
                }
            }
        }

        public async Task SaveImageToLibrary(Stream image, string filename)
        {
            var context = contextFactory();
            if (context == null) return;
            if (string.IsNullOrEmpty(filename)) throw new ArgumentException(nameof(filename));

            var root = Android.OS.Environment.ExternalStorageDirectory;
            var imagePath = System.IO.Path.Combine(root.AbsolutePath, filename);
            var directory = System.IO.Path.GetDirectoryName(imagePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var imageFile = new Java.IO.File(imagePath);
            if (!imageFile.Exists()) imageFile.CreateNewFile();

            using (var file = new Java.IO.FileOutputStream(imageFile))
            {
                var bytes = new byte[image.Length];
                image.Read(bytes, 0, bytes.Length);
                file.Write(bytes, 0, bytes.Length);
                file.Flush();
            }

            image.Seek(0, SeekOrigin.Begin);
            var bitmap = await BitmapFactory.DecodeStreamAsync(image);

            MediaStore.Images.Media.InsertImage(context.ContentResolver, bitmap, imageFile.Name, "");
            context.SendBroadcast(new Intent(Intent.ActionMediaScannerScanFile, Android.Net.Uri.Parse("file://" + root)));
        }

        private bool trimAudioZeros;
        private AudioRecord recorder;
        private MemoryStream audioCaptureStream;
        private byte[] audioBuffer = new Byte[10240];
        private static readonly string recorderFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "LastRecorded.wav";

        public void BeginCaptureAudio()
        {
            if (recorder != null) recorder.Dispose();

            audioCaptureStream = new MemoryStream();
            WriteWavHeader(audioCaptureStream, DefaultAudioSamplingRate);

            recorder = new AudioRecord(AudioSource.Mic, DefaultAudioSamplingRate, ChannelIn.Mono, Encoding.Pcm16bit, audioBuffer.Length);

            if (recorder.State != State.Initialized)
            {
                recorder = null;
                return;
            }

            recorder.StartRecording();
            trimAudioZeros = true;

            ReadAudioBufferAsync();
        }

        private async void ReadAudioBufferAsync()
        {
            try
            {
                while (recorder != null)
                {
                    // Ensure we are on the UI thread.
                    var read = await recorder.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                    if (read > 0)
                    {
                        var offset = TrimAudioZeros(read);
                        if (read > offset) audioCaptureStream.Write(audioBuffer, offset, read - offset);
                    }
                }
            }
            catch { }
        }

        private int TrimAudioZeros(int read)
        {
            var offset = 0;
            if (trimAudioZeros)
            {
                trimAudioZeros = false;
                while (offset < read && audioBuffer[offset] == 0) offset++;
            }
            return offset;
        }

        public Stream EndCaptureAudio()
        {
            if (recorder != null)
            {
                recorder.Stop();

                var read = recorder.Read(audioBuffer, 0, audioBuffer.Length);
                var offset = TrimAudioZeros(read);
                if (read > offset) audioCaptureStream.Write(audioBuffer, offset, read - offset);

                recorder.Release();
                recorder.Dispose();
                recorder = null;

                UpdateWavHeader(audioCaptureStream);

                audioCaptureStream.Seek(0, SeekOrigin.Begin);
                return audioCaptureStream;
            }
            return null;
        }

        private MediaPlayer player;
        private TaskCompletionSource<bool> playerTcs;

        public Task PlaySound(string uri)
        {
            StopSound();

            var player = new MediaPlayer();
            var playerTcs = new TaskCompletionSource<bool>();

            player.Error += (sender, e) => { playerTcs.TrySetException(new InvalidOperationException("MediaPlayer error: " + e.What.ToString())); };
            player.Completion += (sender, e) => { playerTcs.TrySetResult(false); };
            player.Prepared += (sender, e) => { player.Start(); };

            player.SetDataSource(uri);
            player.PrepareAsync();

            this.player = player;
            this.playerTcs = playerTcs;

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
    }
}
