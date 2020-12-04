using System.Text;

namespace ElMariachi.Http
{
    public static class HttpConst
    {
        public static readonly string LineReturn = "\r\n";
        public static readonly byte[] LineReturnBytes = Encoding.ASCII.GetBytes(LineReturn);

        public const char DefaultHeaderDelimiter = ',';

        public static class Headers
        {
            #region Common Headers

            public const string ContentLength = "Content-Length";

            public const string ContentType = "Content-Type";

            public const string TransferEncoding = "Transfer-Encoding";

            public const string Connection = "Connection";

            public const string Server = "Server";

            public const string Date = "Date";

            public const string Host = "Host";

            public const string UserAgent = "User-Agent";

            public const string Range = "Range";

            public const string ContentRange = "Content-Range";

            #endregion  
        }
    }
}
