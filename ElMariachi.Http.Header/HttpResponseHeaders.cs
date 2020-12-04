using ElMariachi.Http.Header.Managed;

namespace ElMariachi.Http.Header
{
    public class HttpResponseHeaders : HttpHeaders, IHttpResponseHeaders
    {
        private readonly ConnectionHeader _connectionHeader = new ConnectionHeader();
        private readonly TransferEncodingHeader _transferEncodingHeader = new TransferEncodingHeader();
        private readonly ContentLengthHeader _contentLengthHeader = new ContentLengthHeader();
        private readonly ContentTypeHeader _contentTypeHeader = new ContentTypeHeader();
        private readonly DateHeader _dateHeader = new DateHeader();
        private readonly ContentRangeHeader _contentRange = new ContentRangeHeader();

        public HttpResponseHeaders()
        {
            AddHeader(_connectionHeader);
            AddHeader(_transferEncodingHeader);
            AddHeader(_contentLengthHeader);
            AddHeader(_contentTypeHeader);
            AddHeader(_dateHeader);
            AddHeader(_contentRange);
        }

        public IConnectionHeader Connection => _connectionHeader;

        public ITransferEncodingHeader TransferEncoding => _transferEncodingHeader;

        public IContentTypeHeader ContentType => _contentTypeHeader;

        public IContentLengthHeader ContentLength => _contentLengthHeader;

        public IDateHeader Date => _dateHeader;

        public IContentRangeHeader ContentRange => _contentRange;

        public string? Server
        {
            get => this[HttpConst.Headers.Server];
            set => this[HttpConst.Headers.Server] = value;
        }
    }
}