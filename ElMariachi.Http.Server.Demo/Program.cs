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

            var httpServer = serviceProvider.Get<IHttpServer>();

            _ = Task.Run(() =>
            {
                Console.WriteLine("Press «ESC» to stop");
                
                while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

                httpServer.Stop();
                Console.Write("Server stopping...");
            });

            Task serverTask;
            try
            {
                serverTask = httpServer.Start(OnRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start HTTP Server: {ex.Message}.");
                Console.WriteLine($"Press any key to exit.");
                Console.ReadKey();
                return;
            }

            try
            {
                await serverTask;
                Console.WriteLine("OK.");
                Console.WriteLine($"Press any key to exit.");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP server stopped abnormally: {ex.Message}.");
            }
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
