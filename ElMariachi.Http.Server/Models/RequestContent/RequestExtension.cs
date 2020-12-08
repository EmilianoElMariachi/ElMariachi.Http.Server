using System;
using System.Text;

namespace ElMariachi.Http.Server.Models.RequestContent
{
    public static class RequestExtension
    {
        public static string ReadContentAsString(this IHttpRequest request)
        {
            var contentLength = request.Headers.ContentLength.Value;
            if (contentLength == null)
                throw new NotSupportedException($"Reading request content as string is not supported when «{request.Headers.ContentLength.Name}» header is not defined.");

            var bytes = new byte[contentLength.Value];

            var nbBytesRead = request.InputStream.Read(bytes);

            var encoding = request.Headers.ContentType.Charset ?? Encoding.UTF8;
            return encoding.GetString(bytes, 0, nbBytesRead);
        }
    }
}
