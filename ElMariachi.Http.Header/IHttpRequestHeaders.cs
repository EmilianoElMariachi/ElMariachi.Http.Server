using ElMariachi.Http.Header.Managed;

namespace ElMariachi.Http.Header
{
    public interface IHttpRequestHeaders : IHttpHeaders
    {
        IConnectionHeader Connection { get; }

        ITransferEncodingHeader TransferEncoding { get; }

        IContentTypeHeader ContentType { get; }

        IContentLengthHeader ContentLength { get; }

        IRangeHeader Range { get; }

        string? UserAgent { get; set; }

        string? Host { get; }
    }
}