﻿namespace Nine.Application
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
        private readonly Func<UIViewController> _viewController;
        
        public MediaLibrary() { }
        public MediaLibrary(Func<UIViewController> viewController) { _viewController = viewController; }
        
        public Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue)
        {
            var controller = _viewController?.Invoke() ?? ApplicationView.ViewController;

            var sourceType = location == ImageLocation.Camera 
                ? UIImagePickerControllerSourceType.Camera
                : UIImagePickerControllerSourceType.PhotoLibrary;
            
            var tcs = new TaskCompletionSource<Stream>();
            var picker = new UIImagePickerController { SourceType = sourceType, AllowsEditing = false };

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
            var newSize = MediaHelper.Crop((int)image.Size.Width, (int)image.Size.Height, maxSize);

            var rect = new CGRect(0, 0, newSize.Item1, newSize.Item2);
            rect = rect.Integral();

            UIGraphics.BeginImageContextWithOptions(new CGSize(rect.Width, rect.Height), false, image.CurrentScale);
            image.Draw(rect);
            var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resizedImage;
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
		private TaskCompletionSource<bool> _audioPlayingTcs;

        public Task PlaySound(string file)
        {
            StopSound();

			AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
			AVAudioSession.SharedInstance().SetActive(true);

            var tcs = new TaskCompletionSource<bool>();
			_audioPlayer = AVAudioPlayer.FromUrl(NSUrl.FromFilename(file));
            _audioPlayer.DecoderError += (sender, e) =>
			{
                tcs.TrySetResult(false);
            };
            _audioPlayer.FinishedPlaying += (sender, e) =>
			{
                tcs.TrySetResult(true);
            };
			if (!_audioPlayer.Play())
			{
				tcs.TrySetResult(false);
			}
			_audioPlayingTcs = tcs;
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

			if (_audioPlayingTcs != null)
			{
				_audioPlayingTcs.TrySetResult(true);
				_audioPlayingTcs = null;
			}
        }

		public async Task<bool> BeginCaptureAudio()
        {
			var tcs = new TaskCompletionSource<bool> ();
			AVAudioSession.SharedInstance ().RequestRecordPermission(granted => tcs.TrySetResult (granted));
			if (!await tcs.Task) return false;

			AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Record);
			AVAudioSession.SharedInstance().SetActive(true);

            NSError error;
			var settings = new AudioSettings 
			{
				Format = AudioFormatType.LinearPCM,
				SampleRate = 8000,
				NumberChannels = 1,
				LinearPcmBitDepth = 16,
				LinearPcmFloat = false,
				LinearPcmBigEndian = false,
			};
			var recordFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
			_audioRecorder = AVAudioRecorder.Create(NSUrl.FromFilename(recordFile), settings, out error);
            if (_audioRecorder != null && _audioRecorder.PrepareToRecord())
            {
                _audioRecorder.Record();
            }
            else
            {
                _audioRecorder = null;
				return false;
            }
			return true;
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
