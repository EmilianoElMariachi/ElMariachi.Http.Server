using System;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace ElMariachi.Http.Server.Services.Internals
{
    internal class HttpResponseSenderFactory : IHttpResponseSenderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public HttpResponseSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IHttpResponseSender Create(NetworkStream networkStream, int maxInputStreamCleaning)
        {
            var defaultResponseHeadersSetter = _serviceProvider.GetRequiredService<IDefaultResponseHeadersSetter>();
            var headersSerializer = _serviceProvider.GetRequiredService<IHttpHeadersSerializer>();

            return new HttpResponseSender(networkStream, maxInputStreamCleaning, defaultResponseHeadersSetter, headersSerializer);
        }
    }
}