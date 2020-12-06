using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Server.Models;
using ElMariachi.Http.Server.Models.ResponseContent;
using ElMariachi.Http.Server.Services;
using ElMariachi.Http.Server.Services.Internals;
using ElMariachi.Http.Server.Streams.Input;
using Microsoft.Extensions.Logging;

namespace ElMariachi.Http.Server
{
    internal class HttpServer : IHttpServer
    {
        public const string SupportedHttpVersion = "HTTP/1.1";

        private TcpListenerEx? _tcpListenerEx;

        private readonly object _lock = new object();
        private readonly ILogger<HttpServer> _logger;
        private readonly IHttpHeaderReader _headerReader;
        private readonly IHttpInputStreamDecodingStrategy _inputStreamDecodingStrategy;
        private readonly IHttpResponseSenderFactory _httpResponseSenderFactory;
        private int _port = 80;
        private IPAddress _ipAddress = IPAddress.Any;

        public HttpServer(ILogger<HttpServer> logger, IHttpHeaderReader headerReader, IHttpInputStreamDecodingStrategy inputStreamDecodingStrategy, IHttpResponseSenderFactory httpResponseSenderFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _headerReader = headerReader ?? throw new ArgumentNullException(nameof(headerReader));
            _inputStreamDecodingStrategy = inputStreamDecodingStrategy ?? throw new ArgumentNullException(nameof(inputStreamDecodingStrategy));
            _httpResponseSenderFactory = httpResponseSenderFactory;
        }

        public int MaxHeadersSize { get; set; } = 8 * 1024; // 4KiB

        public int MaxRequestUriSize { get; set; } = 4 * 1024; // 4KiB

        public IPAddress IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_tcpListenerEx != null)
                    throw new InvalidOperationException("Server is started.");

