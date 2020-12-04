using System.Collections.Generic;

namespace ElMariachi.Http.Header.Managed
{
    public interface ITransferEncodingHeader : IManagedHeader
    {
        IList<string> Values { get; }
    }
}