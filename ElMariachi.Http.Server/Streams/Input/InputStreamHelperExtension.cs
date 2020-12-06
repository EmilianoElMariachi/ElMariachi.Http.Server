using System.IO;
using System.Text;
using ElMariachi.Http.Exceptions;

namespace ElMariachi.Http.Server.Streams.Input
{
    public static class InputStreamHelperExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="limit">Any negative value to specify no limit</param>
        /// <returns></returns>
        public static string ReadUntilNewLine(this Stream stream, int limit)
        {
            var realStream = limit < 0 ? stream : new LimiterInputStream(stream, limit);

            var sb = new StringBuilder();
            while (true)
            {
                var c = realStream.ReadChar();

                if (c == '\r')
                {
                    var c2 = realStream.ReadChar();

                    if (c2 == '\n')
                        return sb.ToString();

                    sb.Append(c);
                    sb.Append(c2);
                    continue;
                }

                sb.Append(c);
            }
        }


        /// <summary>
        /// Read until a space is found.
        /// Returns the read string without the space.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="StreamLimitException"></exception>
        public static string ReadUntilSpace(this Stream stream, int limit)
        {
            var realStream = limit < 0 ? stream : new LimiterInputStream(stream, limit);

            var sb = new StringBuilder();
            while (true)
            {
                var c = realStream.ReadChar();
                if (c == ' ')
                    return sb.ToString();
                sb.Append(c);
            }
        }

        public static char ReadChar(this Stream stream)
        {
            var oneByteArray = new byte[1];
            var nbBytesRead = stream.Read(oneByteArray, 0, 1);

            if (nbBytesRead != 1)
                throw new StreamEndException();

            return (char)oneByteArray[0];
        }

    }
}
