using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class ContentRangeHeaderTest
    {
        private readonly ContentRangeHeader _contentRangeHeader;

        public ContentRangeHeaderTest()
        {
            _contentRangeHeader = new ContentRangeHeader();
        }

        [Fact]
        public void InitialState()
        {
            var contentRangeHeader = new ContentRangeHeader();
            Assert.Null(contentRangeHeader.RawValue);
            Assert.Null(contentRangeHeader.Unit);
            Assert.Null(contentRangeHeader.Range);
        }

        [Fact]
        public void WhenRawValueIsNull_UpdatingAPropertyUpdatesRawValue()
        {
            var contentRangeHeader = new ContentRangeHeader();
            Assert.Null(contentRangeHeader.RawValue);

            contentRangeHeader.Unit = "bytes";

            Assert.Equal("bytes */*", contentRangeHeader.RawValue);

            contentRangeHeader.RawValue = null;
            Assert.Null(contentRangeHeader.RawValue);

            contentRangeHeader.Size = 5000;

            Assert.Equal(" */5000", contentRangeHeader.RawValue);

            contentRangeHeader.RawValue = null;
            Assert.Null(contentRangeHeader.RawValue);

            contentRangeHeader.Range = new Range(5, 10);

            Assert.Equal(" 5-10/*", contentRangeHeader.RawValue);
        }

        [Theory]
        [InlineData(" bytes 200-1000/67589 ", "bytes", 200, 1000, 67589)]
        [InlineData(" bytes  200  - 1000 / 67589 ", "bytes", 200, 1000, 67589)]
        [InlineData(" bits  0  - 5000 / 5000 ", "bits", 0, 5000, 5000)]
        public void RawValue_IsParsed(string rawValue, string expectedUnit, long? expectedStart, long? expectedEnd, long? expectedSize)
        {
            _contentRangeHeader.RawValue = rawValue;

            Assert.Equal(expectedUnit, _contentRangeHeader.Unit);
            Assert.Equal(expectedStart, _contentRangeHeader.Range.Start);
            Assert.Equal(expectedEnd, _contentRangeHeader.Range.End);
            Assert.Equal(expectedSize, _contentRangeHeader.Size);

            Assert.Equal(rawValue, _contentRangeHeader.RawValue);
        }

        [Theory]
        [InlineData(" bytes 200-1000/* ", "bytes", 200, 1000)]
        [InlineData(" bytes  200  - 1000 / * ", "bytes", 200, 1000)]
        [InlineData(" bits  0  - 5000 / * ", "bits", 0, 5000)]
        public void RawValue_IsParsedWhenSizeIsUnknown(string rawValue, string expectedUnit, long? expectedStart, long? expectedEnd)
        {
            _contentRangeHeader.RawValue = rawValue;

            Assert.Equal(expectedUnit, _contentRangeHeader.Unit);
            Assert.Equal(expectedStart, _contentRangeHeader.Range.Start);
            Assert.Equal(expectedEnd, _contentRangeHeader.Range.End);
            Assert.Null(_contentRangeHeader.Size);

            Assert.Equal(rawValue, _contentRangeHeader.RawValue);
        }

        [Theory]
        [InlineData(" bytes * / 50 ", "bytes", 50)]
        [InlineData(" bytes  */ 200 ", "bytes", 200)]
        [InlineData(" bits  * / 5000 ", "bits", 5000)]
        public void RawValue_IsParsedWhenRangeIsUnknown(string rawValue, string expectedUnit, long? expectedSize)
        {
            _contentRangeHeader.RawValue = rawValue;

            Assert.Equal(expectedUnit, _contentRangeHeader.Unit);
            Assert.Equal(expectedSize, _contentRangeHeader.Size);

            Assert.Equal(rawValue, _contentRangeHeader.RawValue);

        }

        [Theory]
        [InlineData(" bytes */* ", "bytes")]
        [InlineData(" bytes  * /* ", "bytes")]
        [InlineData(" bits  * / * ", "bits")]
        public void RawValue_IsParsedWhenRangeAndSizeIsUnknown(string rawValue, string expectedUnit)
        {
            _contentRangeHeader.RawValue = rawValue;

            Assert.Equal(expectedUnit, _contentRangeHeader.Unit);
            Assert.Null(_contentRangeHeader.Range);
            Assert.Null(_contentRangeHeader.Size);

            Assert.Equal(rawValue, _contentRangeHeader.RawValue);
        }

        [Theory]
        [InlineData("bytes")]
        [InlineData(" bytes ")]
        [InlineData("Mo")]
        public void SettingUnit_UpdatesRawValue(string unit)
        {
            var suffix = " 200-1000/67589";
            _contentRangeHeader.RawValue = "SomeUnit" + suffix;

            _contentRangeHeader.Unit = unit;

            Assert.Equal(unit + suffix, _contentRangeHeader.RawValue);
        }

        [Fact]
        public void SettingsRangeToNull_UpdatesRawValue()
        {
            _contentRangeHeader.RawValue = "bytes 200-1000/67589";
            _contentRangeHeader.Range = null;
            Assert.Equal("bytes */67589", _contentRangeHeader.RawValue);
        }

        [Fact]
        public void SettingsRange_UpdatesRawValue()
        {
            _contentRangeHeader.RawValue = "bytes 200-1000/67589";
            _contentRangeHeader.Range = new Range(10, 50);
            Assert.Equal("bytes 10-50/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range = new Range(null, null);
            Assert.Equal("bytes -/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range = new Range(15, null);
            Assert.Equal("bytes 15-/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range = new Range(null, 154);
            Assert.Equal("bytes -154/67589", _contentRangeHeader.RawValue);
        }

        [Fact]
        public void SettingsRangeStartOrEnd_UpdatesRawValue()
        {
            _contentRangeHeader.RawValue = "bytes 200-1000/67589";

            _contentRangeHeader.Range.End = 50;

            Assert.Equal("bytes 200-50/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range.Start = 20;

            Assert.Equal("bytes 20-50/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range.Start = null;

            Assert.Equal("bytes -50/67589", _contentRangeHeader.RawValue);

            _contentRangeHeader.Range.End = null;

            Assert.Equal("bytes -/67589", _contentRangeHeader.RawValue);
        }

        [Fact]
        public void SettingSize_UpdatesRawValue()
        {
            _contentRangeHeader.RawValue = "bytes 200-1000/67589";

            _contentRangeHeader.Size = 10;

            Assert.Equal("bytes 200-1000/10", _contentRangeHeader.RawValue);

            _contentRangeHeader.Size = 50;

            Assert.Equal("bytes 200-1000/50", _contentRangeHeader.RawValue);

            _contentRangeHeader.Size = null;

            Assert.Equal("bytes 200-1000/*", _contentRangeHeader.RawValue);
        }

        [Fact]
        public void Unset_NullifiesRawValue()
        {
            _contentRangeHeader.RawValue = "bytes 200-1000/67589";

            _contentRangeHeader.Unset();

            Assert.Null(_contentRangeHeader.RawValue);
            Assert.Null(_contentRangeHeader.Unit);
            Assert.Null(_contentRangeHeader.Range);
        }
    }
}
