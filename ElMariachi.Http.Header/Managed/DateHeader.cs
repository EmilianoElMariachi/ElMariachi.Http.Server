using System;
using System.Globalization;
using System.Linq;

namespace ElMariachi.Http.Header.Managed
{

    /// <summary>
    /// Chose date implementation is rfc1123-date. (Date: <shortWeekDay>, <day> <shortMonthName> <year> <hour>:<min>:<sec> GMT)
    /// Specification: https://tools.ietf.org/html/rfc2616#section-3.3.1
    /// </summary>
    public class DateHeader : ManagedHeader, IDateHeader
    {

        private static readonly DateTimeFormatInfo _dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

        private string? _rawValue;
        private DateTime? _date;

        public override string Name => HttpConst.Headers.Date;

        public DateTime? Value
        {
            get => _date;
            set
            {
                _date = value;
                if (value == null)
                    _rawValue = null;
                else
                {
                    var dateTime = value.Value;

                    if (dateTime.Kind != DateTimeKind.Utc)
                        throw new ArgumentException($"Header date should be of UTC kind.");

                    var dayName = _dateTimeFormat.GetAbbreviatedDayName(dateTime.DayOfWeek);
                    var monthName = _dateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);

                    _rawValue = $"{dayName}, {dateTime.Day:00} {monthName} {dateTime.Year:0000} {dateTime.Hour:00}:{dateTime.Minute:00}:{dateTime.Second:00} GMT";
                }
            }
        }

        protected override string? GetRawValue()
        {
            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;
            _date = null;

            if (rawValue == null)
                return;

            var stringReader = new StringReader(rawValue.Trim());

            var dayOfWeek = ParseWkDay(stringReader.Read(3));
            if (dayOfWeek == null)
                return;

            if (stringReader.Read(1) != ",")
                return;

            if (stringReader.Read(1) != " ")
                return;

            var day = IsValidDay(stringReader.ReadDigits(2));
            if (day == null)
                return;

            if (stringReader.Read(1) != " ")
                return;

            var month = ParseMonth(stringReader.Read(3));
            if (month == null)
                return;

            if (stringReader.Read(1) != " ")
                return;

            var year = stringReader.ReadDigits(4);
            if (year == null)
                return;

            if (stringReader.Read(1) != " ")
                return;

            var hour = IsValidHour(stringReader.ReadDigits(2));
            if (hour == null)
                return;

            if (stringReader.Read(1) != ":")
                return;

            var min = IsValidMin(stringReader.ReadDigits(2));
            if (min == null)
                return;

            if (stringReader.Read(1) != ":")
                return;

            var sec = IsValidSec(stringReader.ReadDigits(2));
            if (sec == null)
                return;

            if (!string.Equals(stringReader.Read(4), " GMT", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (stringReader.NbRemainingChars > 0)
                return;

            var dateTime = new DateTime(year.Value, month.Value, day.Value, hour.Value, min.Value, sec.Value, DateTimeKind.Utc);

            if (dateTime.DayOfWeek != dayOfWeek)
                return;

            _date = dateTime;
        }

        private static DayOfWeek? ParseWkDay(string? wkDay)
        {
            if (wkDay == null)
                return null;

            switch (wkDay.ToLower())
            {
                case "mon":
                    return DayOfWeek.Monday;
                case "tue":
                    return DayOfWeek.Tuesday;
                case "wed":
                    return DayOfWeek.Wednesday;
                case "thu":
                    return DayOfWeek.Thursday;
                case "fri":
                    return DayOfWeek.Friday;
                case "sat":
                    return DayOfWeek.Saturday;
                case "sun":
                    return DayOfWeek.Sunday;
            }

            return null;
        }

        private static int? ParseMonth(string? month)
        {
            if (month == null)
                return null;

            switch (month.ToLower())
            {
                case "jan":
                    return 1;
                case "feb":
                    return 2;
                case "mar":
                    return 3;
                case "apr":
                    return 4;
                case "may":
                    return 5;
                case "jun":
                    return 6;
                case "jul":
                    return 7;
                case "aug":
                    return 8;
                case "sep":
                    return 9;
                case "oct":
                    return 10;
                case "nov":
                    return 11;
                case "dec":
                    return 12;
            }

            return null;
        }

        private static int? IsValidDay(int? num)
        {
            if (num == null)
                return null;

            if (num.Value >= 1 && num.Value <= 31)
                return num;

            return null;
        }

        private static int? IsValidHour(int? num)
        {
            if (num == null)
                return null;

            if (num >= 0 && num <= 23)
                return num;

            return null;
        }

        private static int? IsValidMin(int? num)
        {
            if (num == null)
                return null;

            if (num >= 0 && num <= 59)
                return num;

            return null;
        }

        private static int? IsValidSec(int? num)
        {
            if (num == null)
                return null;

            if (num >= 0 && num <= 59)
                return num;

            return null;
        }

        private class StringReader
        {
            private readonly string _str;
            private int _pos;

            public StringReader(string str)
            {
                _str = str;
                _pos = 0;
            }

            public int NbRemainingChars => Math.Max(0, _str.Length - _pos);

            public string? Read(int nbChars)
            {
                if (_pos + nbChars > _str.Length)
                {
                    _pos = _str.Length;
                    return null;
                }

                var read = _str.Substring(_pos, nbChars);
                _pos += nbChars;
                return read;
            }

            public int? ReadDigits(int nbChars)
            {
                var digits = Read(nbChars);
                if (digits == null)
                    return null;

                if (digits.Any(digit => digit < '0' || digit > '9'))
                    return null;

                return int.Parse(digits);
            }
        }

    }
}