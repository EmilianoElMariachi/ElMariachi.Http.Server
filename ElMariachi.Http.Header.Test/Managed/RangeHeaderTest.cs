using System.Collections;
using System.Collections.Generic;
using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class RangeHeaderTest
    {
        private readonly RangeHeader _rangeHeader;

        public RangeHeaderTest()
        {
            _rangeHeader = new RangeHeader();
        }

        [Fact]
        public void HeaderNameIsRange()
        {
            Assert.Equal("Range", _rangeHeader.Name);
        }

        [Theory]
        [ClassData(typeof(ValidRangesTestCases))]
        internal void SettingRawValue_ParsesRanges(string headerValue, string expectedUnit, Range[] expectedRanges)
        {
            _rangeHeader.RawValue = headerValue;

            var ranges = _rangeHeader.Ranges;
            Assert.Equal(expectedRanges.Length, ranges.Count);

            for (var i = 0; i < expectedRanges.Length; i++)
            {
                Assert.Equal(expectedRanges[i].Start, ranges[i].Start);
                Assert.Equal(expectedRanges[i].End, ranges[i].End);
            }

            Assert.Equal(expectedUnit, _rangeHeader.Unit);
        }

        private class ValidRangesTestCases : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "bytes=0-200", "bytes", new[] { new Range { Start = 0, End = 200 } } };
                yield return new object[] { "bytes  = 0  -   200   ", "bytes", new[] { new Range { Start = 0, End = 200 } } };
                yield return new object[] { "bytes  =  - 200   ", "bytes", new[] { new Range { Start = null, End = 200 } } };
                yield return new object[] { "bytes  = 50 -  ", "bytes", new[] { new Range { Start = 50, End = null } } };
                yield return new object[] { " someUnit = 50 -  ", "someUnit", new[] { new Range { Start = 50, End = null } } };
                yield return new object[] { "bytes=200-1000, 2000-6576, 19000- ", "bytes", new[] { new Range { Start = 200, End = 1000 }, new Range { Start = 2000, End = 6576 }, new Range { Start = 19000, End = null } } };
                yield return new object[] { "  bytes   = 200 -  1000 , 2000  -   6576 , 19000  - ", "bytes", new[] { new Range { Start = 200, End = 1000 }, new Range { Start = 2000, End = 6576 }, new Range { Start = 19000, End = null } } };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(InvalidRangesTestCases))]
        internal void SettingRawValue_IfAtLeastOneRangeIsNotValidThenRangesPropertyIsEmpty(string headerValue)
        {
            _rangeHeader.RawValue = headerValue;

            Assert.Null(_rangeHeader.Unit);
            Assert.Empty(_rangeHeader.Ranges);

            Assert.Equal(headerValue, _rangeHeader.RawValue);
        }

        private class InvalidRangesTestCases : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "bytes=-" };
                yield return new object[] { "bytes=--51" };
                yield return new object[] { "bytes=-84-51" };
                yield return new object[] { "=84-51" };
                yield return new object[] { "   =84-51" };
                yield return new object[] { "bytes=84-51," };
                yield return new object[] { "bytes=84-51,51-a" };
                yield return new object[] { "bytes=a-51" };
                yield return new object[] { "bytes=52-b" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(null)]
        public void ChangingEndOfOneExistingRange_UpdatesTheRawValue(long? end)
        {
            _rangeHeader.RawValue = "bytes=200-1000";
            _rangeHeader.Ranges[0].End = end;

            Assert.Equal($"bytes=200-{end}", _rangeHeader.RawValue);
            Assert.Equal(200, _rangeHeader.Ranges[0].Start);
            Assert.Equal(end, _rangeHeader.Ranges[0].End);


            _rangeHeader.RawValue = "bytes= 200 - 1000 , 100-4516, 651 - 516";
            _rangeHeader.Ranges[2].End = end;

            Assert.Equal($"bytes=200-1000,100-4516,651-{end}", _rangeHeader.RawValue);
            Assert.Equal(651, _rangeHeader.Ranges[2].Start);
            Assert.Equal(end, _rangeHeader.Ranges[2].End);

            _rangeHeader.RawValue = "bytes = 200   -1000 ,  400- 4500";
            Assert.Equal(2, _rangeHeader.Ranges.Count);
            Assert.Equal(200, _rangeHeader.Ranges[0].Start);
            Assert.Equal(1000, _rangeHeader.Ranges[0].End);
            Assert.Equal(400, _rangeHeader.Ranges[1].Start);
            Assert.Equal(4500, _rangeHeader.Ranges[1].End);
            Assert.Equal("bytes = 200   -1000 ,  400- 4500", _rangeHeader.RawValue);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(null)]
        public void ChangingStartOfOneExistingRange_UpdatesTheRawValue(long? start)
        {
            _rangeHeader.RawValue = "bytes=200-1000";
            _rangeHeader.Ranges[0].Start = start;

            Assert.Equal($"bytes={start}-1000", _rangeHeader.RawValue);
            Assert.Equal(start, _rangeHeader.Ranges[0].Start);
            Assert.Equal(1000, _rangeHeader.Ranges[0].End);


            _rangeHeader.RawValue = "bytes=200-1000,400-4500";
            _rangeHeader.Ranges[1].Start = start;

            Assert.Equal($"bytes=200-1000,{start}-4500", _rangeHeader.RawValue);
            Assert.Equal(start, _rangeHeader.Ranges[1].Start);
            Assert.Equal(4500, _rangeHeader.Ranges[1].End);

            _rangeHeader.RawValue = "bytes = 200   -1000 ,  400- 4500";
            Assert.Equal(2, _rangeHeader.Ranges.Count);
            Assert.Equal(200, _rangeHeader.Ranges[0].Start);
            Assert.Equal(1000, _rangeHeader.Ranges[0].End);
            Assert.Equal(400, _rangeHeader.Ranges[1].Start);
            Assert.Equal(4500, _rangeHeader.Ranges[1].End);
            Assert.Equal("bytes = 200   -1000 ,  400- 4500", _rangeHeader.RawValue);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(10, 15)]
        [InlineData(null, 87)]
        [InlineData(78, null)]
        public void ChangingARange_UpdatesTheRawValue(long? start, long? end)
        {
            _rangeHeader.RawValue = "bytes=200-1000";
            _rangeHeader.Ranges[0] = new Range { Start = start, End = end };

            Assert.Equal($"bytes={start}-{end}", _rangeHeader.RawValue);
            Assert.Equal(start, _rangeHeader.Ranges[0].Start);
            Assert.Equal(end, _rangeHeader.Ranges[0].End);

            _rangeHeader.RawValue = "bytes = 200   -1000 ,  400- 4500";
            Assert.Equal(2, _rangeHeader.Ranges.Count);
            Assert.Equal(200, _rangeHeader.Ranges[0].Start);
            Assert.Equal(1000, _rangeHeader.Ranges[0].End);
            Assert.Equal(400, _rangeHeader.Ranges[1].Start);
            Assert.Equal(4500, _rangeHeader.Ranges[1].End);
            Assert.Equal("bytes = 200   -1000 ,  400- 4500", _rangeHeader.RawValue);
        }

        [Fact]
        public void Unit_UpdatesTheRawValue()
        {
            _rangeHeader.RawValue = "bytes= 200 - 1000";
            _rangeHeader.Unit = "bits";
            Assert.Equal("bits=200-1000",_rangeHeader.RawValue);
        }

        [Theory]
        [InlineData("bytes=0-200", "bytes=")]
        [InlineData("bytes=0-200,50-1050", "bytes=")]
        [InlineData("bits=0-200,50-1050", "bits=")]
        public void ClearingRanges_UpdatesRawValue(string rawValue, string expectedRawValue)
        {
            _rangeHeader.RawValue = rawValue;

            Assert.NotEmpty(_rangeHeader.Ranges);

            _rangeHeader.Ranges.Clear();

            Assert.Equal(expectedRawValue, _rangeHeader.RawValue);
        }
    }
}
