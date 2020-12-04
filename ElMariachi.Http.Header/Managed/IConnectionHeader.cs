namespace ElMariachi.Http.Header.Managed
{
    public interface IConnectionHeader : IManagedHeader
    {
        bool KeepAlive { get; set; }

        bool Close { get; set; }
    }
}