using System;
using System.IO;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Models.ResponseContent
{
    /// <summary>
    /// Represents the content (body) of an HTTP response.
    /// </summary>
    public interface IHttpResponseContent : IDisposable
    {
        /// <summary>
        /// Sets the headers related with this content.
        /// This method is called by the <see cref="IHttpServer"/> when headers are about to be sent to the client.
        /// </summary>
        /// <param name="headers"></param>
        public void FillHeaders(IHttpResponseHeaders headers);

        /// <summary>
        /// Writes the HTTP content (body).
        /// This method is called by the <see cref="IHttpServer"/> when body is to be sent to the client.
        /// </summary>
        /// <param name="stream">The http output stream</param>
        /// <returns></returns>
        void CopyToStream(Stream stream);
    }
}