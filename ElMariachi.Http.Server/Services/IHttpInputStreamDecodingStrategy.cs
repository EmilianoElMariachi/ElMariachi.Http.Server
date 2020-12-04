using System.IO;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    public interface IHttpInputStreamDecodingStrategy
    {
        /// <summary>
        /// Creates a stream in charge of decoding the raw encoding stream.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        Stream Create(Stream inputStream, IHttpHeader header);
    }
}