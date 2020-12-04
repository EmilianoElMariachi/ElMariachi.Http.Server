namespace ElMariachi.Http.Header.Managed
{
    public interface IContentLengthHeader : IManagedHeader
    {
        long? Value { get; set; }
    }
}