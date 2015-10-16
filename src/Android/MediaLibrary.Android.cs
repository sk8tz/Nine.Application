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

        public MediaLibrary(Context context) : this(() => context) { }
        public MediaLibrary(Func<Context> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            this.contextFactory = contextFactory;
        }

        public Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue)
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
            if (location.HasFlag(ImageLocation.Camera))
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

            GC.Collect();
            Java.Lang.Runtime.GetRuntime().Gc();

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
                    using (var input = new DelegateStream(() => context.ContentResolver.OpenInputStream(selectedImageUri)))
                    using (var output = context.ContentResolver.OpenOutputStream(_compressedPath, "w"))
                    using (var bitmap = CreateBitmap(input, _maxSize))
                    {
                        if (bitmap == null)
                        {
                            imageChooserTcs.TrySetResult(null);
                            return;
                        }
                        if (!bitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, output))
                        {
                            imageChooserTcs.TrySetResult(null);
                            return;
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

                    GC.Collect();
                    Java.Lang.Runtime.GetRuntime().Gc();
                }
            }
        }

        public static Bitmap CreateBitmap(Stream stream, int maxPixelSize)
        {
            try
            {
                var op = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeStream(stream, null, op);
                stream.Seek(0, SeekOrigin.Begin);
                var sampleSize = ComputeSampleSize(op, -1, maxPixelSize * maxPixelSize);
                return BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options { InSampleSize = sampleSize });
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap CreateBitmap(string file, int maxPixelSize)
        {
            try
            {
                var op = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(file, op);
                var sampleSize = ComputeSampleSize(op, -1, maxPixelSize * maxPixelSize);
                return BitmapFactory.DecodeFile(file, new BitmapFactory.Options { InSampleSize = sampleSize });
            }
            catch
            {
                return null;
            }
        }

        private static int ComputeSampleSize(BitmapFactory.Options options, int minSideLength, int maxNumOfPixels)
        {
            int initialSize = ComputeInitialSampleSize(options, minSideLength, maxNumOfPixels);
            int roundedSize;
            if (initialSize <= 8)
            {
                roundedSize = 1;
                while (roundedSize < initialSize)
                {
                    roundedSize <<= 1;
                }
            }
            else
            {
                roundedSize = (initialSize + 7) / 8 * 8;
            }

            return roundedSize;
        }

        private static int ComputeInitialSampleSize(BitmapFactory.Options options, int minSideLength, int maxNumOfPixels)
        {
            double w = options.OutWidth;
            double h = options.OutHeight;

            int lowerBound = (maxNumOfPixels == -1) ? 1 : (int)Math.Ceiling(Math.Sqrt(w * h / maxNumOfPixels));
            int upperBound = (minSideLength == -1) ? 128 : (int)Math.Min(Math.Floor(w / minSideLength), Math.Floor(h / minSideLength));

            if (upperBound < lowerBound)
            {
                // return the larger one when there is no overlapping zone. 
                return lowerBound;
            }

            if ((maxNumOfPixels == -1) && (minSideLength == -1))
            {
                return 1;
            }
            else if (minSideLength == -1)
            {
                return lowerBound;
            }
            else
            {
                return upperBound;
            }
        }

        public async Task<string> SaveImageToLibrary(Stream image, string filename)
        {
            var context = contextFactory();
            if (context == null) return null;
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

            return imageFile.AbsolutePath;
        }

        private bool _trimAudioZeros;
        private AudioRecord _recorder;
        private MemoryStream _audioCaptureStream;
        private byte[] _audioBuffer = new byte[10240];
        private static readonly string _recorderFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "LastRecorded.wav";

        public void BeginCaptureAudio()
        {
            if (_recorder != null) _recorder.Dispose();

            _audioCaptureStream = new MemoryStream();
            WriteWavHeader(_audioCaptureStream, DefaultAudioSamplingRate);

            _recorder = new AudioRecord(AudioSource.Mic, DefaultAudioSamplingRate, ChannelIn.Mono, Encoding.Pcm16bit, _audioBuffer.Length);

            if (_recorder.State != State.Initialized)
            {
                _recorder = null;
                return;
            }

            _recorder.StartRecording();
            _trimAudioZeros = true;

            ReadAudioBufferAsync();
        }

        private async void ReadAudioBufferAsync()
        {
            try
            {
                while (_recorder != null)
                {
                    // Ensure we are on the UI thread.
                    var read = await _recorder.ReadAsync(_audioBuffer, 0, _audioBuffer.Length);

                    if (read <= 0 || _audioCaptureStream == null) break;

                    var offset = TrimAudioZeros(read);
                    if (read > offset)
                    {
                        _audioCaptureStream.Write(_audioBuffer, offset, read - offset);
                    }
                }
            }
            catch { }
        }

        private int TrimAudioZeros(int read)
        {
            var offset = 0;
            if (_trimAudioZeros)
            {
                _trimAudioZeros = false;
                while (offset < read && _audioBuffer[offset] == 0) offset++;
            }
            return offset;
        }

        public Stream EndCaptureAudio()
        {
            if (_recorder != null)
            {
                _recorder.Stop();

                var read = _recorder.Read(_audioBuffer, 0, _audioBuffer.Length);
                var offset = TrimAudioZeros(read);

                var audioStream = _audioCaptureStream;
                _audioCaptureStream = null;

                if (read > offset) audioStream.Write(_audioBuffer, offset, read - offset);

                _recorder.Release();
                _recorder.Dispose();
                _recorder = null;

                UpdateWavHeader(audioStream);

                audioStream.Seek(0, SeekOrigin.Begin);
                return audioStream;
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
