using System;

namespace ElMariachi.Http.Header.Managed
{
    public interface IDateHeader : IHeader
    {

        /// <summary>
        /// Get or set the date.
        /// Date should be UTC.
        /// </summary>
        DateTime? Value { get; set; }

    }
}
