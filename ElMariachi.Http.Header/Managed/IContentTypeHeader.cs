using System.Text;

namespace ElMariachi.Http.Header.Managed
{
    public interface IContentTypeHeader : IManagedHeader
    {
        string? MediaType { get; set; }

        string? Type { get; }

        string? SubType { get; }

        Encoding? Charset { get; set; }

        string? Boundary { get; set; }
    }
}