using System;

namespace ElMariachi.Http.Header.Managed
{
    public struct Range : IRange
    {
        private long? _end;
        private long? _start;
        public event RangeChangedHandler? Changed;

        public Range(long? start, long? end)
        {
            _start = start;
            _end = end;
            Changed = null;
        }

        public long? Start
        {
            get => _start;
            set
            {
                _start = value;
                NotifyRangeChanged(nameof(Start));
            }
        }

        public long? End
        {
            get => _end;
            set
            {
                _end = value;
                NotifyRangeChanged(nameof(End));
            }
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }

        public bool Equals(IRange? other)
        {
            return other != null && (Start == other.Start && End == other.End);
        }

        public override bool Equals(object? obj)
        {
            return obj is IRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        private void NotifyRangeChanged(string propertyName)
        {
            Changed?.Invoke(this, new RangeChangedHandlerArgs(propertyName));
        }
    }


}