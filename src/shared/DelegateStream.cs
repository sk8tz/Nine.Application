namespace System.IO
{
    class DelegateStream : Stream
    {
        private Stream _stream;
        private readonly Func<Stream> _open;
        private readonly Action _dispose;

        public DelegateStream(Func<Stream> open, Action dispose = null)
        {
            _stream = open();
            _dispose = dispose;
            _open = open;
        }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public override bool CanSeek => true;
        public override bool CanRead => _stream.CanRead;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override void Flush() => _stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override void SetLength(long value) => _stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                if (_stream.CanSeek)
                {
                    return _stream.Seek(offset, origin);
                }
            }
            catch (NotSupportedException) when (offset == 0 && origin == SeekOrigin.Begin) { }

            _stream.Dispose();
            _stream = _open();

            return 0;
        }

        protected override void Dispose(bool disposing)
        {
            _stream.Dispose();
            _dispose?.Invoke();
            base.Dispose(disposing);
        }
    }
}
