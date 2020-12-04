namespace ElMariachi.Http.Header.Utils
{
    public interface IHttpHeadersSerializer
    {
        string Serialize(IHttpHeaders headers);
    }
}