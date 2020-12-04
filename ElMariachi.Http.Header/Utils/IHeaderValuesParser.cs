using System.Collections.Generic;

namespace ElMariachi.Http.Header.Utils
{
    public interface IHeaderValuesParser
    {
        public IEnumerable<string> ParseInterpreted(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter);

        public IEnumerable<ParsedValue> Parse(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter);
    }

    public class ParsedValue
    {
        public ParsedValue(string raw, string? interpreted)
        {
            Raw = raw;
            Interpreted = interpreted;
        }

        public string Raw { get; }

        public string? Interpreted { get; }
    }
}