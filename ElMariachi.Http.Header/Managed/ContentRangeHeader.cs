using System.Text;

namespace ElMariachi.Http.Header.Managed
{
    public class ContentRangeHeader : ManagedHeader, IContentRangeHeader
    {
        private IRange? _range;
        private string? _unit;
        private string? _rawValue;
        private long? _size;

        public override string Name => HttpConst.Headers.ContentRange;

        public long? Size
        {
            get => _size;
            set
            {
                _size = value;
                IsDirty = true;
            }
        }


        public string? Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                IsDirty = true;
            }
        }

        public IRange? Range
        {
            get => _range;
            set
            {
                if (_range != null)
                    _range.Changed -= OnRangeChanged;

                if (value != null)
                    value.Changed += OnRangeChanged;

                _range = value;
                IsDirty = true;
            }
        }

        private bool IsDirty { get; set; }

        protected override string? GetRawValue()
        {
            if (!IsDirty)
                return _rawValue;

            var sb = new StringBuilder();

            sb.Append(Unit);
            sb.Append(" ");
            var range = Range;
            if (range == null)
                sb.Append("*");
            else
            {
                sb.Append(range.Start);
                sb.Append("-");
                sb.Append(range.End);
            }
            sb.Append("/");

            var size = Size;
            sb.Append(size == null ? "*" : size.ToString());

            _rawValue = sb.ToString();

            IsDirty = false;

            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            IsDirty = false;
            _rawValue = rawValue;
            _range = null;
            _size = null;
            _unit = null;

            if (rawValue == null)
                return;

            var partsTmp = rawValue.Trim().Split(' ', 2);
            if (partsTmp.Length != 2)
                return;

            var unit = partsTmp[0];
            var rangeRaw = partsTmp[1];

            partsTmp = rangeRaw.Split('/', 2);
            if (partsTmp.Length != 2)
                return;

            var rangesRaw = partsTmp[0].Trim();
            IRange? range;
            if (rangesRaw == "*")
                range = null;
            else
            {
                var rangeParts = rangesRaw.Split('-', 2);
                if (rangeParts.Length != 2)
                    return;
                if (!long.TryParse(rangeParts[0], out var start))
                    return;

                if (!long.TryParse(rangeParts[1], out var end))
                    return;
                range = new Range(start, end);
            }

            var sizeRaw = partsTmp[1].Trim();

            long? size;
            if (sizeRaw == "*")
                size = null;
            else
            {
                if (!long.TryParse(sizeRaw, out var nonNullableSize))
                    return;
                size = nonNullableSize;
            }

            Range = range;
            Size = size;
            Unit = unit;
            IsDirty = false;
        }

        private void OnRangeChanged(object sender, RangeChangedHandlerArgs args)
        {
            IsDirty = true;
        }

    }
}