using Microsoft.Extensions.Logging;

namespace ElMariachi.Http.Server.Services.Internals.Logging
{
    /// <summary>
    /// This interface should always be used internally for obtaining a <see cref="ILogger&lt;T&gt;"/> instance.
    /// Why? This project is only referencing the abstract layer of the .NET Core Logging, but here we don't
    /// want to force end-users of this library to always register a Logging implementation.
    /// </summary>
    internal interface IInternalLoggerFactory
    {
        ILogger<T> Create<T>();
    }
}
