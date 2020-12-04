using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header.Managed
{
    public class TransferEncodingHeader : ManagedHeader, ITransferEncodingHeader
    {

        public IList<string> Values => _valueCollection;
        private readonly ValueCollection _valueCollection;
        private string? _rawValue;

        public TransferEncodingHeader()
        {
            _valueCollection = new ValueCollection(this);
        }

        public override string Name => HttpConst.Headers.TransferEncoding;

        private bool IsDirty { get; set; }

        protected override string? GetRawValue()
        {
            if (!IsDirty)
                return _rawValue;

            _rawValue = string.Join(HttpConst.DefaultHeaderDelimiter, _valueCollection);

            IsDirty = false;

            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;
            _valueCollection.Clear();
            IsDirty = false; // NOTE: important to set after Clear(), because as Clear() sets IsDirty as true
            if (rawValue == null)
                return;

            foreach (var interpreted in HeaderValuesParser.ParseInterpretedStatic(rawValue, HttpConst.DefaultHeaderDelimiter))
                _valueCollection.Add(interpreted);

            IsDirty = false;
        }

        private class ValueCollection : Collection<string>
        {
            private readonly TransferEncodingHeader _owner;

            public ValueCollection(TransferEncodingHeader owner)
            {
                _owner = owner;
            }

            protected override void ClearItems()
            {
                base.ClearItems();
                _owner.IsDirty = true;
            }

            protected override void InsertItem(int index, string item)
            {
                var cleanedItem = CleanItem(item);
                base.InsertItem(index, cleanedItem);
                _owner.IsDirty = true;
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                _owner.IsDirty = true;
            }

            protected override void SetItem(int index, string item)
            {
                var cleanedItem = CleanItem(item);
                base.SetItem(index, cleanedItem);
                _owner.IsDirty = true;
            }

            private static string CleanItem(string item)
            {
                if (item == null)
                    throw new ArgumentNullException(item);
                return item;
            }

        }

    }
}