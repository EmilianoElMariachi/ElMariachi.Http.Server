using System;
using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class DateHeaderTest
    {
        private readonly DateHeader _dateHeader;

        public DateHeaderTest()
        {
            _dateHeader = new DateHeader();
        }

        [Fact]
        public void DefaultValues()
        {
            var dateHeader = new DateHeader();
            Assert.Null(dateHeader.Value);
            Assert.Null(dateHeader.RawValue);
        }

        [Theory]
        [InlineData("Mon, 14 Nov 1994 08:12:31 GMT", 1994, 11, 14, 8, 12, 31)]
        [InlineData("Tue, 15 Nov 1994 08:12:31 GMT", 1994, 11, 15, 8, 12, 31)]
        [InlineData("Wed, 16 Nov 1994 08:12:31 GMT", 1994, 11, 16, 8, 12, 31)]
        [InlineData("Thu, 17 Nov 1994 08:12:31 GMT", 1994, 11, 17, 8, 12, 31)]
        [InlineData("Fri, 18 Nov 1994 08:12:31 GMT", 1994, 11, 18, 8, 12, 31)]
        [InlineData("Sat, 19 Nov 1994 08:12:31 GMT", 1994, 11, 19, 8, 12, 31)]
        [InlineData("Sun, 20 Nov 1994 08:12:31 GMT", 1994, 11, 20, 8, 12, 31)]
        [InlineData("Mon, 14 Nov 1994 00:00:00 GMT", 1994, 11, 14, 0, 0, 0)]
        [InlineData("Mon, 14 Nov 1994 23:59:59 GMT", 1994, 11, 14, 23, 59, 59)]
        [InlineData("Wed, 22 Jan 2020 13:01:02 GMT", 2020, 1, 22, 13, 1, 2)]
        [InlineData("Sat, 22 Feb 2020 13:01:02 GMT", 2020, 2, 22, 13, 1, 2)]
        [InlineData("Sun, 22 Mar 2020 13:01:02 GMT", 2020, 3, 22, 13, 1, 2)]
        [InlineData("Wed, 22 Apr 2020 13:01:02 GMT", 2020, 4, 22, 13, 1, 2)]
        [InlineData("Fri, 22 May 2020 13:01:02 GMT", 2020, 5, 22, 13, 1, 2)]
        [InlineData("Mon, 22 Jun 2020 13:01:02 GMT", 2020, 6, 22, 13, 1, 2)]
        [InlineData("Wed, 22 Jul 2020 13:01:02 GMT", 2020, 7, 22, 13, 1, 2)]
        [InlineData("Sat, 22 Aug 2020 13:01:02 GMT", 2020, 8, 22, 13, 1, 2)]
        [InlineData("Tue, 22 Sep 2020 13:01:02 GMT", 2020, 9, 22, 13, 1, 2)]
        [InlineData("Thu, 22 Oct 2020 13:01:02 GMT", 2020, 10, 22, 13, 1, 2)]
        [InlineData("Sun, 22 Nov 2020 13:01:02 GMT", 2020, 11, 22, 13, 1, 2)]
        [InlineData("Tue, 22 Dec 2020 13:01:02 GMT", 2020, 12, 22, 13, 1, 2)]
        [InlineData("tue, 22 Dec 2020 13:01:02 GMT", 2020, 12, 22, 13, 1, 2)]
        [InlineData("Tue, 22 dec 2020 13:01:02 GMT", 2020, 12, 22, 13, 1, 2)]
        [InlineData("Tue, 22 Dec 2020 13:01:02 gmT", 2020, 12, 22, 13, 1, 2)]
        [InlineData(" Tue, 22 Dec 2020 13:01:02 gmT ", 2020, 12, 22, 13, 1, 2)]
        public void ParsesDateFromRawValue(string rawValue, int year, int month, int day, int hour, int min, int sec)
        {
            var expectedDate = new DateTime(year, month, day, hour, min, sec);

            _dateHeader.RawValue = rawValue;

            Assert.Equal(expectedDate, _dateHeader.Value);
            Assert.Equal(DateTimeKind.Utc, _dateHeader.Value.Value.Kind);

            Assert.Equal(rawValue, _dateHeader.RawValue);
        }

        [Theory]
        [InlineData("Tue, 14 Nov 1994 08:12:31 GMT")]
        [InlineData("Wed, 14 Nov 1994 08:12:31 GMT")]
        [InlineData("Thu, 14 Nov 1994 08:12:31 GMT")]
        [InlineData("Fri, 14 Nov 1994 08:12:31 GMT")]
        [InlineData("Sat, 14 Nov 1994 08:12:31 GMT")]
        [InlineData("Sun, 14 Nov 1994 08:12:31 GMT")]
        public void DoNotParseDateWhenDayOfWeekDoNotMatchRealDayOfWeek(string rawValue)
        {
            _dateHeader.RawValue = rawValue;

            Assert.Null(_dateHeader.Value);

            Assert.Equal(rawValue, _dateHeader.RawValue);
        }

        [Theory]
        [InlineData("Sat, 00 Nov 2020 13:01:02 GMT")]
        [InlineData("Sun, 01 Dec 2020 13:01:02 GMT")]
        [InlineData("Sun, 01 AVB 2020 13:01:02 GMT")]
        [InlineData("Sun, 01 Dek 2020 13:01:02 GMT")]
        [InlineData("Sun, 01 Nov 2020 24:01:02 GMT")]
        [InlineData("Sun, 01 Nov 2020 01:60:02 GMT")]
        [InlineData("Sun, 01 Nov 2020 01:50:60 GMT")]
        [InlineData("Sun, 01 Nov 2020 01:50:59 GMT0")]
        [InlineData("Sun, 01 Nov 2020 01:50:59 GST")]
        public void DoNotParseDateWhenDateIsInvalid(string rawValue)
        {
            _dateHeader.RawValue = rawValue;

            Assert.Null(_dateHeader.Value);

            Assert.Equal(rawValue, _dateHeader.RawValue);
        }

        [Fact]
        public void SettingNonUtcDateThrows()
        {
            foreach (DateTimeKind dateTimeKind in Enum.GetValues(typeof(DateTimeKind)))
            {
                if (dateTimeKind == DateTimeKind.Utc)
                    continue;
                var ex = Assert.Throws<ArgumentException>(() =>
                {
                    _dateHeader.Value = new DateTime(0, DateTimeKind.Unspecified);
                });
                Assert.Equal("Header date should be of UTC kind.", ex.Message);
            }
        }

        [Fact]
        public void SettingTheDateUpdatesRawValue()
        {
            _dateHeader.Value = new DateTime(0, DateTimeKind.Utc);
            Assert.Equal("Mon, 01 Jan 0001 00:00:00 GMT", _dateHeader.RawValue);

            _dateHeader.Value = new DateTime(2000, 10, 1, 20, 10, 50, DateTimeKind.Utc);
            Assert.Equal("Sun, 01 Oct 2000 20:10:50 GMT", _dateHeader.RawValue);
        }

        [Fact]
        public void SettingRawValueToNullNullifiesDate()
        {
            _dateHeader.Value = DateTime.UtcNow;
            Assert.NotNull(_dateHeader.Value);

            _dateHeader.RawValue = null;

            Assert.Null(_dateHeader.Value);
        }

        [Fact]
        public void SettingDateToNullNullifiesRawValue()
        {
            _dateHeader.RawValue = "Some Header";
            Assert.Equal("Some Header", _dateHeader.RawValue);
            _dateHeader.Value = null;

            Assert.Null(_dateHeader.RawValue);
        }
    }
}
