using System;

namespace ElMariachi.Http.Header
{
    public interface IHttpHeader
    {
        string HttpVersion { get; }

        string Method { get; }

        Uri RequestUri { get; }

        IHttpRequestHeaders Headers { get; }
    }
}