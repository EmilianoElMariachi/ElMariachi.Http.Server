using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{

    public class TransferEncodingHeaderTest
    {
        private readonly TransferEncodingHeader _transferEncodingHeader;

        public TransferEncodingHeaderTest()
        {
            _transferEncodingHeader = new TransferEncodingHeader();
        }

        [Fact]
        public void DefaultValue()
        {
            var transferEncodingHeader = new TransferEncodingHeader();

            Assert.Equal("Transfer-Encoding", transferEncodingHeader.Name);

            Assert.Null(transferEncodingHeader.RawValue);

            Assert.Empty(transferEncodingHeader.Values);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a,b,c")]
        [InlineData("a , b , c")]
        [InlineData("chunked , deflate, gzip, identity")]
        [InlineData("aa, chunked , compress, deflate, gzip, identity, bb")]
        public void TheGetRawValueIsAlwaysEqualToTheSetRawValue(string rawValue)
        {
            _transferEncodingHeader.RawValue = rawValue;
            Assert.Equal(rawValue, _transferEncodingHeader.RawValue);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a,b,c")]
        [InlineData("a , b , c")]
        [InlineData("chunked , deflate, gzip, identity")]
        [InlineData("aa, chunked , compress, deflate, gzip, identity, bb")]
        public void ClearingTheValuesUpdatesRawValue(string rawValue)
        {
            _transferEncodingHeader.RawValue = rawValue;

            _transferEncodingHeader.Values.Clear();

            Assert.Equal("", _transferEncodingHeader.RawValue);
        }

        [Theory]
        [InlineData("A , B ,  C", "D", "A,B,C,D")]
        [InlineData("", "A", "A")]
        [InlineData("", " A ", " A ")]
        public void AddingOneValue_UpdatesRawValue(string rawValue, string addedValue, string expectedRawValue)
        {
            _transferEncodingHeader.RawValue = rawValue;
            _transferEncodingHeader.Values.Add(addedValue);

            Assert.Equal(expectedRawValue, _transferEncodingHeader.RawValue);
        }


        [Theory]
        [InlineData("A , B ,  C", "D", "D,B,C")]
        [InlineData("A , B ,  C", " D ", " D ,B,C")]
        public void ReplacingOneValue_UpdatesRawValue(string rawValue, string replacedValue, string expectedRawValue)
        {
            _transferEncodingHeader.RawValue = rawValue;
            _transferEncodingHeader.Values[0] = replacedValue;

            Assert.Equal(expectedRawValue, _transferEncodingHeader.RawValue);
        }
    }
}
