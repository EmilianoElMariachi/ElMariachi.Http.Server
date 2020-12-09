using System;
using ElMariachi.Http.Server.Services.Internals;
using ElMariachi.Http.Server.Services.Internals.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ElMariachi.Http.Server.Services
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddHttpServer(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddTransient<IHttpServer, HttpServer>();
            serviceCollection.AddTransient<IHttpHeaderReader, HttpHeaderReader>();

            serviceCollection.AddSingleton<IHttpResponseSenderFactory, HttpResponseSenderFactory>();
            serviceCollection.AddSingleton<IInternalLoggerFactory, InternalLoggerFactory>();

            serviceCollection.TryAddSingleton<IDefaultResponseHeadersSetter, DefaultResponseHeadersSetter>();
            serviceCollection.TryAddSingleton<IHttpHeadersSerializer, HttpHeadersSerializer>();
            serviceCollection.TryAddSingleton<IHttpInputStreamDecodingStrategy, HttpInputStreamDecodingStrategy>();

            return serviceCollection;
        }

    }
}
