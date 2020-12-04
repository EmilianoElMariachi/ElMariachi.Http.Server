using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Models;
using ElMariachi.Http.Server.Streams.Input;
using ElMariachi.Http.Server.Streams.Output;

namespace ElMariachi.Http.Server.Services.Internals
{
    internal class HttpResponseSender : IHttpResponseSender
    {
        private readonly NetworkStream _networkStream;
        private readonly int _maxInputStreamCleaning;
        private readonly object _lock = new object();

        public HttpResponseSender(NetworkStream networkStream, int maxInputStreamCleaning)
        {
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
            _maxInputStreamCleaning = maxInputStreamCleaning;
        }

        public bool IsResponseSent { get; private set; }

        public event ResponseSentHandler? ResponseSent;

        public IHttpResponse? SentResponse { get; private set; }

        public bool CloseConnection { get; private set; }

        public void Send(IHttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var status = response.Status;
            if (status == null)
                throw new ArgumentException($"{nameof(IHttpResponse)}.{nameof(IHttpResponse.Status)} can't be null.");

            var headers = response.Headers;
            if (headers == null)
                throw new ArgumentException($"{nameof(IHttpResponse)}.{nameof(IHttpResponse.Headers)} can't be null.");

            var content = response.Content;
            if (content == null)
                throw new ArgumentException($"{nameof(IHttpResponse)}.{nameof(IHttpResponse.Content)} can't be null.");

            content.FillHeaders(headers);

            lock (_lock)
            {
                if (IsResponseSent)
                    throw new InvalidOperationException("A response was already sent for this request.");

                HttpServices.Instance.DefaultResponseHeadersSetter.Set(headers);

                CloseConnection = headers.Connection.Close;
                SentResponse = response;
                IsResponseSent = true;
            }

            try
            {
                EmptyInputStream(_networkStream);

                SendResponseHeader(status, headers, _networkStream);

                if (!string.Equals(response.Request?.Method, "HEAD"))
                    content.CopyToStream(new HttpOutputStream(_networkStream));

                NotifyResponseSent();
            }
            catch (Exception)
            {
                CloseConnection = true;
                throw;
            }
            finally
            {
                content.Dispose();
            }
        }

        private static void SendResponseHeader(HttpStatus status, IHttpHeaders headers, Stream outputStream)
        {
            var httpHeader = HttpServer.SupportedHttpVersion + " " + status.Code + " " + status.Description + HttpConst.LineReturn + HttpServices.Instance.HeadersSerializer.Serialize(headers) + HttpConst.LineReturn;
            outputStream.Write(Encoding.ASCII.GetBytes(httpHeader));
        }

        private void EmptyInputStream(NetworkStream networkStream)
        {
            var cleanLimit = _maxInputStreamCleaning;
            if (cleanLimit == 0)
                return;

            try
            {
                var inputStream = cleanLimit < 0 ? (Stream)networkStream : new LimiterInputStream(networkStream, cleanLimit);
                var buffer = new byte[1024 * 1024 * 1 /*1 MiB*/];

                while (networkStream.DataAvailable)
                {
                    var read = inputStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        throw new StreamEndException();

                }
            }
            catch (StreamLimitException)
            {
            }
        }

        private void NotifyResponseSent()
        {
            ResponseSent?.Invoke(this, new ResponseSentHandlerArgs());
        }
    }
}