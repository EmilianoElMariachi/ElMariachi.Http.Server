using System.IO;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    public interface IHttpHeaderReader
    {
        /// <summary>
        /// Reads all the HTTP header (method, request URI, headers)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="maxMethodNameSize"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxRequestUriSize"></param>
        /// <returns></returns>
        /// <exception cref="StreamLimitException"></exception>
        /// <exception cref="RequestFormatException"></exception>
        /// <exception cref="RequestUriToolLongException"></exception>
        public IHttpHeader Read(Stream stream, int maxMethodNameSize, int maxHeaderSize, int maxRequestUriSize);
    }
}