                _ipAddress = value;
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (_tcpListenerEx != null)
                    throw new InvalidOperationException("Server is started.");
                _port = value;
            }
        }

        public int ReadTimeoutMs { get; set; } = 2000; // 2 sec

        public int ConnectionKeepAliveTimeoutMs { get; set; } = 30000; // 60 sec

        public int MaxMethodNameSize { get; set; } = 30;

        public int MaxInputStreamCleaning { get; set; } = 4 * 1024 * 1024; // 4MiB

        public bool IsStarted
        {
            get
            {
                lock (_lock)
                {
                    return _tcpListenerEx != null;
                }
            }
        }

        public bool IsSecureConnection => false;

        public Task Start(RequestHandler requestHandler)
        {
            return Start(requestHandler, new CancellationToken());
        }

        public Task Start(RequestHandler requestHandler, CancellationToken cancellationToken)
        {
            if (requestHandler == null)
                throw new ArgumentNullException(nameof(requestHandler));

            var ipAddress = IPAddress;
            if (ipAddress == null)
                throw new InvalidOperationException($"{nameof(IPAddress)} is not defined.");

            if (_tcpListenerEx != null)
                throw new InvalidOperationException("Server is already started.");

            lock (_lock)
            {
                try
                {
                    _tcpListenerEx = new TcpListenerEx(ipAddress, Port);

                    _tcpListenerEx.Start(cancellationToken);

                    return Task.Run(() =>
                    {
                        ListenClients(requestHandler, cancellationToken);
                    }, cancellationToken).ContinueWith(task =>
                    {
                        _tcpListenerEx = null;
                    }, cancellationToken);
                }
                catch
                {
                    try
                    { _tcpListenerEx?.Stop(); }
                    catch { }

                    _tcpListenerEx = null;
                    throw;
                }
            }
        }

        private void ListenClients(RequestHandler requestHandler, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && _tcpListenerEx != null)
                {
                    var client = _tcpListenerEx.AcceptTcpClient();
                    Task.Run(() => HandleClient(client, requestHandler), ct);
                }
            }
            catch (SocketException) when (ct.IsCancellationRequested)
            {
                // NOTE: here, the server has been stopped properly
            }
        }

        private async Task HandleClient(TcpClient client, RequestHandler requestHandler)
        {
            _logger.LogInformation($"Incoming client: {client.GetHashCode()}.");

            NetworkStream? networkStream = null;

            try
            {
                networkStream = client.GetStream();

                var keepConnectionOpened = false;
                var atLeastOneByteRead = false;
                do
                {
                    var isFirstRequestWithClient = !keepConnectionOpened;
                    atLeastOneByteRead = false;

                    networkStream.ReadTimeout = isFirstRequestWithClient ? ReadTimeoutMs : ConnectionKeepAliveTimeoutMs;

                    var responseSender = _httpResponseSenderFactory.Create(networkStream, MaxInputStreamCleaning);

                    var httpInputStream = new HttpInputStream(networkStream, responseSender);

                    DateTime? requestStart = null;
                    httpInputStream.AtLeastOneByteRead += () =>
                    {
                        atLeastOneByteRead = true;
                        requestStart = DateTime.Now;
                        networkStream.ReadTimeout = ReadTimeoutMs;
                    };

                    responseSender.ResponseSent += (sender, args) =>
                    {
                        if (requestStart != null)
                        {
                            var requestEnd = DateTime.Now;
                            _logger.LogInformation($"Processing time (ms): {(requestEnd - requestStart.Value).TotalMilliseconds}");
                        }
                    };

                    HttpRequest request;
                    try
                    {
                        var header = _headerReader.Read(httpInputStream, MaxMethodNameSize, MaxHeadersSize, MaxRequestUriSize);

                        // Check HTTP version is supported
                        if (!string.Equals(header.HttpVersion, SupportedHttpVersion, StringComparison.OrdinalIgnoreCase))
                            throw new HttpVersionNotSupportedException(new[] { SupportedHttpVersion }, header.HttpVersion);

                        _logger.LogInformation($"Incoming request {header.RequestUri}");

                        // Build the absolute resource request Uri (https://tools.ietf.org/html/rfc2616#section-5.2)
                        Uri absRequestUri;
                        if (header.RequestUri.IsAbsoluteUri)
                            absRequestUri = header.RequestUri;
                        else
                        {
                            var host = header.Headers.Host;
                            if (host == null)
                                throw new NoAbsUriResourceRequestException("Unable to determine absolute resource URL, raw request URL is relative and Host header is missing.");

                            var protocol = IsSecureConnection ? "https://" : "http://";
                            var serverUrl = $"{protocol}{host}";

                            try
                            {
                                absRequestUri = new Uri(new Uri(serverUrl, UriKind.Absolute), header.RequestUri);
                            }
                            catch (Exception ex)
                            {
                                throw new NoAbsUriResourceRequestException($"Unable to build absolute resource URL: {ex.Message}.");
                            }
                        }

                        var inputStream = _inputStreamDecodingStrategy.Create(httpInputStream, header);

                        request = new HttpRequest(header, inputStream, absRequestUri, responseSender);
                    }
                    catch (HttpVersionNotSupportedException ex)
                    {
                        SendHttpVersionNotSupported(ex, responseSender);
                        break;
                    }
                    catch (RequestUriToolLongException ex)
                    {
                        SendHttpRequestUriToolLong(ex, responseSender);
                        break;
                    }
                    catch (NoAbsUriResourceRequestException ex)
                    {
                        SendBadRequest(ex, responseSender);
                        break;
                    }
                    catch (HeaderFormatException ex)
                    {
                        SendBadRequest(ex, responseSender);
                        break;
                    }
                    catch (RequestFormatException ex)
                    {
                        SendBadRequest(ex, responseSender);
                        break;
                    }
                    catch (StreamLimitException ex)
                    {
                        SendBadRequest(ex, responseSender);
                        break;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException ex2 && ex2.SocketErrorCode == SocketError.TimedOut)
                    {
                        // NOTE: here, the read timeout has been reached, we simply close the connection

                        if (!atLeastOneByteRead && isFirstRequestWithClient)
                            _logger.LogWarning($"Incoming client {client.GetHashCode()} didn't send any byte within the time limit ({networkStream.ReadTimeout}ms).");
                        break;
                    }
                    catch (StreamEndException)
                    {
                        // NOTE: here the connection has been closed (or broken) while reading the input inputStream.
                        break;
                    }

                    try
                    {
                        // ============================= //
                        // ==> Calls request handler <== //
                        requestHandler(request);
                        // ==> Calls request handler <== //
                        // ============================= //
                    }
                    catch (Exception ex)
                    {
                        if (!responseSender.IsResponseSent)
                            SendApplicativeError(ex, request);
                        break;
                    }

                    // NOTE: here, the handler didn't send a response
                    if (!responseSender.IsResponseSent)
                        SendFallbackResponse(request);

                    keepConnectionOpened = request.Headers.Connection.KeepAlive && !responseSender.CloseConnection;
                } while (keepConnectionOpened);

            }
            catch (Exception ex)
            {
                OnUnexpectedErrorInternal(ex);
            }
            finally
            {
                if (networkStream != null)
                    await networkStream.DisposeAsync();

                _logger.LogInformation($"Exiting client: {client.GetHashCode()}.");
                client.Dispose();
            }

        }

        private void OnUnexpectedErrorInternal(Exception ex)
        {
            try
            {
                OnUnexpectedError(ex);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Override this method to change the behavior on unexpected server exception
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnUnexpectedError(Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error occurred: {ex.Message}.");
        }

        private static void SendHttpVersionNotSupported(HttpVersionNotSupportedException ex, IHttpResponseSender responseSender)
        {
            var headers = new HttpResponse(HttpStatus.HttpVersionNotSupported);
            responseSender.Send(headers);
        }

        private void SendHttpRequestUriToolLong(RequestUriToolLongException ex, IHttpResponseSender responseSender)
        {
            var response = CreateHttpRequestUriToolLongResponse(ex);
            responseSender.Send(response);
        }

        /// <summary>
        /// Override this method to customize request URI too long response
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected virtual IHttpResponse CreateHttpRequestUriToolLongResponse(RequestUriToolLongException ex)
        {
            var response = new HttpResponse(HttpStatus.RequestUriTooLarge)
            {
                Content = new StringResponseContent(ex.Message)
            };
            return response;
        }

        private void SendBadRequest(Exception ex, IHttpResponseSender responseSender)
        {
            var response = CreateBadRequestResponse(ex);
            responseSender.Send(response);
        }

        /// <summary>
        /// Override this method to customize bad request response
        /// </summary>
        /// <returns></returns>
        protected virtual IHttpResponse CreateBadRequestResponse(Exception ex)
        {
            var httpResponse = new HttpResponse(HttpStatus.BadRequest)
            {
                Content = new StringResponseContent(ex.Message)
            };
            return httpResponse;
        }

        private void SendApplicativeError(Exception ex, IHttpRequest request)
        {
            var response = CreateApplicativeErrorResponse(ex);
            response.Request = request;
            request.SendResponse(response);
        }

        /// <summary>
        /// Override this method to customize applicative error response.
        /// This method is called when an exception occurred in the HTTP response handler.
        /// </summary>
        /// <returns></returns>
        protected virtual IHttpResponse CreateApplicativeErrorResponse(Exception ex)
        {
            var httpResponse = new HttpResponse(HttpStatus.InternalServerError)
            {
                Content = new StringResponseContent($"Oops: {ex.Message}")
            };

            return httpResponse;
        }

        private void SendFallbackResponse(IHttpRequest request)
        {
            var response = CreateFallbackResponse();
            response.Request = request;
            request.SendResponse(response);
        }

        /// <summary>
        /// Override this method to customize fallback response.
        /// This method is call when the HTTP server handler didn't send any response.
        /// </summary>
        /// <returns></returns>
        protected virtual IHttpResponse CreateFallbackResponse()
        {
            var httpResponse = new HttpResponse(HttpStatus.NotFound)
            {
                Content = new StringResponseContent("404, Not Found :(")
            };
            return httpResponse;
        }
    }
}
