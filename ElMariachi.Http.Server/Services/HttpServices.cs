using System;

namespace ElMariachi.Http.Server.Services
{
    public class HttpServices : IHttpServices
    {
        private IHttpInputStreamDecodingStrategy _inputStreamDecodingStrategy = new HttpInputStreamDecodingStrategy();
        private IDefaultResponseHeadersSetter _defaultResponseHeadersSetter = new DefaultResponseHeadersSetter();
        private IHttpHeaderReader _headerReader = new HttpHeaderReader();
        private IHttpHeadersSerializer _headersSerializer = new HttpHeadersSerializer();

        private HttpServices()
        {
        }

        public IHttpInputStreamDecodingStrategy InputStreamDecodingStrategy
        {
            get => _inputStreamDecodingStrategy;
            set => _inputStreamDecodingStrategy = value ?? throw new ArgumentNullException(nameof(InputStreamDecodingStrategy));
        }

        public IDefaultResponseHeadersSetter DefaultResponseHeadersSetter
        {
            get => _defaultResponseHeadersSetter;
            set => _defaultResponseHeadersSetter = value ?? throw new ArgumentNullException(nameof(DefaultResponseHeadersSetter));
        }

        public IHttpHeaderReader HeaderReader
        {
            get => _headerReader;
            set => _headerReader = value ?? throw new ArgumentNullException(nameof(HeaderReader));
        }

        public IHttpHeadersSerializer HeadersSerializer
        {
            get => _headersSerializer;
            set => _headersSerializer = value ?? throw new ArgumentNullException(nameof(HeadersSerializer));
        }

        public static IHttpServices Instance { get; } = new HttpServices();
    }
}