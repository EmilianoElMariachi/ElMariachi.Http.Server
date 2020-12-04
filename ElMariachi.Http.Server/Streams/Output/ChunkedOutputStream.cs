using System;
using System.IO;
using System.Text;

namespace ElMariachi.Http.Server.Streams.Output
{
    public class ChunkedOutputStream : WriteOnlyStream
    {
        private static readonly byte[] _chunkEndBytes = Encoding.ASCII.GetBytes("0" + HttpConst.LineReturn + HttpConst.LineReturn);
        private bool _closed = false;

        private readonly Stream _outputStream;

        public ChunkedOutputStream(Stream outputStream)
        {
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var chunkSize = Encoding.ASCII.GetBytes(count.ToString("X"));
            _outputStream.Write(chunkSize);
            _outputStream.Write(HttpConst.LineReturnBytes);
            _outputStream.Write(buffer, offset, count);
            _outputStream.Write(HttpConst.LineReturnBytes);
        }

        public override void Close()
        {
            if (_closed)
                return;
            try
            {
                base.Close();
                WriteFinalChunk();
                _outputStream.Close();
            }
            finally
            {
                _closed = true;
            }
        }

        private void WriteFinalChunk()
        {
            _outputStream.Write(_chunkEndBytes);
        }
    }

}