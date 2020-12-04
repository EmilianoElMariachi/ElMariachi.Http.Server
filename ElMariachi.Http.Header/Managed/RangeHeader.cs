using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ElMariachi.Http.Header.Managed
{
    public class RangeHeader : ManagedHeader, IRangeHeader
    {
        private string? _rawValue;
        private readonly RangeCollection _ranges;
        private string? _unit;

        public RangeHeader()
        {
            _ranges = new RangeCollection(this);
        }

        public override string Name => HttpConst.Headers.Range;

        public string? Unit
        {
            get => _unit;
            set
            {
                IsDirty = true;
                _unit = value;
            }
        }

        public IList<IRange> Ranges => _ranges;

        private bool IsDirty { get; set; }

        protected override string? GetRawValue()
        {
            if (!IsDirty)
                return _rawValue;

            var sb = new StringBuilder($"{this.Unit}=");

            for (var index = 0; index < Ranges.Count; index++)
            {
                var range = Ranges[index];

                sb.Append(range.Start.ToString());
                sb.Append("-");
                sb.Append(range.End.ToString());

                if (index < Ranges.Count - 1)
                    sb.Append(",");
            }

            _rawValue = sb.ToString();
            IsDirty = false;
            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;

            _ranges.Clear();
            IsDirty = false; // NOTE: important to set after Clear(), because as Clear() sets IsDirty as true
            if (rawValue == null)
                return;

            var ranges = new List<Range>();

            var equalSplits = rawValue.Split('=');
            if (equalSplits.Length != 2)
                return;

            var unit = equalSplits[0].Trim();
            if (string.IsNullOrWhiteSpace(unit))
                return;

            var rangesRaw = equalSplits[1];

            foreach (var rangeRaw in rangesRaw.Split(HttpConst.DefaultHeaderDelimiter))
            {

                var values = rangeRaw.Split('-');
                if (values.Length != 2)
                    return;

                long? start;
                {
                    var startRaw = values[0];
                    if (string.IsNullOrWhiteSpace(startRaw))
                        start = null;
                    else if (!long.TryParse(startRaw, out var startTmp))
                        return;
                    else
                        start = startTmp;
                }

                long? end;
                {
                    var endRaw = values[1];
                    if (string.IsNullOrWhiteSpace(endRaw))
                        end = null;
                    else if (!long.TryParse(endRaw, out var endTmp))
                        return;
                    else
                        end = endTmp;
                }

                if (start == null && end == null)
                    return;

                ranges.Add(new Range
                {
                    Start = start,
                    End = end
                });
            }

            Unit = unit;

            foreach (var range in ranges)
                Ranges.Add(range);

            IsDirty = false;
        }

        private class RangeCollection : Collection<IRange>
        {
            private readonly RangeHeader _owner;

            public RangeCollection(RangeHeader owner)
            {
                _owner = owner;
            }

            private void OnRangeChanged(object sender, RangeChangedHandlerArgs args)
            {
                _owner.IsDirty = true;
            }

            protected override void ClearItems()
            {
                foreach (var item in this)
                    item.Changed -= OnRangeChanged;

                base.ClearItems();
                _owner.IsDirty = true;
            }

            protected override void RemoveItem(int index)
            {
                try
                {
                    this[index].Changed -= OnRangeChanged;
                }
                catch
                {
                    // ignored
                }
                base.RemoveItem(index);
                _owner.IsDirty = true;
            }

            protected override void InsertItem(int index, IRange item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                base.InsertItem(index, item);
                _owner.IsDirty = true;
                item.Changed += OnRangeChanged;
            }

            protected override void SetItem(int index, IRange item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                try
                {
                    this[index].Changed -= OnRangeChanged;
                }
                catch
                {
                    // ignored
                }

                base.SetItem(index, item);
                _owner.IsDirty = true;
                item.Changed += OnRangeChanged;
            }
        }

    }

}
