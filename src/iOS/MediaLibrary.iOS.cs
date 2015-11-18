namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AudioToolbox;
    using AVFoundation;
    using Foundation;
    using UIKit;
    using CoreGraphics;

    public partial class MediaLibrary : IMediaLibrary
    {
        public Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue)
        {
            var controller = ApplicationView.ViewController;

            var sourceType = location == ImageLocation.Camera 
                ? UIImagePickerControllerSourceType.Camera
                : UIImagePickerControllerSourceType.PhotoLibrary;
            
            var tcs = new TaskCompletionSource<Stream>();
            var picker = new UIImagePickerController { SourceType = sourceType, AllowsEditing = (sourceType == UIImagePickerControllerSourceType.Camera) };

            picker.Canceled += (sender, e) => { tcs.TrySetResult(null); picker.DismissViewController(true, null); };
            picker.FinishedPickingMedia += (sender, e) => { tcs.TrySetResult(GetStream(ResizeImage(e.EditedImage ?? e.OriginalImage, maxSize))); picker.DismissViewController(true, null); };
            picker.FinishedPickingImage += (sender, e) => { tcs.TrySetResult(GetStream(ResizeImage(e.Image, maxSize))); picker.DismissViewController(true, null); };

            if (controller != null)
            {
                controller.PresentViewController(picker, true, null);
            }
            else
            {
                UIApplication.SharedApplication.KeyWindow.RootViewController = picker;
            }
            return tcs.Task;
        }

        private static UIImage ResizeImage(UIImage image, int maxSize)
        {
            var newSize = Crop((int)image.Size.Width, (int)image.Size.Height, maxSize);
            if (newSize.Item1 == image.Size.Width && newSize.Item2 == image.Size.Height)
            {
                return image;
            }

            // https://github.com/giacgbj/UIImageSwiftExtensions/blob/master/UIImage%2BResize.swift
            var transpose = (image.Orientation == UIImageOrientation.Left ||
                             image.Orientation == UIImageOrientation.LeftMirrored ||
                             image.Orientation == UIImageOrientation.Right ||
                             image.Orientation == UIImageOrientation.RightMirrored);

            var rect = new CGRect(0, 0, newSize.Item1, newSize.Item2); 
            if (!transpose)
            {
                rect = rect.Integral();
            }

            var bitmap = new CGBitmapContext(
                             null, 
                             newSize.Item1, 
                             newSize.Item2, 
                             image.CGImage.BitsPerComponent,
                             0,
                             image.CGImage.ColorSpace,
                             image.CGImage.BitmapInfo);
            using (bitmap)
            {
                bitmap.ConcatCTM(TransformForOrientation(image, newSize.Item1, newSize.Item2));
                bitmap.InterpolationQuality = CGInterpolationQuality.High;
                bitmap.DrawImage(rect, image.CGImage);

                return new UIImage(bitmap.ToImage());
            }
        }

        private static CGAffineTransform TransformForOrientation(UIImage image, float width, float height)
        { 
            var transform = CGAffineTransform.MakeIdentity();

            switch (image.Orientation) {
                case UIImageOrientation.Down:
                case UIImageOrientation.DownMirrored:
                    // EXIF = 3 / 4
                    transform = CGAffineTransform.Translate(transform, width, height);
                    transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
                    break;
                case UIImageOrientation.Left:
                case UIImageOrientation.LeftMirrored:
                    // EXIF = 6 / 5
                    transform = CGAffineTransform.Translate(transform, width, 0);
                    transform = CGAffineTransform.Rotate(transform, (float)Math.PI * 2);
                    break;
                case UIImageOrientation.Right:
                case UIImageOrientation.RightMirrored:
                    // EXIF = 8 / 7
                    transform = CGAffineTransform.Translate(transform, 0, height);
                    transform = CGAffineTransform.Rotate(transform, -(float)Math.PI * 2);
                    break;
                default:
                    break;
            }

            switch(image.Orientation) {
                case UIImageOrientation.UpMirrored:
                case UIImageOrientation.DownMirrored:
                    // EXIF = 2 / 4
                    transform = CGAffineTransform.Translate(transform, width, 0);
                    transform = CGAffineTransform.Scale(transform, -1, 1);
                    break;
                case UIImageOrientation.LeftMirrored:
                case UIImageOrientation.RightMirrored:
                    // EXIF = 5 / 7
                    transform = CGAffineTransform.Translate(transform, height, 0);
                    transform = CGAffineTransform.Scale(transform, -1, 1);
                    break;
                default:
                    break;
            }

            return transform;
        }

        private static Stream GetStream(UIImage image)
        {
            using (image)
            {
                return image.AsJPEG(0.8f).AsStream();
            }
        }

        public Task<string> SaveImageToLibrary(Stream image, string filename)
        {
            if (image == null) return Task.FromResult<string>(null);

            var tcs = new TaskCompletionSource<string>();
            using (var uiImage = new UIImage(NSData.FromStream(image)))
            {
                uiImage.SaveToPhotosAlbum((img, err) => tcs.TrySetResult(err != null ? null : filename));
            }
            return tcs.Task;
        }

        private AVAudioPlayer _audioPlayer;
        private AVAudioRecorder _audioRecorder;

        public Task PlaySound(string file)
        {
            StopSound();

            var tcs = new TaskCompletionSource<bool>();
            _audioPlayer = AVAudioPlayer.FromUrl(NSUrl.FromFilename(file));
            _audioPlayer.NumberOfLoops = 1;
            _audioPlayer.Volume = 1.0f;
            _audioPlayer.DecoderError += (sender, e) =>
            {
                tcs.TrySetResult(false);
            };
            _audioPlayer.FinishedPlaying += (sender, e) =>
            {
                tcs.TrySetResult(true);
            };
            _audioPlayer.PrepareToPlay();
            _audioPlayer.Play();
            return tcs.Task;
        }

        public void StopSound()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
        }

        public void BeginCaptureAudio()
        {
            NSError error;
            var settings = new AudioSettings { AudioQuality = AVAudioQuality.Medium };
            
            _audioRecorder = AVAudioRecorder.Create(new NSUrl($"record-{Guid.NewGuid().ToString()}.wav"), settings, out error);
            if (_audioRecorder != null && _audioRecorder.PrepareToRecord())
            {
                _audioRecorder.Record();
            }
            else
            {
                _audioRecorder = null;
            }
        }

        public Stream EndCaptureAudio()
        {
            if (_audioRecorder == null) return null;

            _audioRecorder.Stop();

            var path = _audioRecorder.Url.Path;

            return new DelegateStream(
                () => File.OpenRead(path), 
                () => File.Delete(path));
        }
    }
}
