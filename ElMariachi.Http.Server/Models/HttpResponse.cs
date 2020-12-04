using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Models.ResponseContent;
using ElMariachi.Http.Server.Services.Internals;

namespace ElMariachi.Http.Server.Models
{

    /// <summary>
    /// NOTE: This class is a model. It is important to allow this model to be temporarily be in some non sendable states.
    /// The <see cref="IHttpResponseSender"/> is responsible for controlling the sendable state of this model.
    /// Therefore it is preferable to be able to accept null property values rather than throwing exceptions.
    /// </summary>
    public class HttpResponse : IHttpResponse
    {
        public HttpResponse()
        {
        }

        public HttpResponse(HttpStatus status)
        {
            Status = status;
        }

        public HttpStatus Status { get; set; /*NOTE: do not throw on null, see class note.*/ }

        public IHttpResponseHeaders Headers { get; } = new HttpResponseHeaders();

        public IHttpResponseContent Content { get; set; /*NOTE: do not throw on null, see class note.*/ } = new EmptyResponseContent();

        public IHttpRequest? Request { get; set; }

    }
}