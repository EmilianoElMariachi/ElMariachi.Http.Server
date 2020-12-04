using System;
using System.IO;
using ElMariachi.Http.Exceptions;

namespace ElMariachi.Http.Server.Streams.Input
{

    /// <summary>
    /// Read only stream throwing an exception if the number of bytes read is greater than the specified limit
    /// </summary>
    /// <exception cref="StreamLimitException"></exception>
    /// <exception cref="StreamEndException"></exception>
    public class LimiterInputStream : ReadOnlyStream
    {
        private readonly Stream _stream;
        private readonly int _limit;
        private int _nbRemainingBytes = 0;

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="limit">The maximum number of bytes which can be read without throwing exception. Limit can't be negative.</param>
        public LimiterInputStream(Stream stream, int limit)
        {
            if (limit < 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit can't be negative.");

            _stream = stream;
            _limit = limit;
            _nbRemainingBytes = limit;
        }

        public int NbRemainingBytes => _nbRemainingBytes;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            var countMax = Math.Min(count, _nbRemainingBytes + 1);
            var nbBytesRead = _stream.Read(buffer, offset, countMax);
            if (nbBytesRead <= 0)
                throw new StreamEndException();

            _nbRemainingBytes -= nbBytesRead;
            if (_nbRemainingBytes < 0)
                throw new StreamLimitException(_limit);

            return nbBytesRead;
        }
    }
}