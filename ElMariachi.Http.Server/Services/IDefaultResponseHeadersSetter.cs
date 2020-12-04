using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    public interface IDefaultResponseHeadersSetter
    {
        void Set(IHttpResponseHeaders headers);
    }
}