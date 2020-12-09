using System;
using System.IO;

namespace ElMariachi.Http.Server.Test.TestUtils
{
    public class NetworkStreamEmulator : Stream
    {
        private readonly Stream _stream;

        public NetworkStreamEmulator(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public int MaxBytesPerRead { get; set; } = 1;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (MaxBytesPerRead <= 0)
                return MaxBytesPerRead;

            var max = Math.Min(count, MaxBytesPerRead);

            return _stream.Read(buffer, offset, max);
        }


        public override void Flush()
        {
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("NotImplementedException");
        }


        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => throw new Exception("NotImplementedException");

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
