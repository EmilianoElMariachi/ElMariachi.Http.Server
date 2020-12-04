using System;
using System.IO;
using ElMariachi.Http.Exceptions;

namespace ElMariachi.Http.Server.Streams.Input
{
    public class FixLengthInputStream : ReadOnlyStream
    {
        private readonly Stream _stream;
        private long _position = 0;
        private long _nbRemainingBytesToRead;

        public FixLengthInputStream(Stream stream, long contentLength)
        {
            _stream = stream;
            Length = contentLength;
            _nbRemainingBytesToRead = contentLength;
        }

        public long NbRemainingBytesToRead => Length - _position;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            var maxBytesAvailable = Math.Min(NbRemainingBytesToRead, count);

            if (maxBytesAvailable <= 0)
                return 0;

            var nbBytesRead = _stream.Read(buffer, offset, (int)maxBytesAvailable);
            if (nbBytesRead <= 0)
                throw new StreamEndException();

            _position += nbBytesRead;

            return nbBytesRead;
        }

        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }
    }

}