using System.Collections.Generic;

namespace ElMariachi.Http.Header.Managed
{
    public class ConnectionHeader : FlagBasedManagedHeader, IConnectionHeader
    {
        private readonly CloseManagedValue _closeManagedValue = new CloseManagedValue();
        private readonly KeepAliveManagedValue _keepAliveManagedValue = new KeepAliveManagedValue();

        public override string Name => HttpConst.Headers.Connection;

        public bool KeepAlive
        {
            get => _keepAliveManagedValue.IsFlagSet;
            set => _keepAliveManagedValue.IsFlagSet = value;
        }

        public bool Close
        {
            get => _closeManagedValue.IsFlagSet;
            set => _closeManagedValue.IsFlagSet = value;
        }

        protected override IEnumerable<ManagedValue> GetManagedValues()
        {
            yield return _closeManagedValue;
            yield return _keepAliveManagedValue;
        }

        private class CloseManagedValue : FlagManagedValue
        {
            protected override string FlagKeyword => "close";
        }

        private class KeepAliveManagedValue : FlagManagedValue
        {
            protected override string FlagKeyword => "keep-alive";
        }

    }
}