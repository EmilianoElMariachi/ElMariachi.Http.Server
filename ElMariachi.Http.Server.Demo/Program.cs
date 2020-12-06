using System;
using System.Threading;
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
            var serviceCollection = new ServiceCollection();


            serviceCollection.AddHttpServer();
            serviceCollection.AddLogging(builder => builder.AddConsole());

            var serviceProvider = serviceCollection.BuildServiceProvider();


            var httpServer = serviceProvider.Get<IHttpServer>();

            var cts = new CancellationTokenSource();
            var serverTask = httpServer.Start(OnRequest, cts.Token);

            Console.WriteLine("Press «ESC» to stop");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
            }

            Console.Write("Server stopping...");
            cts.Cancel();

            serverTask.Wait(TimeSpan.FromSeconds(10));

            Console.WriteLine("OK");

            Console.WriteLine("Press any key to exit");
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
