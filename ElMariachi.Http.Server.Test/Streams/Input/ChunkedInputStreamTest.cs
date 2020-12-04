using System.IO;
using System.Text;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Server.Streams.Input;
using Xunit;

namespace ElMariachi.Http.Server.Test.Streams.Input
{
    public class ChunkedInputStreamTest
    {
        private readonly MemoryStream _memoryStream;
        private readonly ChunkedInputStream _chunkedInputStream;
        private readonly NetworkStreamEmulator _networkStreamEmulator;

        public ChunkedInputStreamTest()
        {
            _memoryStream = new MemoryStream();
            _networkStreamEmulator = new NetworkStreamEmulator(_memoryStream);
            _chunkedInputStream = new ChunkedInputStream(_networkStreamEmulator);
        }

        [Theory]
        [InlineData("1a\r\n" +
                    "abcdefghijklmnopqrstuvwxyz \r\n" +
                    "0\r\n" +
                    "\r")]
        public void Throws_WhenChunkIsNotFollowedByLineReturn(string chunks)
        {
            SetupData(chunks);

            var ex = Assert.Throws<RequestFormatException>(() =>
            {
                new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd();
            });

            Assert.Equal("Chunk of length «26» is not followed by expected sequence «\\r\\n».", ex.Message);
        }

        [Theory]
        [InlineData("7FFFFFFFF\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0\r\n" +
                    "\r")]
        public void Throws_WhenHexValuesAreLongerThen8CharsLimit(string chunks)
        {
            SetupData(chunks);

            var ex = Assert.Throws<RequestFormatException>(() =>
            {
                new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd();
            });

            Assert.Equal("Chunk length can't be longer than «8» hexadecimal chars.", ex.Message);
        }

        [Theory]
        [InlineData("1G\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0\r\n" +
                    "\r")]
        public void Throws_WhenHexValuesAreNotValid(string chunks)
        {
            SetupData(chunks);

            var ex = Assert.Throws<RequestFormatException>(() =>
            {
                new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd();
            });

            Assert.Equal("Chunk length «1G» is not a valid hexadecimal number.", ex.Message);
        }

        [Theory]
        [InlineData("1A\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0\r\n" +
                    "\r")]

        [InlineData("1A\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "10\r\n")]

        [InlineData("1A\r\n" +
                    "abcdefg")]
        public void Throws_WhenStreamEndIsReachedBeforeTheFinalChunk(string chunks)
        {
            SetupData(chunks);

            Assert.Throws<StreamEndException>(() =>
            {
                new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd();
            });
        }

        [Theory]
        [InlineData("1A\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0\r\n" +
                    "\r\n", "abcdefghijklmnopqrstuvwxyz")]

        [InlineData("1A\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "5\r\n" +
                    "12345\r\n" +
                    "0\r\n" +
                    "\r\n", "abcdefghijklmnopqrstuvwxyz12345")]

        [InlineData("A\r\n" +
                    "some\r\ntext\r\n" +
                    "5\r\n" +
                    "12345\r\n" +
                    "0\r\n" +
                    "\r\n", "some\r\ntext12345")]
        public void Chunks_AreDecodedAsExpected(string chunks, string expectedContent)
        {
            SetupData(chunks);

            Assert.Equal(expectedContent, new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd());
        }

        [Theory]
        [InlineData("1a\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0\r\n" +
                    "\r\n", "abcdefghijklmnopqrstuvwxyz")]

        [InlineData("1A\r\n" +
                    "abcdefghijklmnopqrstuvwxyz\r\n" +
                    "0F\r\n" +
                    "123456123456789\r\n" +
                    "0f\r\n" +
                    "123456123456789\r\n" +
                    "0\r\n" +
                    "\r\n", "abcdefghijklmnopqrstuvwxyz123456123456789123456123456789")]
        public void Chunks_HexValuesAreNotCaseSensitive(string chunks, string expectedContent)
        {
            SetupData(chunks);

            Assert.Equal(expectedContent, new StreamReader(_chunkedInputStream, Encoding.ASCII).ReadToEnd());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        public void Chunks_AreReadAsExpected(int nbBytesPerRead)
        {
            _networkStreamEmulator.MaxBytesPerRead = nbBytesPerRead;

            SetupData(
                "1A\r\n" +
                "abcdefghijklmnopqrstuvwxyz\r\n" +
                "0\r\n" +
                "\r\n"
                );

            var buffer = new byte[6];
            Assert.Equal(6, _chunkedInputStream.Read(buffer));
            Assert.Equal("abcdef", Encoding.ASCII.GetString(buffer));

            buffer = new byte[10];
            Assert.Equal(10, _chunkedInputStream.Read(buffer));
            Assert.Equal("ghijklmnop", Encoding.ASCII.GetString(buffer));

            buffer = new byte[10];
            Assert.Equal(10, _chunkedInputStream.Read(buffer));
            Assert.Equal("qrstuvwxyz", Encoding.ASCII.GetString(buffer, 0, 10));
        }

        [Fact]
        public void EmptyChunks_IsSupported()
        {
            SetupData(
                "0\r\n" +
                "\r\n"
            );

            var buffer = new byte[6];
            Assert.Equal(0, _chunkedInputStream.Read(buffer));
        }

        [Fact]
        public void WhenAllChunksAreRead_ReadReturnsZero()
        {
            SetupData(
                "1A\r\n" +
                "abcdefghijklmnopqrstuvwxyz\r\n" +
                "0\r\n" +
                "\r\n"
            );

            var buffer = new byte[24];
            Assert.Equal(24, _chunkedInputStream.Read(buffer));
            Assert.Equal("abcdefghijklmnopqrstuvwx", Encoding.ASCII.GetString(buffer));

            buffer = new byte[10];
            Assert.Equal(2, _chunkedInputStream.Read(buffer));
            Assert.Equal("yz", Encoding.ASCII.GetString(buffer, 0, 2));

            buffer = new byte[10];
            Assert.Equal(0, _chunkedInputStream.Read(buffer));

            buffer = new byte[10];
            Assert.Equal(0, _chunkedInputStream.Read(buffer));

            buffer = new byte[10];
            Assert.Equal(0, _chunkedInputStream.Read(buffer));
        }

        [Fact]
        public void ReturnsZero_WhenCountIsZero()
        {
            SetupData(
                "1A\r\n" +
                "abcdefghijklmnopqrstuvwxyz\r\n" +
                "0\r\n" +
                "\r\n"
            );

            var buffer = new byte[50];

            const int countIsZero = 0;
            Assert.Equal(0, _chunkedInputStream.Read(buffer, 0, countIsZero));

            Assert.Equal(1, _chunkedInputStream.Read(buffer, 0, 1));
            Assert.Equal('a', (char)buffer[0]);

            Assert.Equal(0, _chunkedInputStream.Read(buffer, 0, countIsZero));

            Assert.Equal(1, _chunkedInputStream.Read(buffer, 0, 1));
            Assert.Equal('b', (char)buffer[0]);

            Assert.Equal(0, _chunkedInputStream.Read(buffer, 0, countIsZero));
        }

        private void SetupData(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            _memoryStream.Write(bytes);
            _memoryStream.Position = 0;
        }
    }
}
