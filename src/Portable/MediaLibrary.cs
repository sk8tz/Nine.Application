namespace Nine.Application
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public partial class MediaLibrary : IMediaLibrary
    {
        private static Tuple<int, int> Crop(int width, int height, int maxSize = 1024)
        {
            var ratio = 1.0 * width / height;
            var w = Math.Min(width, maxSize);
            var h = Math.Min(height, maxSize);

            w = Math.Min(w, (int)(h * ratio));
            h = Math.Min(h, (int)(w / ratio));

            return Tuple.Create(w, h);
        }

        // http://stackoverflow.com/questions/1622307/sample-rate-for-iphone-audio-recording
        private const int DefaultAudioSamplingRate = 11025;

        /// <summary>
        /// http://damianblog.com/2011/02/07/storing-wp7-recorded-audio-as-wav-format-streams/
        /// </summary>
        private void WriteWavHeader(Stream stream, int sampleRate)
        {
            const int bitsPerSample = 16;
            const int bytesPerSample = bitsPerSample / 8;
            var encoding = System.Text.Encoding.UTF8;

            // ChunkID Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
            stream.Write(encoding.GetBytes("RIFF"), 0, 4);

            // NOTE this will be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);

            // Format Contains the letters "WAVE"(0x57415645 big-endian form).
            stream.Write(encoding.GetBytes("WAVE"), 0, 4);

            // Subchunk1ID Contains the letters "fmt " (0x666d7420 big-endian form).
            stream.Write(encoding.GetBytes("fmt "), 0, 4);

            // Subchunk1Size 16 for PCM.  This is the size of therest of the Subchunk which follows this number.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // AudioFormat PCM = 1 (i.e. Linear quantization) Values other than 1 indicate some form of compression.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // NumChannels Mono = 1, Stereo = 2, etc.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // SampleRate 8000, 44100, etc.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // ByteRate =  SampleRate * NumChannels * BitsPerSample/8
            stream.Write(BitConverter.GetBytes(sampleRate * bytesPerSample), 0, 4);

            // BlockAlign NumChannels * BitsPerSample/8 The number of bytes for one sample including all channels.
            stream.Write(BitConverter.GetBytes((short)(bytesPerSample)), 0, 2);

            // BitsPerSample    8 bits = 8, 16 bits = 16, etc.
            stream.Write(BitConverter.GetBytes((short)(bitsPerSample)), 0, 2);

            // Subchunk2ID Contains the letters "data" (0x64617461 big-endian form).
            stream.Write(encoding.GetBytes("data"), 0, 4);

            // NOTE to be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);
        }

        private void UpdateWavHeader(Stream stream)
        {
            if (!stream.CanSeek) throw new Exception("Can't seek stream to update wav header");

            var oldPos = stream.Position;

            // ChunkSize  36 + SubChunk2Size
            stream.Seek(4, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);

            // Subchunk2Size == NumSamples * NumChannels * BitsPerSample/8 This is the number of bytes in the data.
            stream.Seek(40, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);

            stream.Seek(oldPos, SeekOrigin.Begin);
        }

#if PCL
        public Task<Stream> PickImage(bool showCamera = true, int maxSize = int.MaxValue) => Task.FromResult<Stream>(null);
        public Task SaveImageToLibrary(Stream image, string filename) => Task.FromResult(0);
        public Task PlaySound(string uri) => Task.FromResult(0);
        public void StopSound() { }
        public void BeginCaptureAudio() { }
        public Stream EndCaptureAudio() => null;
#endif
    }
}
