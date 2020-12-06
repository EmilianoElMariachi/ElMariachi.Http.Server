using System;
using System.Net.Sockets;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Server.Services.Internals;

namespace ElMariachi.Http.Server.Streams.Input
{
    internal class HttpInputStream : ReadOnlyStream
    {
        private readonly IHttpResponseSender _httpResponseSender;
        private readonly NetworkStream _networkStream;
        private bool _atLeastOneByteRead = false;

        public event AtLeastOneByteReadHandler? AtLeastOneByteRead;

        public HttpInputStream(NetworkStream networkStream, IHttpResponseSender httpResponseSender)
        {
            _httpResponseSender = httpResponseSender ?? throw new ArgumentNullException(nameof(httpResponseSender));
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_httpResponseSender.IsResponseSent)
                throw new InvalidOperationException("Response is already sent.");

            var nbBytesRead = _networkStream.Read(buffer, offset, count);
            if (nbBytesRead <= 0)
                throw new StreamEndException();

            if (!_atLeastOneByteRead)
            {
                _atLeastOneByteRead = true;
                NotifyAtLeastOneByteRead();
            }

            return nbBytesRead;
        }

        public sealed override void Close()
        {
            // NOTE: not testable but do not close the NetworkStream!
        }

        private void NotifyAtLeastOneByteRead()
        {
            AtLeastOneByteRead?.Invoke();
        }
    }

    internal delegate void AtLeastOneByteReadHandler();
}