using ElMariachi.Http.Server.Models;

namespace ElMariachi.Http.Server.Services.Internals
{
    internal interface IHttpResponseSender
    {
        bool IsResponseSent { get; }

        public bool CloseConnection { get; }

        IHttpResponse? SentResponse { get; }

        void Send(IHttpResponse response);

        event ResponseSentHandler ResponseSent;
    }

    internal delegate void ResponseSentHandler(object sender, ResponseSentHandlerArgs args);

    internal class ResponseSentHandlerArgs  
    {
    }
}