using System;
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

namespace ElMariachi.Http.Server
{

    public class HttpServer : IHttpServer
    {
        public const string SupportedHttpVersion = "HTTP/1.1";

        private readonly TcpListenerEx _tcpListenerEx;

        private readonly object _lock = new object();

        public HttpServer(IPAddress ipAddress, int port = 80)
        {
            _tcpListenerEx = new TcpListenerEx(ipAddress, port);
        }

        public int MaxHeadersSize { get; set; } = 8 * 1024; // 4KiB

        public int MaxRequestUriSize { get; set; } = 4 * 1024; // 4KiB

        public int InputStreamReadTimeoutMs { get; set; } = 10000; // 10 sec

        public int MaxMethodNameSize { get; set; } = 30;

        public int MaxInputStreamCleaning { get; set; } = 4 * 1024 * 1024; // 4MiB

        public bool IsListening => _tcpListenerEx.IsListening;

        public bool IsSecureConnection => false;

        public Task Start(RequestHandler requestHandler)
        {
            return Start(requestHandler, new CancellationToken());
        }

        public Task Start(RequestHandler requestHandler, CancellationToken cancellationToken)
        {
            if (requestHandler == null)
                throw new ArgumentNullException(nameof(requestHandler));

            lock (_lock)
            {
                _tcpListenerEx.Start();

                // NOTE: unfortunately, the AcceptTcpClient method doesn't accept a CancellationToken,
                // the task below is waiting for a the cancellationToken to be signaled in order to stop the server.
                Task.Run(() =>
                {
                    cancellationToken.WaitHandle.WaitOne();
                    _tcpListenerEx.Stop();
                }, cancellationToken);

                return Task.Run(() =>
                {
                    ListenClients(requestHandler, cancellationToken);
                }, cancellationToken);
            }
        }

        private void ListenClients(RequestHandler requestHandler, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = _tcpListenerEx.AcceptTcpClient();
                    Task.Run(() => HandleClient(client, requestHandler), ct);
                }
            }
            catch (SocketException) when (ct.IsCancellationRequested)
            {
            }
        }

        private async Task HandleClient(TcpClient client, RequestHandler requestHandler)
        {
            Console.WriteLine($"Incoming client: {client.GetHashCode()}.");

            NetworkStream? networkStream = null;

            try
            {
                networkStream = client.GetStream();

                bool keepConnectionOpened;
                do
                {
                    var requestStart = DateTime.Now;
                    client.ReceiveTimeout = InputStreamReadTimeoutMs;

                    IHttpResponseSender responseSender = new HttpResponseSender(networkStream, MaxInputStreamCleaning);
                    responseSender.ResponseSent += (sender, args) =>
                    {
                        var requestEnd = DateTime.Now;
                        Console.WriteLine($"Processing time (ms): {(requestEnd - requestStart).TotalMilliseconds}");
                    };

                    var httpInputStream = new HttpInputStream(networkStream, responseSender);

                    HttpRequest request;
                    try
                    {
                        client.ReceiveTimeout = Timeout.Infinite;
                        var header = await Task.Run(() => HttpServices.Instance.HeaderReader.Read(httpInputStream, MaxMethodNameSize, MaxHeadersSize, MaxRequestUriSize, onFirstCharRead: () =>
                        {
                            client.ReceiveTimeout = InputStreamReadTimeoutMs;
                        }));

                        // Check HTTP version is supported
                        if (!string.Equals(header.HttpVersion, SupportedHttpVersion, StringComparison.OrdinalIgnoreCase))
                            throw new HttpVersionNotSupportedException(new[] { SupportedHttpVersion }, header.HttpVersion);

                        Console.WriteLine($"Incoming request {header.RequestUri}");

                        // Build the absolute resource request Uri (https://tools.ietf.org/html/rfc2616#section-5.2)
                        Uri absRequestUri;
                        if (header.RequestUri.IsAbsoluteUri)
                            absRequestUri = header.RequestUri;
                        else
                        {
                            var host = header.Headers.Host;
                            if (host == null)
                                throw new NoAbsUriResourceRequestException("Unable to determine absolute resource URL, raw request URL is relative and Host header is missing.");

                            var protocol = (IsSecureConnection ? "https://" : "http://");
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

                        var inputStream = HttpServices.Instance.InputStreamDecodingStrategy.Create(httpInputStream, header);
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
                        OnUnexpectedErrorInternal(ex);
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

                Console.WriteLine($"Exiting client: {client.GetHashCode()}.");
                client.Dispose();
            }

        }


        protected virtual void OnUnexpectedError(Exception ex)
        {
            Console.Error.WriteLine(ex);
        }

        private void OnUnexpectedErrorInternal(Exception ex)
        {
            try
            {
                OnUnexpectedError(ex);
            }
            catch (Exception)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void SendHttpVersionNotSupported(HttpVersionNotSupportedException ex, IHttpResponseSender responseSender)
        {
            var headers = new HttpResponse(HttpStatus.HttpVersionNotSupported);
            responseSender.Send(headers);
        }

        private void SendHttpRequestUriToolLong(RequestUriToolLongException ex, IHttpResponseSender responseSender)
        {
            try
            {
                var response = CreateHttpRequestUriToolLongResponse(ex);
                responseSender.Send(response);
            }
            catch (Exception ex2)
            {
                OnUnexpectedErrorInternal(ex2);
                throw;
            }
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
            try
            {
                var response = CreateBadRequestResponse(ex);
                responseSender.Send(response);
            }
            catch (Exception ex2)
            {
                OnUnexpectedErrorInternal(ex2);
                throw;
            }
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
            try
            {
                var response = CreateApplicativeErrorResponse(ex);
                response.Request = request;
                request.SendResponse(response);
            }
            catch (Exception ex2)
            {
                OnUnexpectedErrorInternal(ex2);
                throw;
            }
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
            try
            {
                var response = CreateFallbackResponse();
                response.Request = request;
                request.SendResponse(response);
            }
            catch (Exception ex)
            {
                OnUnexpectedErrorInternal(ex);
                throw;
            }
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
