using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header
{
    public interface IHeader
    {
        /// <summary>
        /// Get the name of the header (The header name is always case insensitive).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get or set the raw value of the header.
        /// The «get» value is always equal to the «set» value.
        /// </summary>
        /// <exception cref="HeaderFormatException"></exception>
        public string? RawValue { get; set; }

        void Deconstruct(out string name, out string? rawValue);

        /// <summary>
        /// Parses the values of current header.
        /// Use this method for headers known to be a list of values separated by commas, or specify the delimiter.
        /// </summary>
        /// <param name="delimiter">The delimiter to use for parsing the values</param>
        /// <returns></returns>
        [Pure]
        IEnumerable<ParsedValue> ParseValues(char delimiter = HttpConst.DefaultHeaderDelimiter);
    }
}