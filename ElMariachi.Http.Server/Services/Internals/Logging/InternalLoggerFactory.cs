using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElMariachi.Http.Server.Services.Internals.Logging
{
    internal class InternalLoggerFactory : IInternalLoggerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public InternalLoggerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }


        public ILogger<T> Create<T>()
        {
            var logger = _serviceProvider.GetService<ILogger<T>>();
            return logger ?? new StubbedLogger<T>();
        }
    }
}