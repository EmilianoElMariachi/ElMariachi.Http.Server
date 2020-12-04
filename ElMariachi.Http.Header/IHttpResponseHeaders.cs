using ElMariachi.Http.Header.Managed;

namespace ElMariachi.Http.Header
{
    public interface IHttpResponseHeaders : IHttpHeaders
    {

        IContentRangeHeader ContentRange { get; }

        IConnectionHeader Connection { get; }

        ITransferEncodingHeader TransferEncoding { get; }

        IContentTypeHeader ContentType { get; }

        IContentLengthHeader ContentLength { get; }

        IDateHeader Date { get; }

        string? Server { get; set; }

    }
}