using System;
using System.Net.Sockets;

namespace ElMariachi.Http.Server.Streams.Output
{
    internal class HttpOutputStream : WriteOnlyStream
    {

        public HttpOutputStream(NetworkStream networkStream)
        {
            NetworkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
        }

        internal NetworkStream NetworkStream { get; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            NetworkStream.Write(buffer, offset, count);
        }

        public sealed override void Close()
        {
        }
    }
}
