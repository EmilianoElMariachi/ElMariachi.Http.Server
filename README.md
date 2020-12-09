# ElMariachi.Http.Server

## Description

This project is a .NET Core HTTP server library written in C#.

## Why ?

I decided to create my own HTTP server implementation for at least 3 reasons:

1. The native «System.Net.HttpListener» can't be run immediately without administrative privileges
2. The native «System.Net.HttpListener» is not really user friendly and quite old
3. I wanted to improve my skills on HTTP protocol

## Key features

- No administrative privileges required
- Simple
- Fast
- Lightweight
- Extensible
- Abstraction for all classes
- Non destructive header parsing
- Transfer Encoding Support (gzip, chunked, deflate)
- Unit tested

## Quick start

Pick the latest nuget at [https://www.nuget.org/packages/ElMariachi.Http.Server/](https://www.nuget.org/packages/ElMariachi.Http.Server/)

```csharp
async void Main(string[] args)
{

    var serviceProvider = new ServiceCollection()
        .AddHttpServer() // Injects the HTTP server implementation in the ServiceCollection
        .BuildServiceProvider();

    // Create a new server
    var httpServer = serviceProvider.GetRequiredService<IHttpServer>();
    httpServer.IPAddress = IPAddress.Any;
    httpServer.Port = 80;
    await httpServer.Start(OnRequest);
}

void OnRequest(IHttpRequest request)
{
    request.SendResponse(new HttpResponse(HttpStatus.Ok)
    {
        Content = new StringResponseContent("Hello World!", "text/plain")
    });
}
```

## Additional notes

There are probably plenty of things to do to improve this library, some feel free to contribute.


*I spent a lot of my free time to develop this library, so if you like it, feel free to buy me <s>a beer</s> some vegetables ;)*

[![](https://www.paypalobjects.com/en_US/FR/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate?hosted_button_id=7HKHQ5Z72CS32)
