using ElMariachi.Http.Header.Exceptions;

namespace ElMariachi.Http.Header.Managed
{
    public class ContentLengthHeader : ManagedHeader, IContentLengthHeader
    {
        public override string Name => HttpConst.Headers.ContentLength;

        private string? _rawValue = null;
        private long? _value;

        public long? Value
        {
            get => _value;
            set
            {
                _value = value;
                _rawValue = value?.ToString();
            }
        }

        protected override string? GetRawValue()
        {
            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            if (rawValue == null)
            {
                _rawValue = null;
                _value = null;
            }
            else
            {
                if (!long.TryParse(rawValue, out var contentLength))
                    throw new HeaderFormatException(
                        $"{HttpConst.Headers.ContentLength} header value «{rawValue}» is not a valid positive number.");

                if (contentLength < 0)
                    throw new HeaderFormatException(
                        $"{HttpConst.Headers.ContentLength} header value «{rawValue}» is not allowed to be negative.");

                _rawValue = rawValue;
                _value = contentLength;
            }
        }

    }
}