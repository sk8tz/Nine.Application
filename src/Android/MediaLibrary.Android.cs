﻿namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
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
        private static Android.Net.Uri lastPickedImage;
        private static int maxImageSize;

        public Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue)
        {
            maxImageSize = maxSize;

            var activity = ActivityContext.Current as Activity;
            if (activity == null) return Task.FromResult<Stream>(null);

            var root = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory + "/Captured");
            root.Mkdirs();

            lastPickedImage = Android.Net.Uri.FromFile(new Java.IO.File(root, Guid.NewGuid().ToString("N") + ".jpg"));

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
                    intent.PutExtra(MediaStore.ExtraOutput, lastPickedImage);
                    cameraIntents.Add(intent);
                }
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, cameraIntents.Cast<IParcelable>().ToArray());
            }

            imageChooserTcs = new TaskCompletionSource<Stream>();
            activity.StartActivityForResult(chooserIntent, ImagePickerCode);
            return imageChooserTcs.Task;
        }

        public static void SetActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (ActivityContext.Current == null) return;

            if (requestCode == ImagePickerCode)
            {
                if (imageChooserTcs == null) return;
                if (resultCode != Result.Ok) { imageChooserTcs.TrySetResult(null); return; }

                var isCamera = (data != null && data.Action != null && data.Action == Android.Provider.MediaStore.ActionImageCapture);
                var selectedImageUri = (isCamera ? lastPickedImage : (data == null ? null : data.Data)) ?? lastPickedImage;
                if (selectedImageUri == null) return;

                using (var input = ActivityContext.Current.ContentResolver.OpenInputStream(selectedImageUri))
                {
                    var stream = new System.IO.MemoryStream();
                    var bitmap = BitmapFactory.DecodeStream(input);
                    var size = Crop(bitmap.Width, bitmap.Height, maxImageSize);
                    var resized = Bitmap.CreateScaledBitmap(bitmap, size.Item1, size.Item2, true);

                    if (!resized.Compress(Bitmap.CompressFormat.Jpeg, 80, stream)) return;

                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    imageChooserTcs.TrySetResult(stream);
                }
            }
        }

        public async Task SaveImageToLibrary(Stream image, string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentException("filename");
            if (ActivityContext.Current == null) return;

            var bitmap = await BitmapFactory.DecodeStreamAsync(image);
            MediaStore.Images.Media.InsertImage(ActivityContext.Current.ContentResolver, bitmap, filename, "");
        }

        private bool trimAudioZeros;
        private AudioRecord recorder;
        private MemoryStream audioCaptureStream;
        private byte[] audioBuffer = new Byte[10240];
        private static readonly string recorderFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "LastRecorded.wav";

        public async void BeginCaptureAudio()
        {
            if (recorder != null) recorder.Dispose();

            audioCaptureStream = new MemoryStream();
            WriteWavHeader(audioCaptureStream, DefaultAudioSamplingRate);

            recorder = new AudioRecord(AudioSource.Mic, DefaultAudioSamplingRate, ChannelIn.Mono, Encoding.Pcm16bit, audioBuffer.Length);
            recorder.StartRecording();
            trimAudioZeros = true;

            await ReadAudioBufferAsync();
        }

        private async Task ReadAudioBufferAsync()
        {
            while (recorder != null)
            {
                // Ensure ew are on the UI thread.
                var read = await recorder.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                if (read > 0)
                {
                    var offset = TrimAudioZeros(read);
                    if (read > offset) audioCaptureStream.Write(audioBuffer, offset, read - offset);
                }
            }
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
