namespace ElMariachi.Http.Header.Managed
{
    public abstract class ManagedHeader : Header, IManagedHeader
    {

        public sealed override HeaderType Type => HeaderType.Managed;

        public void Unset()
        {
            this.RawValue = null;
        }

    }
}