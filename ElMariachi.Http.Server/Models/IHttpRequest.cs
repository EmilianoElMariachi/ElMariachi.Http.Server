using System;
using System.IO;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Models
{
    public interface IHttpRequest
    {
        /// <summary>
        /// The request method (always uppercase)
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Get the raw request URI.
        /// This is the raw value emitted by the client.
        /// It can be relative or absolute.
        /// </summary>
        Uri RawRequestUri { get; }

        /// <summary>
        /// The absolute Uri
        /// </summary>
        Uri AbsRequestUri { get; }

        string HttpVersion { get; }

        IHttpRequestHeaders Headers { get; }

        Stream InputStream { get; }

        public bool IsResponseSent { get; }

        void SendResponse(IHttpResponse response);
    }
}