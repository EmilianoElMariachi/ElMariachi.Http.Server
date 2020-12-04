using System;

namespace ElMariachi.Http.Header.Managed
{
    public static class IRangeExt
    {
        /// <summary>
        /// Returns true if range is valid (according to RFC2616 https://tools.ietf.org/html/rfc2616#section-14.35)
        /// </summary>
        /// <returns></returns>
        public static bool IsValid(this IRange range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            if (range.Start == null && range.End == null)
                return false;

            if (range.Start < 0)
                return false;

            if (range.End < 0)
                return false;

            return range.Start == null || range.End == null || !(range.Start > range.End);
        }

    }
}
