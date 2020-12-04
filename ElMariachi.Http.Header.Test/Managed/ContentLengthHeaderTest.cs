using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class ContentLengthHeaderTest
    {
        private readonly ContentLengthHeader _contentLengthHeader;

        public ContentLengthHeaderTest()
        {
            _contentLengthHeader = new ContentLengthHeader();
        }

        [Theory]
        [InlineData("51651", 51651)]
        [InlineData(" 51651 ", 51651)]
        public void RawValue_InitializesTheValueAndPreservesRawValue(string rawValue, long expectedValue)
        {
            _contentLengthHeader.RawValue = rawValue;

            Assert.Equal(expectedValue, _contentLengthHeader.Value);
            Assert.Equal(rawValue, _contentLengthHeader.RawValue);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(5165, "5165")]
        public void SettingTheValue_OverridesTheRawValue(long value, string expectedRawValue)
        {
            _contentLengthHeader.RawValue = "8794";

            _contentLengthHeader.Value = value;

            Assert.Equal(expectedRawValue, _contentLengthHeader.RawValue);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("5165")]
        public void SettingTheRawValueToNull_NullifiesTheValue(string rawValue)
        {
            _contentLengthHeader.RawValue = rawValue;
            Assert.NotNull(_contentLengthHeader.Value);

            _contentLengthHeader.RawValue = null;
            Assert.Null(_contentLengthHeader.Value);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("-654")]
        public void SettingRawValueWithNegativeNumberThrows(string rawValue)
        {
            var ex = Assert.Throws<HeaderFormatException>(() =>
            {
                _contentLengthHeader.RawValue = rawValue;
            });

            Assert.Equal($"Content-Length header value «{rawValue}» is not allowed to be negative.", ex.Message);
        }

        [Theory]
        [InlineData("a1")]
        [InlineData("6.4")]
        [InlineData("dsd sd sdfqs 6.4")]
        [InlineData(" \"51651\" ")]
        [InlineData(" \"0\" ")]
        [InlineData(" \t \" 151 \" \t ")]
        public void SettingRawValueWithInvalidNumberThrows(string rawValue)
        {
            var ex = Assert.Throws<HeaderFormatException>(() =>
            {
                _contentLengthHeader.RawValue = rawValue;
            });

            Assert.Equal($"Content-Length header value «{rawValue}» is not a valid positive number.", ex.Message);
        }


    }
}
