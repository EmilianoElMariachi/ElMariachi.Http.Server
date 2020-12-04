namespace ElMariachi.Http.Server.Streams.Input
{
    public class NullInputStream : ReadOnlyStream
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }
    }
}