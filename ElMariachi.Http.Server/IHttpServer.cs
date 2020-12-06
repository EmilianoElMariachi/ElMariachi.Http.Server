using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ElMariachi.Http.Server.Models;

namespace ElMariachi.Http.Server
{
    public interface IHttpServer
    {
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
        /// <param name="requestHandler"></param>
        /// <returns></returns>
        Task Start(RequestHandler requestHandler);

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="requestHandler"></param>
        /// <param name="cancellationToken">The cancellation token for stopping the server</param>
        /// <returns></returns>
        Task Start(RequestHandler requestHandler, CancellationToken cancellationToken);

        public static IHttpServer Create(IPAddress ipAddress, int port = 80)
        {
            return new HttpServer(ipAddress, port);
        }

    }

    public delegate void RequestHandler(IHttpRequest request);

}