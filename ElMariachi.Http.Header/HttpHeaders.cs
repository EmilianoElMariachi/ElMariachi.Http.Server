using System;
using System.Collections;
using System.Collections.Generic;
using ElMariachi.Http.Header.Unmanaged;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header
{
    public abstract class HttpHeaders : IHttpHeaders
    {

        private readonly Dictionary<string, Header> _dictionary = new Dictionary<string, Header>();

        private readonly object _lock = new object();

        public event HeadersChangedHandler? HeadersChanged;

        public string? this[string headerName]
        {
            get
            {
                if (headerName == null)
                    throw new ArgumentNullException(nameof(headerName));
                var headerNameLower = headerName.ToLower().Trim();

                lock (_lock)
                {
                    _dictionary.TryGetValue(headerNameLower, out var header);
                    return header?.RawValue;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(headerName))
                    throw new ArgumentNullException(nameof(headerName), "Header name can neither be null nor white space.");

                headerName = headerName.Trim();

                var headerNameLower = headerName.ToLower();

                lock (_lock)
                {
                    if (value == null)
                    {
                        _dictionary.TryGetValue(headerNameLower, out var header);
                        if (header == null)
                            return;

                        if (header.Type == HeaderType.Unmanaged)
                            _dictionary.Remove(headerNameLower);
                        else
                            header.RawValue = null;

                        NotifyHeaderChanged(headerNameLower, HeaderChangeType.Removed);
                    }
                    else
                    {
                        if (_dictionary.TryGetValue(headerNameLower, out var header))
                        {
                            if (header.Type == HeaderType.Unmanaged)
                                ((UnmanagedHeader)header).ChangeName(headerName);

                            header.RawValue = value;
                            NotifyHeaderChanged(headerNameLower, HeaderChangeType.Replaced);
                        }
                        else
                        {
                            _dictionary.Add(headerNameLower, new UnmanagedHeader(headerName)
                            {
                                RawValue = value
                            });
                            NotifyHeaderChanged(headerNameLower, HeaderChangeType.Added);
                        }
                    }
                }
            }
        }

        protected void AddHeader(Header header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header));

            var headerName = header.Name;

            if (string.IsNullOrWhiteSpace(headerName))
                throw new ArgumentNullException($"{nameof(Header)}.{nameof(Header.Name)}", "Header name can neither be null nor white space.");

            var headerNameLower = headerName.Trim().ToLower();
            lock (_lock)
            {
                _dictionary.Add(headerNameLower, header);
            }
        }

        public bool ContainsHeader(string headerName)
        {
            if (headerName == null)
                throw new ArgumentNullException(nameof(headerName));

            var headerLower = headerName.ToLower();
            lock (_lock)
            {
                return _dictionary.ContainsKey(headerLower);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IHeader> GetEnumerator()
        {
            lock (_lock)
            {
                return new HeadersEnumerator(_dictionary.GetEnumerator());
            }
        }

        internal class HeadersEnumerator : IEnumerator<IHeader>
        {
            private Dictionary<string, Header>.Enumerator _enumerator;

            public HeadersEnumerator(Dictionary<string, Header>.Enumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    if (_enumerator.Current.Value.RawValue != null)
                        return true;
                }
                return false;
            }

            public void Reset()
            {
                ((IEnumerator)_enumerator).Reset();
            }

            public IHeader Current => _enumerator.Current.Value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }

        public override string ToString()
        {
            return HttpHeadersSerializer.SerializeHeaders(this);
        }

        protected virtual void NotifyHeaderChanged(string headerName, HeaderChangeType changeType)
        {
            HeadersChanged?.Invoke(this, new HeaderChangedHandlerArgs(headerName, changeType));
        }
    }
}