using System.Text;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    internal class HttpHeadersSerializer : IHttpHeadersSerializer
    {
        public string Serialize(IHttpHeaders headers)
        {
            var sb = new StringBuilder();
            foreach (var (header, value) in headers)
            {
                sb.Append(header + ": " + value + "\r\n");
            }
            return sb.ToString();
        }
    }
}
