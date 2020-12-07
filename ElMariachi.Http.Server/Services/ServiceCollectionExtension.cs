using System;
using ElMariachi.Http.Server.Services.Internals;
using Microsoft.Extensions.DependencyInjection;

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

            serviceCollection.AddSingleton<IDefaultResponseHeadersSetter, DefaultResponseHeadersSetter>();
            serviceCollection.AddSingleton<IHttpResponseSenderFactory, HttpResponseSenderFactory>();
            serviceCollection.AddSingleton<IHttpHeadersSerializer, HttpHeadersSerializer>();
            serviceCollection.AddSingleton<IHttpInputStreamDecodingStrategy, HttpInputStreamDecodingStrategy>();

            return serviceCollection;
        }

        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="provider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
        /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
        public static T Get<T>(this IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var serviceType = typeof(T);
            var service = provider.GetService(serviceType);
            if (service == null)
                throw new InvalidOperationException($"Type «{serviceType}» is not registered!");
            return (T)service;
        }

    }
}
