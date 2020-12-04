using System.Collections.Generic;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header
{
    public abstract class Header : IHeader
    {
        public abstract string Name { get; }

        public abstract HeaderType Type { get; }

        public string? RawValue
        {
            get => GetRawValue();
            set
            {
                if (value != null && value.Contains(HttpConst.LineReturn))
                    throw new HeaderFormatException("New line sequence «\\r\\n» is not allowed in a header value.");

                SetRawValue(value);
            }
        }

        protected abstract string? GetRawValue();

        protected abstract void SetRawValue(string? rawValue);

        public void Deconstruct(out string name, out string? rawValue)
        {
            name = Name;
            rawValue = RawValue;
        }

        public IEnumerable<ParsedValue> ParseValues(char delimiter = HttpConst.DefaultHeaderDelimiter)
        {
            return HeaderValuesParser.ParseStatic(RawValue, delimiter);
        }
    }

    public enum HeaderType
    {
        Managed,
        Unmanaged,
    }

}