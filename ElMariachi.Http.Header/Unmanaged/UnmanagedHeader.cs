using System;

namespace ElMariachi.Http.Header.Unmanaged
{
    public class UnmanagedHeader : Header
    {
        private string _name;
        private string? _rawValue;

        public UnmanagedHeader(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public sealed override HeaderType Type => HeaderType.Unmanaged;

        public override string Name => _name;

        protected override string? GetRawValue()
        {
            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;
        }

        public void ChangeName(string newName)
        {
            _name = newName ?? throw new ArgumentNullException(nameof(newName));
        }

    }
}