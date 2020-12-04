using System;
using System.IO;
using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Services.Internals;

namespace ElMariachi.Http.Server.Models
{
    internal class HttpRequest : IHttpRequest
    {
        private readonly IHttpHeader _header;
        private readonly IHttpResponseSender _responseSender;

        public HttpRequest(IHttpHeader header, Stream inputStream, Uri absRequestUri, IHttpResponseSender httpResponseSender)
        {
            _header = header ?? throw new ArgumentNullException(nameof(header));
            _responseSender = httpResponseSender ?? throw new ArgumentNullException(nameof(httpResponseSender));
            InputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
            AbsRequestUri = absRequestUri ?? throw new ArgumentNullException(nameof(absRequestUri));
        }

        public string Method => _header.Method;

        public Uri RawRequestUri => _header.RequestUri;

        public Uri AbsRequestUri { get; }

        public string HttpVersion => _header.HttpVersion;

        public IHttpRequestHeaders Headers => _header.Headers;

        public Stream InputStream { get; }

        public bool IsResponseSent => _responseSender.IsResponseSent;

        public void SendResponse(IHttpResponse response)
        {
            response.Request = this;
            _responseSender.Send(response);
        }

    }
}