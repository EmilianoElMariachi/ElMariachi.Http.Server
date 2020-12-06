using System.Net.Sockets;

namespace ElMariachi.Http.Server.Services.Internals
{
    internal interface IHttpResponseSenderFactory
    {
        IHttpResponseSender Create(NetworkStream networkStream, int maxInputStreamCleaning);
    }
}
