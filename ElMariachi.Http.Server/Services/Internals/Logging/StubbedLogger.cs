using System;
using Microsoft.Extensions.Logging;

namespace ElMariachi.Http.Server.Services.Internals.Logging
{
    internal class StubbedLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new EmptyDisposable();
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}