using System;

namespace ElMariachi.Http.Header
{
    public class HttpHeader : IHttpHeader
    {
        public string HttpVersion { get; set; }

        public string Method { get; set; }

        public Uri RequestUri { get; set; }

        public IHttpRequestHeaders Headers { get; set; }
    }
}