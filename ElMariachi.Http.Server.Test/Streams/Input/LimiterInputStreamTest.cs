using System;
using System.IO;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Server.Streams.Input;
using Xunit;

namespace ElMariachi.Http.Server.Test.Streams.Input
{
    public class LimiterInputStreamTest
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void ConstructorThrowsWhenLimitIsNegative(int limit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var memoryStream = new MemoryStream();
                new LimiterInputStream(memoryStream, limit);
            });
        }

        [Theory]
        [InlineData(50, 49, 50)]
        [InlineData(12, 5, 6)]
        [InlineData(12, 0, 1)]
        public void ReadThrowsWhenTheNumberOfBytesReadIsGreaterThanMax(int nbBytesAvailable, int limit, int readBufferLength)
        {
            Assert.True(nbBytesAvailable >= readBufferLength, "Invalid test case, the number of bytes available should be greater than or equal to the number of bytes to read.");
            Assert.True(limit < readBufferLength, "Invalid test case, limit should be less than the number of bytes to read.");

            var ex = Assert.Throws<StreamLimitException>(() =>
            {
                var memoryStream = new MemoryStream(new byte[nbBytesAvailable]);
                var limiterInputStream = new LimiterInputStream(memoryStream, limit);

                var buffer = new byte[readBufferLength];
                limiterInputStream.Read(buffer);
            });

            Assert.Equal($"Maximum number of readable byte(s) reached (limit={limit}).", ex.Message);
        }

        [Theory]
        [InlineData(100, 50, 50)]
        [InlineData(12, 6, 1)]
        [InlineData(12, 6, 6)]
        [InlineData(12, 15, 0)]
        public void ReadingZeroBytesReturnsZeroWithoutThrowingException(int nbBytesAvailable, int limit, int readBufferLength)
        {
            var memoryStream = new MemoryStream(new byte[nbBytesAvailable]);
            var limiterInputStream = new LimiterInputStream(memoryStream, limit);

            var buffer = new byte[readBufferLength];
            const int zeroBytesToRead = 0;
            Assert.Equal(zeroBytesToRead, limiterInputStream.Read(buffer, zeroBytesToRead, zeroBytesToRead));
        }

        [Theory]
        [InlineData(100, 50, 50)]
        [InlineData(12, 6, 1)]
        [InlineData(12, 6, 6)]
        [InlineData(12, 15, 0)]
        public void DoesNotThrow_WhenTheNumberOfBytesReadIsLessThanTheSpecifiedLimitAndThereAreEnoughBytesAvailable(int nbBytesAvailable, int limit, int readBufferLength)
        {
            Assert.True(nbBytesAvailable >= readBufferLength, "Invalid test case, not enough bytes available for reading.");
            Assert.True(limit >= readBufferLength, "Invalid test case, limit should be greater than or equal to the number of bytes to read.");

            var memoryStream = new MemoryStream(new byte[nbBytesAvailable]);
            var limiterInputStream = new LimiterInputStream(memoryStream, limit);

            var buffer = new byte[readBufferLength];
            limiterInputStream.Read(buffer);
        }

        [Theory]
        [InlineData(100, 101, 101)]
        [InlineData(10, 20, 11)]
        [InlineData(0, 20, 1)]
        public void ThrowsStreamEndException_WhenEndOfStreamIsReachedBeforeHavingTheRequestedNumberOfBytes(int nbBytesAvailable, int limit, int readBufferLength)
        {
            Assert.True(nbBytesAvailable < readBufferLength, "Invalid test case, there should be less bytes available than the number of bytes to read.");
            Assert.True(limit >= readBufferLength, "Invalid test case, limit should be greater than or equal to the number of bytes to read.");

            var memoryStream = new MemoryStream(new byte[nbBytesAvailable]);
            var limiterInputStream = new LimiterInputStream(memoryStream, limit);

            var buffer = new byte[readBufferLength];
            Assert.Throws<StreamEndException>(() =>
            {
                limiterInputStream.Read(buffer);
                limiterInputStream.Read(buffer);
            });
        }

        [Fact]
        public void NbRemainingBytes_IsUpdatedOnRead()
        {
            var memoryStream = new MemoryStream(new byte[200]);
            var limiterInputStream = new LimiterInputStream(memoryStream, 100);

            Assert.Equal(100, limiterInputStream.NbRemainingBytes);

            limiterInputStream.Read(new byte[50]);

            Assert.Equal(50, limiterInputStream.NbRemainingBytes);

            limiterInputStream.Read(new byte[49]);

            Assert.Equal(1, limiterInputStream.NbRemainingBytes);

            limiterInputStream.Read(new byte[1]);

            Assert.Equal(0, limiterInputStream.NbRemainingBytes);
        }
    }
}
