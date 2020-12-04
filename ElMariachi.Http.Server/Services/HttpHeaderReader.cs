using System;
using System.IO;
using System.Text;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Streams.Input;

namespace ElMariachi.Http.Server.Services
{
    public class HttpHeaderReader : IHttpHeaderReader
    {

        public IHttpHeader Read(Stream stream, int maxMethodNameSize, int maxHeadersSize, int maxRequestUriSize, Action? onFirstCharRead)
        {
            string methodUpper;
            try
            {
                var firstChar = stream.ReadChar();
                onFirstCharRead?.Invoke();
                methodUpper = firstChar + stream.ReadUntilSpace(maxMethodNameSize).Trim().ToUpper();
            }
            catch (StreamLimitException ex)
            {
                throw new RequestFormatException($"Http method not valid, max chars «{ex.Limit}» reached.");
            }

            string requestUriString;
            try
            {
                requestUriString = stream.ReadUntilSpace(maxRequestUriSize).Trim();
            }
            catch (StreamLimitException)
            {
                throw new RequestUriToolLongException(maxRequestUriSize);
            }

            Uri requestUri;
            try
            {
                requestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);
            }
            catch (Exception ex)
            {
                throw new RequestFormatException($"Request URI not valid: {ex.Message}");
            }

            string httpVersion;
            try
            {
                httpVersion = stream.ReadUntilNewLine(20).Trim();
            }
            catch (StreamLimitException ex)
            {
                throw new RequestFormatException($"Http version not valid, max chars «{ex.Limit}» reached.");
            }

            var headers = ReadHeaders(stream, maxHeadersSize);

            return new HttpHeader
            {
                HttpVersion = httpVersion,
                Method = methodUpper,
                RequestUri = requestUri,
                Headers = headers
            };
        }

        private static HttpRequestHeaders ReadHeaders(Stream stream, int maxHeadersSize)
        {
            var realStream = maxHeadersSize < 0 ? stream : new LimiterInputStream(stream, maxHeadersSize);

            var headers = new HttpRequestHeaders();
            while (ReadHeader(realStream, out var header, out var value))
            {
                headers[header] = value;
            }

            return headers;
        }

        private static bool ReadHeader(Stream stream, out string? header, out string? value)
        {
            header = null;
            value = null;

            var buff = new StringBuilder();
            while (true)
            {
                var c = stream.ReadChar();

                switch (c)
                {
                    case '\r':
                        {
                            var c2 = stream.ReadChar();
                            if (c2 == '\n')
                            {
                                var buffStr = buff.ToString();

                                if (header == null && buffStr != "")
                                    throw new RequestFormatException("Invalid header, value separator «:» not found!");

                                if (header == null && buffStr == "")
                                    return false;

                                value = buffStr.Trim();
                                return true;
                            }

                            buff.Append(c);
                            buff.Append(c2);
                            break;
                        }
                    case ':' when header == null:
                        {
                            header = buff.ToString().Trim();
                            buff.Clear();
                            break;
                        }
                    default:
                        {
                            buff.Append(c);
                            break;
                        }
                }
            }
        }

    }
}