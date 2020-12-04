using System.Text;
using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class ContentTypeHeaderTest
    {
        private readonly ContentTypeHeader _contentTypeHeader;

        public ContentTypeHeaderTest()
        {
            _contentTypeHeader = new ContentTypeHeader();
        }

        [Fact]
        public void InitialState()
        {
            Assert.Null(_contentTypeHeader.RawValue);
            Assert.Null(_contentTypeHeader.SubType);
            Assert.Null(_contentTypeHeader.Type);
            Assert.Null(_contentTypeHeader.Charset);
            Assert.Null(_contentTypeHeader.Boundary);

            Assert.Equal("Content-Type", _contentTypeHeader.Name);
        }

        [Theory]
        [InlineData("application/javascript", "application", "javascript")]
        [InlineData("Plain/Text", "Plain", "Text")]
        [InlineData("DummyTest ; Plain/Text ; Something else", "Plain", "Text")]
        public void MediaTypeIsParsed(string contentType, string type, string subType)
        {
            var contentTypeHeader = _contentTypeHeader;

            contentTypeHeader.RawValue = contentType;

            Assert.Equal($"{type}/{subType}", contentTypeHeader.MediaType);
            Assert.Equal(type, contentTypeHeader.Type);
            Assert.Equal(subType, contentTypeHeader.SubType);
        }

        [Theory]
        [InlineData("application/javascript; charset = utf-8", "utf-8")]
        [InlineData("application/javascript; charset = \"utf-8\"", "utf-8")]
        [InlineData("charset=iso-8859-1 ;some Text", "iso-8859-1")]
        [InlineData("dummy;charset= us-ascii ;some Text", "us-ascii")]

        public void CharsetIsParsed(string contentType, string expectedCharset)
        {
            var contentTypeHeader = _contentTypeHeader;

            contentTypeHeader.RawValue = contentType;

            var charSet = contentTypeHeader.Charset;
            Assert.NotNull(charSet);
            Assert.Equal(expectedCharset, charSet.WebName);
        }

        [Theory]
        [InlineData("plain/text;", " multipart/form-data ", " multipart/form-data ")]
        [InlineData("application/javascript; charset=utf-8", "audio/midi", "audio/midi;charset=utf-8")]
        [InlineData("plain/text; charset = utf-8", " multipart/form-data ", " multipart/form-data ;charset=utf-8")]
        public void SettingMediaType_UpdatesRawValue(string initialContentType, string mediaType, string expectedContentType)
        {
            var contentTypeHeader = _contentTypeHeader;

            contentTypeHeader.RawValue = initialContentType;

            contentTypeHeader.MediaType = mediaType;

            Assert.Equal(expectedContentType, contentTypeHeader.RawValue);
        }

        [Theory]
        [InlineData("application/javascript", "utf-8", "application/javascript;charset=utf-8")]
        [InlineData("plain/text; charset = utf-8", "iso-8859-1", "plain/text;charset=iso-8859-1")]
        public void SettingCharset_UpdatesRawValue(string initialContentType, string encodingName, string expectedContentType)
        {
            var contentTypeHeader = _contentTypeHeader;

            contentTypeHeader.RawValue = initialContentType;

            contentTypeHeader.Charset = Encoding.GetEncoding(encodingName);

            Assert.Equal(expectedContentType, contentTypeHeader.RawValue);
        }

        [Theory]
        [InlineData("multipart/byteranges", "ABCDE-Boundary-12345", "multipart/byteranges;boundary=ABCDE-Boundary-12345")]
        public void SettingBoundary_UpdatesRawValue(string initialContentType, string boundary, string expectedContentType)
        {
            var contentTypeHeader = _contentTypeHeader;

            contentTypeHeader.RawValue = initialContentType;

            contentTypeHeader.Boundary = boundary;

            Assert.Equal(expectedContentType, contentTypeHeader.RawValue);
        }
    }
}
