using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using ElMariachi.Http.Header.Exceptions;

namespace ElMariachi.Http.Header.Utils
{
    public class HeaderValuesParser : IHeaderValuesParser
    {

        [Pure]
        public IEnumerable<string> ParseInterpreted(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter)
        {
            return ParseInterpretedStatic(rawHeaderValue, delimiter);
        }

        [Pure]
        public IEnumerable<ParsedValue> Parse(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter)
        {
            return ParseStatic(rawHeaderValue, delimiter);
        }

        [Pure]
        public static IEnumerable<string> ParseInterpretedStatic(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter)
        {
            foreach (var parsedItem in ParseStatic(rawHeaderValue, delimiter))
            {
                var interpreted = parsedItem.Interpreted;
                if (interpreted != null)
                    yield return interpreted;
            }
        }

        [Pure]
        public static IEnumerable<ParsedValue> ParseStatic(string? rawHeaderValue, char delimiter = HttpConst.DefaultHeaderDelimiter)
        {
            if (rawHeaderValue == null)
                yield break;

            if (rawHeaderValue.Length == 0)
            {
                yield return new ParsedValue("", null);
                yield break;
            }

            var buffRaw = new StringBuilder();
            StringBuilder? buffInterpreted = null;
            var buffPostWhiteSpaces = new StringBuilder();

            var inDelimitedStr = false;
            var cPrev = '\0';
            foreach (var c in rawHeaderValue)
            {
                if (inDelimitedStr)
                {
                    // Here we are in delimited string (buffInterpreted can't be null)

                    switch (c)
                    {
                        case '\\':
                            if (cPrev == '\\') // Back slash followed by back slash, we take the first one and wait for the next char
                                buffInterpreted.Append(cPrev);

                            break;
                        case '"':
                            if (cPrev != '\\')
                                inDelimitedStr = false;
                            else
                                buffInterpreted.Append(c); // Here, double quote was escaped (sequence \" => ")

                            break;
                        default:
                            if (cPrev == '\\') // Back slash followed by a normal char (no escape, we preserve both)
                            {
                                buffInterpreted.Append(cPrev);
                                buffInterpreted.Append(c);
                            }
                            else
                                buffInterpreted.Append(c);

                            break;
                    }
                }
                else
                {
                    // Here we are outside of a delimited string

                    if (c == '"')
                    {
                        buffInterpreted ??= new StringBuilder();
                        buffInterpreted.Append(buffPostWhiteSpaces);
                        buffPostWhiteSpaces.Clear();

                        inDelimitedStr = true;
                    }
                    else if (c == delimiter)
                    {
                        yield return new ParsedValue(buffRaw.ToString(), buffInterpreted?.ToString());
                        buffRaw.Clear();
                        buffPostWhiteSpaces.Clear();
                        buffInterpreted = null;

                        goto ContinueWithoutAppendRaw; // NOTE that here, we won't collect the comma
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        if (buffInterpreted?.Length > 0)
                            buffPostWhiteSpaces.Append(c);
                    }
                    else
                    {
                        // Here, char is a useful char
                        if (buffInterpreted?.Length > 0 && buffPostWhiteSpaces.Length > 0)
                        {
                            // Here, we found a useful char followed by some whitespaces, followed by userful chars, so we take the whitespaces
                            buffInterpreted.Append(buffPostWhiteSpaces);
                            buffPostWhiteSpaces.Clear();
                        }

                        buffInterpreted ??= new StringBuilder();
                        buffInterpreted.Append(c);
                    }
                }

                buffRaw.Append(c);

            ContinueWithoutAppendRaw:
                cPrev = c;
            }

            if (inDelimitedStr)
                throw new HeaderFormatException($"Header value «{rawHeaderValue}» is missing a string end delimiter.");

            if (buffRaw.Length > 0 || cPrev == delimiter)
                yield return new ParsedValue(buffRaw.ToString(), buffInterpreted?.ToString());
        }


    }
}
