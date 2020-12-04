using System;
using System.IO;

namespace ElMariachi.Http.Server.Streams.Input
{
    public abstract class ReadOnlyStream : Stream
    {

        public sealed override bool CanRead => true;

        public override bool CanSeek => false;

        public sealed override bool CanWrite => false;

        public override long Length => throw new NotSupportedException($"Getting the length of a {GetType().Name} is not permitted.");

        public override long Position
        {
            get => throw new NotSupportedException($"Getting the position of a {GetType().Name} is not permitted.");
            set => throw new NotSupportedException($"Setting the position of a {GetType().Name} is not permitted.");
        }

        public sealed override void Flush()
        {
            throw new NotSupportedException($"Flushing a {GetType().Name} is not permitted.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"Seeking in a {GetType().Name} is not permitted.");
        }

        public sealed override void SetLength(long value)
        {
            throw new NotSupportedException($"Setting the length of a {GetType().Name} is not permitted.");
        }

        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"Writing to a {GetType().Name} is not permitted.");
        }

    }
}
