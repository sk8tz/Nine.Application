namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    [Flags]
    public enum ImageLocation
    {
        Library = 1,
        Camera = 2,
        All = Library | Camera,
    }

    public interface IMediaLibrary
    {
        Task<Stream> PickImage(ImageLocation location = ImageLocation.All, int maxSize = int.MaxValue);

        Task<string> SaveImageToLibrary(Stream image, string filename);

        Task PlaySound(string uri);

        void StopSound();

		Task<bool> BeginCaptureAudio();

        Stream EndCaptureAudio();
    }
}
