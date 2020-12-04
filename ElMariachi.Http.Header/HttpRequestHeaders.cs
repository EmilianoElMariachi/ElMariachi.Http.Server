using ElMariachi.Http.Header.Managed;

namespace ElMariachi.Http.Header
{

    public class HttpRequestHeaders : HttpHeaders, IHttpRequestHeaders
    {

        private readonly ConnectionHeader _connectionHeader = new ConnectionHeader();
        private readonly TransferEncodingHeader _transferEncodingHeader = new TransferEncodingHeader();
        private readonly ContentLengthHeader _contentLengthHeader = new ContentLengthHeader();
        private readonly ContentTypeHeader _contentTypeHeader = new ContentTypeHeader();
        private readonly RangeHeader _rangeHeader = new RangeHeader();

        public HttpRequestHeaders()
        {
            AddHeader(_connectionHeader);
            AddHeader(_transferEncodingHeader);
            AddHeader(_contentLengthHeader);
            AddHeader(_contentTypeHeader);
            AddHeader(_rangeHeader);
        }

        public IConnectionHeader Connection => _connectionHeader;

        public ITransferEncodingHeader TransferEncoding => _transferEncodingHeader;

        public IContentTypeHeader ContentType => _contentTypeHeader;

        public IContentLengthHeader ContentLength => _contentLengthHeader;

        public IRangeHeader Range => _rangeHeader;

        public string? Host => this[HttpConst.Headers.Host];

        public string? UserAgent
        {
            get => this[HttpConst.Headers.UserAgent];
            set => this[HttpConst.Headers.UserAgent] = value;
        }

    }
}