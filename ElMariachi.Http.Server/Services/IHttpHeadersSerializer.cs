using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    public interface IHttpHeadersSerializer
    {
        string Serialize(IHttpHeaders headers);
    }
}