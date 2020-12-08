using System;
using System.Collections.Generic;
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

        private readonly object _tcpListenerLock = new object();
        private readonly ILogger<HttpServer> _logger;
        private readonly IHttpHeaderReader _headerReader;
        private readonly IHttpInputStreamDecodingStrategy _inputStreamDecodingStrategy;
        private readonly IHttpResponseSenderFactory _httpResponseSenderFactory;
        private int _port = 80;
        private IPAddress _ipAddress = IPAddress.Any;
        private readonly List<TcpClient> _activeClients = new List<TcpClient>();
        private StopRequest _stopRequest = StopRequest.None;

        public HttpServer(ILogger<HttpServer> logger, IHttpHeaderReader headerReader, IHttpInputStreamDecodingStrategy inputStreamDecodingStrategy, IHttpResponseSenderFactory httpResponseSenderFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _headerReader = headerReader ?? throw new ArgumentNullException(nameof(headerReader));
            _inputStreamDecodingStrategy = inputStreamDecodingStrategy ?? throw new ArgumentNullException(nameof(inputStreamDecodingStrategy));
            _httpResponseSenderFactory = httpResponseSenderFactory;
        }

        public int MaxHeadersSize { get; set; } = 8 * 1024; // 4KiB

        public int MaxRequestUriSize { get; set; } = 4 * 1024; // 4KiB

        public event ActiveConnectionsCountChangedHandler? ActiveConnectionsCountChanged;

        public int ActiveConnectionsCount
        {
            get
            {
                lock (_activeClients)
                {
                    return _activeClients.Count;
                }
            }
        }

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

        public bool IsListening
        {
            get
            {
                lock (_tcpListenerLock)
                {
                    return _tcpListenerEx != null && _tcpListenerEx.IsListening;
                }
            }
        }

        public bool IsSecureConnection => false;

        public async Task Start(RequestHandler requestHandler)
        {
            if (requestHandler == null)
                throw new ArgumentNullException(nameof(requestHandler));

            var ipAddress = IPAddress;
            if (ipAddress == null)
                throw new InvalidOperationException($"{nameof(IPAddress)} is not defined.");


            lock (_tcpListenerLock)
            {
                if (_tcpListenerEx != null)
                    throw new InvalidOperationException("Server is already started.");

                _tcpListenerEx = new TcpListenerEx(ipAddress, Port);
            }

            try
            {
                _tcpListenerEx.Start();

                while (_tcpListenerEx != null && _stopRequest == StopRequest.None)
                {
                    TcpClient client;
                    try
                    {
                        client = await _tcpListenerEx.AcceptTcpClientAsync();
                    }
                    catch when (_stopRequest != StopRequest.None)
                    {
                        // NOTE: here, the TCP listener has been stopped
                        break;
                    }

                    _ = Task.Run(() =>
                    {
                        using (client)
                        {
                            try
                            {
                                _logger.LogTrace($"Incoming client: {client.GetHashCode()}.");
                                lock (_activeClients)
                                {
                                    _activeClients.Add(client);
                                    NotifyActiveConnectionsCountChanged(_activeClients.Count, CountChangeType.Gained);
                                }
                                HandleClient(client, requestHandler);
                            }
                            finally
                            {
                                lock (_activeClients)
                                {
                                    _activeClients.Remove(client);
                                    NotifyActiveConnectionsCountChanged(_activeClients.Count, CountChangeType.Lost);
                                }
                                _logger.LogTrace($"Exiting client: {client.GetHashCode()}.");
                            }
                        }
                    });
                }
            }
            finally
            {
                lock (_tcpListenerLock)
                {
                    _tcpListenerEx?.Stop();
                    _tcpListenerEx = null;
                    _stopRequest = StopRequest.None;
                }
            }
        }

        public Task Stop(TimeSpan timeout)
        {
            lock (_tcpListenerLock)
            {
                if (_tcpListenerEx == null)
                    return Task.CompletedTask;

                return Task.Run(() =>
                {
                    _stopRequest = StopRequest.Kind;

                    try
                    {
                        using var noMoreConnectionsWaiter = new NoMoreConnectionsWaiter(this);
                        if (!noMoreConnectionsWaiter.Wait(timeout))
                            throw new TimeoutException($"Failed to kindly stop the server within the allocated time ({timeout}).");
                    }
                    finally
                    {
                        _tcpListenerEx?.Stop();
                    }
                });
            }
        }

        private class NoMoreConnectionsWaiter : IDisposable
        {
            private readonly IHttpServer _httpServer;
            private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

            public NoMoreConnectionsWaiter(IHttpServer httpServer)
            {

                _httpServer = httpServer;
                if (httpServer.ActiveConnectionsCount <= 0)
                    _manualResetEvent.Set();
                else
                    httpServer.ActiveConnectionsCountChanged += OnActiveConnectionsCountChanged;
            }

            private void OnActiveConnectionsCountChanged(object sender, ActiveConnectionsCountChangedHandlerArgs args)
            {
                if (args.ActualCount <= 0)
                {
                    _httpServer.ActiveConnectionsCountChanged -= OnActiveConnectionsCountChanged;
                    _manualResetEvent.Set();
                }
            }

            /// <summary>
            /// Wait until there is no active connection left.
            /// </summary>
            /// <param name="timeout"></param>
            /// <returns>false if a timeout occurred, true otherwise</returns>
            public bool Wait(TimeSpan timeout)
            {
                var signaled = _manualResetEvent.WaitOne(timeout);
                return signaled;
            }

            public void Dispose()
            {
                _httpServer.ActiveConnectionsCountChanged -= OnActiveConnectionsCountChanged;
                _manualResetEvent.Dispose();
            }
        }

        public void Stop()
        {
            lock (_tcpListenerLock)
            {
                if (_tcpListenerEx == null)
                    return;
                _stopRequest = StopRequest.Aggressive;
                _tcpListenerEx.Stop();
            }
        }

        private enum StopRequest
        {
            None,
            Aggressive,
            Kind
        }

        private void HandleClient(TcpClient client, RequestHandler requestHandler)
        {
            try
            {
                var networkStream = client.GetStream();
                var atLeastOneByteRead = false;
                bool keepConnectionOpened;
                do
                {
                    atLeastOneByteRead = false;

                    networkStream.ReadTimeout = _stopRequest == StopRequest.None ? ConnectionKeepAliveTimeoutMs : ReadTimeoutMs;

                    var responseSender = _httpResponseSenderFactory.Create(networkStream, MaxInputStreamCleaning);

                    var httpInputStream = new HttpInputStream(networkStream, responseSender);

                    DateTime? requestStart = null;
                    httpInputStream.AtLeastOneByteRead += () =>
                    {
                        atLeastOneByteRead = true;
                        requestStart = DateTime.Now;
                        networkStream.ReadTimeout = ReadTimeoutMs;  // NOTE: once at least one byte is received we set the default read timeout
                    };

                    responseSender.ResponseSent += (sender, args) =>
                    {
                        if (requestStart != null)
                        {
                            var requestEnd = DateTime.Now;
                            _logger.LogInformation($"Request finished in {(requestEnd - requestStart.Value).TotalMilliseconds}ms {args.StatusCode}");
                        }
                    };

                    HttpRequest request;
                    try
                    {
                        var header = _headerReader.Read(httpInputStream, MaxMethodNameSize, MaxHeadersSize, MaxRequestUriSize);

                        // Check HTTP version is supported
                        if (!string.Equals(header.HttpVersion, SupportedHttpVersion, StringComparison.OrdinalIgnoreCase))
                            throw new HttpVersionNotSupportedException(new[] { SupportedHttpVersion }, header.HttpVersion);

                        _logger.LogInformation($"Request starting {header.Method} {header.RequestUri}");

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

                        if (atLeastOneByteRead)
                            _logger.LogWarning($"Request read timeout of ({networkStream.ReadTimeout}ms) occurred for client {client.GetHashCode()}, connection will be forcibly closed.");
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

                    keepConnectionOpened = request.Headers.Connection.KeepAlive && !responseSender.CloseConnection && _stopRequest == StopRequest.None;
                } while (keepConnectionOpened);

            }
            catch (Exception ex)
            {
                OnUnexpectedErrorInternal(ex);
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

        private void NotifyActiveConnectionsCountChanged(int actualCount, CountChangeType changeType)
        {
            ActiveConnectionsCountChanged?.Invoke(this, new ActiveConnectionsCountChangedHandlerArgs(actualCount, changeType));
        }
    }
}
