namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public partial class MediaLibrary : IMediaLibrary
    {
        public void BeginCaptureAudio()
        {
            throw new NotImplementedException();
        }

        public Stream EndCaptureAudio()
        {
            throw new NotImplementedException();
        }

        public Task PlaySound(string uri)
        {
            throw new NotImplementedException();
        }

        public void PlaySoundName(string name)
        {
            throw new NotImplementedException();
        }

        public Task SaveImageToLibrary(Stream image, string filename)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public void StopSound()
        {
            throw new NotImplementedException();
        }
    }
}
