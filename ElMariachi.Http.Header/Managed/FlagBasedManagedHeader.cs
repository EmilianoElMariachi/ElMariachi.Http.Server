using System;
using System.Collections.Generic;
using System.Linq;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header.Managed
{
    /// <summary>
    /// Base class for managed headers where the header value consists in a list of values where order doesn't matter (similar to flag enum).
    /// </summary>
    public abstract class FlagBasedManagedHeader : ManagedHeader
    {

        private readonly List<Value> _values = new List<Value>();
        private string? _rawValue = null;


        public FlagBasedManagedHeader()
        {
            this.RawValue = null;
        }

        protected virtual char ValuesDelimiter => HttpConst.DefaultHeaderDelimiter;

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;
            _values.Clear();
            var managedValues = GetManagedValues().ToList();

            var position = 0;

            var managedValuesInfo = new List<ManagedValueInfo>();

            foreach (var managedValue in managedValues)
            {
                managedValuesInfo.Add(new ManagedValueInfo { Value = managedValue, Parsed = false });
                managedValue.Reset();
                managedValue.ActualPosition = position++;
            }

            _values.AddRange(managedValues);

            if (rawValue == null)
                return;

            foreach (var parsedValue in HeaderValuesParser.ParseStatic(rawValue, ValuesDelimiter))
            {
                Value? value = null;

                foreach (var managedValueInfo in managedValuesInfo)
                {
                    if (!managedValueInfo.Parsed && managedValueInfo.Value.Parse(parsedValue.Interpreted, parsedValue.Raw))
                    {
                        managedValueInfo.Parsed = true;
                        value = managedValueInfo.Value;
                        break;
                    }
                }

                if (value == null)
                {
                    value = new UnmanagedValue(parsedValue.Raw);
                    _values.Add(value);
                }
                value.ActualPosition = position++;
            }
        }

        protected override string? GetRawValue()
        {
            var isDirty = _values.FirstOrDefault(value => value is ManagedValue managedValue && managedValue.IsDirty) != null;

            if (!isDirty)
                return _rawValue;

            var values = _values.Where(value => value.RawValue != null).ToList();

            if (values.Count <= 0)
                return null;
            values.Sort((value1, value2) => value1.ActualPosition - value2.ActualPosition);
            _rawValue = string.Join(ValuesDelimiter, values.Select(value => value.RawValue));

            return _rawValue;
        }

        protected abstract IEnumerable<ManagedValue> GetManagedValues();

        private class ManagedValueInfo
        {
            public ManagedValue Value;

            public bool Parsed;
        }

        /// <summary>
        /// Represents one value for multi valued headers.
        /// </summary>
        public abstract class Value
        {
            public int ActualPosition { get; set; }

            public abstract string? RawValue { get; }
        }

        public abstract class ManagedValue : Value
        {
            public bool IsDirty { get; protected set; }

            public abstract bool Parse(string? interpretedValue, string rawValue);

            public abstract void Reset();
        }

        internal abstract class FlagManagedValue : ManagedValue
        {

            private string _rawValue;
            private bool _isFlagSet;

            internal FlagManagedValue()
            {
                _rawValue = FlagKeyword;
            }

            protected abstract string FlagKeyword { get; }

            public sealed override string? RawValue => IsFlagSet ? _rawValue : null;

            public bool IsFlagSet
            {
                get => _isFlagSet;
                set
                {
                    _isFlagSet = value;
                    IsDirty = true;
                }
            }

            public sealed override bool Parse(string? interpretedValue, string rawValue)
            {
                if (!string.Equals(interpretedValue, FlagKeyword, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                _rawValue = rawValue;
                IsFlagSet = true;
                return true;
            }

            public sealed override void Reset()
            {
                _rawValue = FlagKeyword;
                IsFlagSet = false;
            }
        }

        private sealed class UnmanagedValue : Value
        {

            public UnmanagedValue(string rawValue)
            {
                RawValue = rawValue ?? throw new ArgumentNullException(nameof(rawValue));
            }

            public override string RawValue { get; }

        }

    }
}