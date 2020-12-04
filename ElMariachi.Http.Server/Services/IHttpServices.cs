namespace ElMariachi.Http.Server.Services
{
    public interface IHttpServices
    {
        IHttpInputStreamDecodingStrategy InputStreamDecodingStrategy { get; set; }

        IDefaultResponseHeadersSetter DefaultResponseHeadersSetter { get; set; }

        IHttpHeaderReader HeaderReader { get; set; }

        IHttpHeadersSerializer HeadersSerializer { get; set; }
    }
}