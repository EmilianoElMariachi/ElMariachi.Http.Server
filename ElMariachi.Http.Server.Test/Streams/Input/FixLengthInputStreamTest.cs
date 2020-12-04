using System.IO;
using System.Text;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Server.Streams.Input;
using Xunit;

namespace ElMariachi.Http.Server.Test.Streams.Input
{
    public class FixLengthInputStreamTest
    {
        [Fact]
        public void ReturnsZero_WhenCountIsZero()
        {
            var networkStream = new NetworkStreamEmulator(new MemoryStream());
            var fixLengthInputStream = new FixLengthInputStream(networkStream, 5);

            const int count = 0;
            Assert.Equal(0, fixLengthInputStream.Read(new byte[50], 0, count));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void ReturnsZero_WhenTryingToReadMoreThanStreamLength(int maxBytesPerRead)
        {
            var bytes = Encoding.ASCII.GetBytes("This is the stream content.");
            var networkStream = new NetworkStreamEmulator(new MemoryStream(bytes))
            {
                MaxBytesPerRead = maxBytesPerRead
            };
            var fixLengthInputStream = new FixLengthInputStream(networkStream, bytes.Length);

            var buff = new byte[bytes.Length + 1];
            while (fixLengthInputStream.NbRemainingBytesToRead > 0)
                fixLengthInputStream.Read(buff);

            Assert.Equal(0, fixLengthInputStream.Read(buff));
            Assert.Equal(0, fixLengthInputStream.NbRemainingBytesToRead);

            Assert.Equal(0, fixLengthInputStream.Read(new byte[500],0,500));
            Assert.Equal(0, fixLengthInputStream.NbRemainingBytesToRead);

            Assert.Equal(0, fixLengthInputStream.Read(new byte[1000], 0, 1000));
            Assert.Equal(0, fixLengthInputStream.NbRemainingBytesToRead);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(1, 100)]
        [InlineData(3, 1)]
        [InlineData(3, 10)]
        [InlineData(3, 100)]
        [InlineData(5, 1)]
        [InlineData(5, 10)]
        [InlineData(5, 100)]
        public void ThrowsStreamEndException(int nbMissingBytes, int maxBytesPerRead)
        {
            var bytes = Encoding.ASCII.GetBytes("This is the stream content.");
            var networkStream = new NetworkStreamEmulator(new MemoryStream(bytes))
            {
                MaxBytesPerRead = maxBytesPerRead
            };
            var fixLengthInputStream = new FixLengthInputStream(networkStream, bytes.Length + nbMissingBytes);

            Assert.Throws<StreamEndException>(() =>
            {
                var buff = new byte[10];
                while (fixLengthInputStream.NbRemainingBytesToRead > 0)
                    fixLengthInputStream.Read(buff);
            });
        }
    }
}
