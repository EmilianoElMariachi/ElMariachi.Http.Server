namespace ElMariachi.Http.Header.Managed
{
    public interface IContentRangeHeader : IManagedHeader
    {
        string? Unit { get; set; }

        IRange? Range { get; set; }

        long? Size { get; set; }
    }
}