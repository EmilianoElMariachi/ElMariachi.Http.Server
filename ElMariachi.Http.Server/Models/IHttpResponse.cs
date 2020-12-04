using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Models.ResponseContent;

namespace ElMariachi.Http.Server.Models
{
    public interface IHttpResponse
    {
        public HttpStatus Status { get; set; }

        IHttpResponseHeaders Headers { get; }

        IHttpRequest? Request { get; set; }

        /// <summary>
        /// Set the body of the response.
        /// </summary>
        IHttpResponseContent Content { get; set; }
    }
}