using System;
using System.IO;

namespace ElMariachi.Http.Server.Streams.Output
{
    public abstract class WriteOnlyStream : Stream
    {

        public sealed override bool CanRead => false;

        public sealed override bool CanSeek => false;

        public sealed override bool CanWrite => true;

        public sealed override long Length => throw new NotSupportedException($"Getting the length of a {GetType().Name} is not permitted.");

        public sealed override long Position
        {
            get => throw new NotSupportedException($"Getting the position of a {GetType().Name} is not permitted.");
            set => throw new NotSupportedException($"Setting the position of a {GetType().Name} is not permitted.");
        }

        public sealed override void Flush()
        {
            throw new NotSupportedException($"Flushing a {GetType().Name} is not permitted.");
        }

        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"Reading a {GetType().Name} is not permitted.");
        }

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"Seeking in a {GetType().Name} is not permitted.");
        }

        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException($"Setting the length of a {GetType().Name} is not permitted.");
        }

    }
}
