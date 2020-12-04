using System.Text;

namespace ElMariachi.Http.Header.Utils
{
    public class HttpHeadersSerializer : IHttpHeadersSerializer
    {
        public string Serialize(IHttpHeaders headers)
        {
            return SerializeHeaders(headers);
        }

        public static string SerializeHeaders(IHttpHeaders headers)
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
