using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ElMariachi.Http.Server.Models;

namespace ElMariachi.Http.Server
{
    public interface IHttpServer
    {
        /// <summary>
        /// Triggered when <see cref="ActiveConnectionsCount"/> property changes
        /// </summary>
        event ActiveConnectionsCountChangedHandler ActiveConnectionsCountChanged;

        /// <summary>
        /// Get the actual number of connected clients
        /// </summary>
        int ActiveConnectionsCount { get; }

        /// <summary>
        /// Get or set the server IP address.
        /// Can't be set when server is started
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        IPAddress IPAddress { get; set; }

        /// <summary>
        /// Get or set the server port.
        /// Can't be set when server is started
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        int Port { get; set; }

        /// <summary>
        /// Get or set the HTTP input stream reading timeout in milliseconds.
        /// Set <see cref="Timeout.Infinite"/> for no timeout (not recommended).
        /// </summary>
        int ReadTimeoutMs { get; set; }

        /// <summary>
        /// Get or set the maximum number of time (in milliseconds) a connection can be kept alive.
        /// Set <see cref="Timeout.Infinite"/> for no timeout (not recommended).
        /// </summary>
        int ConnectionKeepAliveTimeoutMs { get; set; }

        /// <summary>
        /// Get or set the maximum number of bytes allowed for the method name for accepting a request
        /// </summary>
        int MaxMethodNameSize { get; set; }

        /// <summary>
        /// Get or set the maximum number of bytes allowed in the headers for accepting a request
        /// </summary>
        int MaxHeadersSize { get; set; }

        /// <summary>
        /// Get or set the maximum number of bytes allowed for as request URI for accepting a request
        /// </summary>
        int MaxRequestUriSize { get; set; }

        /// <summary>
        /// Get or set the maximum number of bytes the server is allowed to read in the HTTP input stream for emptying the pending data sent by a client.
        /// Set zero to disable cleaning, or a negative value to clean all the input stream.
        /// By default, an HTTP server is always supposed to read all the data sent by a client.
        /// </summary>
        int MaxInputStreamCleaning { get; set; }

        /// <summary>
        /// Get a boolean indicating that the server is currently listening
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Get a boolean indicating if connection is secure (HTTPS)
        /// </summary>
        bool IsSecureConnection { get; }

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="requestHandler">The request handler</param>
        /// <returns></returns>
        Task Start(RequestHandler requestHandler);

        /// <summary>
        /// Stops the server immediately.
        /// Does not throw if server is already stopped.
        /// </summary>
        void Stop();

        /// <summary>
        /// Kindly stops the server, waiting for all pending requests and connections.
        /// Does not throw if server is already stopped.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        Task Stop(TimeSpan timeout);

    }

    public delegate void RequestHandler(IHttpRequest request);


    public delegate void ActiveConnectionsCountChangedHandler(object sender, ActiveConnectionsCountChangedHandlerArgs args);


    public enum CountChangeType
    {
        Gained,
        Lost
    }

    public class ActiveConnectionsCountChangedHandlerArgs
    {
        public ActiveConnectionsCountChangedHandlerArgs(int actualCount, CountChangeType changeType)
        {
            ActualCount = actualCount;
            ChangeType = changeType;
        }
        public int ActualCount { get; }

        public CountChangeType ChangeType { get; }
    }


}