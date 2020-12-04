using System.IO;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Models.ResponseContent
{
    public class EmptyResponseContent : IHttpResponseContent
    {
        public void FillHeaders(IHttpResponseHeaders headers)
        {
            headers.ContentLength.Value = 0;
        }

        public void CopyToStream(Stream stream)
        {
        }

        public void Dispose()
        {
        }
    }
}
