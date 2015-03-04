namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IMediaLibrary
    {
        Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue);

        Task SaveImageToLibrary(Stream image, string filename);

        Task PlaySound(string uri);

        void StopSound();

        void BeginCaptureAudio();

        Stream EndCaptureAudio();
    }
}
