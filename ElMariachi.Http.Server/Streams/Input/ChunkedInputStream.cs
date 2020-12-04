using System;
using System.Globalization;
using System.IO;
using ElMariachi.Http.Exceptions;

namespace ElMariachi.Http.Server.Streams.Input
{
    public class ChunkedInputStream : ReadOnlyStream
    {

        private readonly ChunkBufferHelper _chunkBufferHelper;

        public ChunkedInputStream(Stream stream)
        {
            _chunkBufferHelper = new ChunkBufferHelper(stream);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var nbRemainingBytesToRead = count;
            var nbBytesRead = 0;
            var offsetTemp = offset;

            while (nbRemainingBytesToRead > 0 && !_chunkBufferHelper.AllChunksRead)
            {
                var nbBytesReadFromChunk = _chunkBufferHelper.Read(buffer, offsetTemp, nbRemainingBytesToRead);
                nbBytesRead += nbBytesReadFromChunk;
                offsetTemp += nbBytesReadFromChunk;
                nbRemainingBytesToRead -= nbBytesReadFromChunk;
            }

            return nbBytesRead;
        }

        private class ChunkBufferHelper
        {
            private readonly Stream _stream;
            private byte[]? _chunkBuffer = null;
            private int _index = 0;
            private bool _noMoreChunks = false;
            private int _nbRemainingBytesToRead;

            private static readonly int _maxHexStrLength = int.MaxValue.ToString("X2").Length; // NOTE: The max number of hex char which can fit an int

            public ChunkBufferHelper(Stream stream)
            {
                _stream = stream;
            }

            public int Read(byte[] buffer, in int offset, in int count)
            {
                if (_chunkBuffer == null || _nbRemainingBytesToRead <= 0)
                {
                    ReadNextChunk();
                }

                var nbAvailableRemainingBytesToRead = _chunkBuffer.Length - _index;
                var nbBytesToCopy = Math.Min(count, nbAvailableRemainingBytesToRead);

                Array.Copy(_chunkBuffer, _index, buffer, offset, nbBytesToCopy);

                _nbRemainingBytesToRead -= nbBytesToCopy;

                _index += nbBytesToCopy;

                return nbBytesToCopy;
            }

            private void ReadNextChunk()
            {
                string chunkLengthHex;
                try
                {
                    chunkLengthHex = _stream.ReadUntilNewLine(_maxHexStrLength).Trim();
                }
                catch (StreamLimitException)
                {
                    throw new RequestFormatException($"Chunk length can't be longer than «{_maxHexStrLength}» hexadecimal chars.");
                }

                if (!int.TryParse(chunkLengthHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var chunkLength))
                    throw new RequestFormatException($"Chunk length «{chunkLengthHex}» is not a valid hexadecimal number.");

                if (chunkLength < 0)
                    throw new RequestFormatException($"Chunk length «{chunkLengthHex}» is not allowed to be negative.");

                if (chunkLength == 0)
                {
                    _noMoreChunks = true;
                    _chunkBuffer = new byte[0];
                    _index = 0;
                    _nbRemainingBytesToRead = _chunkBuffer.Length;

                    var c1 = _stream.ReadChar();
                    var c2 = _stream.ReadChar();

                    if (c1 != '\r' || c2 != '\n')
                        throw new RequestFormatException("Last chunk of length «0» is not terminated with expected final sequence «\\r\\n».");
                }
                else
                {
                    var chunkBuffer = new byte[chunkLength];

                    var nbRemainingBytesToRead = chunkLength;
                    var buffOffset = 0;
                    do
                    {
                        var nbBytesRead = _stream.Read(chunkBuffer, buffOffset, nbRemainingBytesToRead);
                        if (nbBytesRead <= 0)
                            throw new StreamEndException();

                        nbRemainingBytesToRead -= nbBytesRead;
                        buffOffset += nbBytesRead;
                    } while (nbRemainingBytesToRead > 0);

                    var c1 = _stream.ReadChar();
                    var c2 = _stream.ReadChar();

                    if (c1 != '\r' || c2 != '\n')
                        throw new RequestFormatException($"Chunk of length «{chunkLength}» is not followed by expected sequence «\\r\\n».");

                    _chunkBuffer = chunkBuffer;
                    _index = 0;
                    _nbRemainingBytesToRead = _chunkBuffer.Length;
                }
            }

            public bool AllChunksRead => _noMoreChunks && _nbRemainingBytesToRead <= 0;

        }

    }

}