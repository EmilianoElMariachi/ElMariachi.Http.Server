using System.Collections.Generic;

namespace ElMariachi.Http.Header.Managed
{
    public interface IRangeHeader : IHeader
    {
        string? Unit { get; set; }

        IList<IRange> Ranges { get; }
    }
}