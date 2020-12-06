using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElMariachi.Http.Server.Services
{
    public static class HttpServices
    {
        private static IHttpInputStreamDecodingStrategy _inputStreamDecodingStrategy = new HttpInputStreamDecodingStrategy();
        private static IDefaultResponseHeadersSetter _defaultResponseHeadersSetter = new DefaultResponseHeadersSetter();
        private static IHttpHeaderReader _headerReader = new HttpHeaderReader();
        private static IHttpHeadersSerializer _headersSerializer = new HttpHeadersSerializer();
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();


        public static IHttpInputStreamDecodingStrategy InputStreamDecodingStrategy
        {
            get => _inputStreamDecodingStrategy;
            set => _inputStreamDecodingStrategy = value ?? throw new ArgumentNullException(nameof(InputStreamDecodingStrategy));
        }

        public static  IDefaultResponseHeadersSetter DefaultResponseHeadersSetter
        {
            get => _defaultResponseHeadersSetter;
            set => _defaultResponseHeadersSetter = value ?? throw new ArgumentNullException(nameof(DefaultResponseHeadersSetter));
        }

        public static IHttpHeaderReader HeaderReader
        {
            get => _headerReader;
            set => _headerReader = value ?? throw new ArgumentNullException(nameof(HeaderReader));
        }

        public static IHttpHeadersSerializer HeadersSerializer
        {
            get => _headersSerializer;
            set => _headersSerializer = value ?? throw new ArgumentNullException(nameof(HeadersSerializer));
        }

        public static ILoggerFactory LoggerFactory
        {
            get => _loggerFactory;
            set => _loggerFactory = value ?? throw new ArgumentNullException(nameof(LoggerFactory));
        }

    }
}