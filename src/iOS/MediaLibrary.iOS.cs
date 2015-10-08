namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UIKit;

    public partial class MediaLibrary : IMediaLibrary
    {
        public async Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue)
        {
            if (!location.HasFlag(ImageLocation.Camera))
            {
                return await PickImage(UIImagePickerControllerSourceType.PhotoLibrary, maxSize);
            }

            var owner = ApplicationView.Current.View;
            var tcs = new TaskCompletionSource<int?>();
            var view = new UIActionSheet() { TintColor = UIWindow.Appearance.TintColor };

            // TODO: localization
            view.AddButton("Take Photo");
            view.AddButton("Choose From Library");
            view.AddButton("Cancel");
            view.CancelButtonIndex = 2;
            view.Clicked += (sender, e) => { tcs.TrySetResult((int)e.ButtonIndex); };
            view.ShowInView(owner);

            var index = await tcs.Task;
            if (index == 0)
            {
                return await PickImage(UIImagePickerControllerSourceType.Camera, maxSize);
            }
            if (index == 1)
            {
                return await PickImage(UIImagePickerControllerSourceType.PhotoLibrary, maxSize);
            }
            return null;
        }

        private static Task<Stream> PickImage(UIImagePickerControllerSourceType type, int maxSize)
        {
            var view = ApplicationView.Current;
            if (view == null) return Task.FromResult<Stream>(null);

            var tcs = new TaskCompletionSource<Stream>();
            var picker = new UIImagePickerController { SourceType = type, AllowsEditing = false, };

            // TODO: Resize and compress
            picker.Canceled += (sender, e) => { tcs.TrySetResult(null); picker.DismissViewController(true, null); };
            picker.FinishedPickingMedia += (sender, e) => { tcs.TrySetResult(GetStream(e.EditedImage ?? e.OriginalImage)); picker.DismissViewController(true, null); };
            picker.FinishedPickingImage += (sender, e) => { tcs.TrySetResult(GetStream(e.Image)); picker.DismissViewController(true, null); };

            view.PresentViewController(picker, true, null);
            return tcs.Task;
        }

        private static Stream GetStream(UIImage uIImage)
        {
            return new MemoryStream(uIImage.AsPNG().ToArray());
        }

        public Task<string> SaveImageToLibrary(Stream image, string filename)
        {
            throw new NotImplementedException();
        }

        public Task PlaySound(string uri)
        {
            throw new NotImplementedException();
        }

        public void StopSound()
        {
            throw new NotImplementedException();
        }

        public void BeginCaptureAudio()
        {
            throw new NotImplementedException();
        }

        public Stream EndCaptureAudio()
        {
            throw new NotImplementedException();
        }
    }
}
