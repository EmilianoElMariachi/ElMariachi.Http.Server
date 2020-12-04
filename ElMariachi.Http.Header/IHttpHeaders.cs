using System;
using System.Collections.Generic;

namespace ElMariachi.Http.Header
{
    public interface IHttpHeaders : IEnumerable<IHeader>
    {

        public event HeadersChangedHandler HeadersChanged;

        /// <summary>
        /// Get a set a header.
        /// Returns null if header doesn't exist.
        /// Setting a null value will remove the header if it exists.
        /// If header already exists, its value will be replaced.
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public string? this[string headerName] { get; set; }

        /// <summary>
        /// Return true if header exists
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        bool ContainsHeader(string headerName);

    }

    public delegate void HeadersChangedHandler(object sender, HeaderChangedHandlerArgs args);

    public class HeaderChangedHandlerArgs
    {
        public HeaderChangedHandlerArgs(string headerName, HeaderChangeType changeType)
        {
            HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
            ChangeType = changeType;
        }

        public string HeaderName { get; }

        public HeaderChangeType ChangeType { get; }

    }

    public enum HeaderChangeType
    {
        Added,
        Replaced,
        Removed
    }
}