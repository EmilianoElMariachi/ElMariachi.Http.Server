using System;
using System.Threading.Tasks;
using ElMariachi.Http.Server.Demo.Properties;
using ElMariachi.Http.Server.Models;
using ElMariachi.Http.Server.Models.ResponseContent;
using ElMariachi.Http.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElMariachi.Http.Server.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpServer()        // Injects the HTTP server implementation in the ServiceCollection
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.Get<ILoggerFactory>().CreateLogger<Program>();

            var httpServer = serviceProvider.Get<IHttpServer>();

            httpServer.ActiveConnectionsCountChanged += (sender, args) =>
            {
                logger.LogInformation($"Active connections {args.ActualCount} ({(args.ChangeType == CountChangeType.Gained ? "+1" : "-1")})");
            };

            _ = Task.Run(async () =>
            {
                logger.LogInformation("Press «ESC» to stop");

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

                logger.LogInformation("Server stopping...");

                try
                {
                    await httpServer.Stop(TimeSpan.FromMilliseconds(httpServer.ConnectionKeepAliveTimeoutMs));
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred while stopping the server: {ex.Message}");
                }
            });

            Task serverTask;
            try
            {
                serverTask = httpServer.Start(OnRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to start HTTP Server: {ex.Message}");
                logger.LogInformation("Press any key to exit.");
                goto WaitEndExit;
            }

            try
            {
                await serverTask;
                logger.LogInformation("Server stopped.");
            }
            catch (Exception ex)
            {
                logger.LogError($"HTTP server stopped abnormally: {ex.Message}");
            }

            WaitEndExit:
            logger.LogInformation("Press any key to exit.");
            Console.ReadKey();

        }

        private static void OnRequest(IHttpRequest request)
        {
            if (request.Method == "GET")
            {
                if (request.AbsRequestUri.AbsolutePath == "/")
                {
                    request.SendResponse(new HttpResponse(HttpStatus.Ok)
                    {
                        Content = new StringResponseContent("<!DOCTYPE html><html><head><title>ElMariachi</title></head><body>Hello World!</body></html>", "text/html")
                    });
                }
                else if (request.AbsRequestUri.AbsolutePath == "/favicon.ico")
                {
                    request.SendResponse(new HttpResponse(HttpStatus.Ok)
                    {
                        Content = new ByteArrayResponseContent(Resources.Favicon)
                    });
                }
            }
            else if (request.Method == "POST")
            {
                //request.ReadContentAsStringAsync();
            }
        }
    }

}
