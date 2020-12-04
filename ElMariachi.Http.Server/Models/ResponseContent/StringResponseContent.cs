using System;
using System.Text;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Models.ResponseContent
{
    public class StringResponseContent : ByteArrayResponseContent
    {
        public const string DefaultTextMediaType = "text/plain";

        private static readonly Encoding _defaultCharset = Encoding.GetEncoding("utf-8");
        private readonly string _mediaType;
        private readonly Encoding _charset;

        public StringResponseContent(string content) : this(content, DefaultTextMediaType, _defaultCharset)
        {
        }

        public StringResponseContent(string content, string mediaType) : this(content, mediaType, _defaultCharset)
        {
        }

        public StringResponseContent(string content, string mediaType, Encoding charset) : base(StrToBytes(content, charset))
        {
            _mediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
            _charset = charset ?? throw new ArgumentNullException(nameof(charset));
        }

        public override void FillHeaders(IHttpResponseHeaders headers)
        {
            base.FillHeaders(headers);
            var contentType = headers.ContentType;
            if (contentType.RawValue != null)
                throw new InvalidOperationException($"Using the {this.GetType().Name} will automatically position the {HttpConst.Headers.ContentType} header which is actually already positioned.");

            contentType.MediaType = _mediaType;
            contentType.Charset = _charset;
        }

        private static byte[] StrToBytes(string content, Encoding encoding)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            var bytes = encoding.GetBytes(content);
            return bytes;
        }


    }
}