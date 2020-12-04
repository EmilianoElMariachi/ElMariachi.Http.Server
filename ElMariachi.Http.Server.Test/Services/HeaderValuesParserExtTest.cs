using System.Linq;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Utils;
using Xunit;

namespace ElMariachi.Http.Server.Test.Services
{
    public class HeaderValuesParserExtTest
    {
        private readonly HeaderValuesParser _headerValuesParser;

        public HeaderValuesParserExtTest()
        {
            _headerValuesParser = new HeaderValuesParser();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(" \t ")]
        public void ParseInterpreted_EmptyOrWhitespaceAreRemoved(string headerValue)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            Assert.Empty(values);
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\"abc\"", "abc")]
        [InlineData("\" \"", " ")]
        [InlineData("\" \t \"", " \t ")]
        [InlineData("\" a\tb \"", " a\tb ")]
        [InlineData("   \" a\tb \"   ", " a\tb ")]
        [InlineData("  \t  \" a\tb \" \t  ", " a\tb ")]
        public void ParseInterpreted_SingleDelimitedValue(string headerValue, string expectedValue)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            Assert.Single(values);
            Assert.Equal(expectedValue, values[0]);
        }

        [Fact]
        public void ParseInterpreted_SingleValue()
        {
            var values = _headerValuesParser.ParseInterpreted("abc").ToArray();
            Assert.Single(values);
            Assert.Equal("abc", values[0]);
        }

        [Theory]
        [InlineData("abc,def", new[] { "abc", "def" })]
        [InlineData("abc , def", new[] { "abc", "def" })]
        [InlineData(" abc,def ", new[] { "abc", "def" })]
        [InlineData("  abc  ,  def  ", new[] { "abc", "def" })]
        [InlineData("  abc  ,  def  , gh  ", new[] { "abc", "def", "gh" })]
        public void ParseInterpreted_NominalCases(string headerValue, string[] expectedValues)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            Assert.Equal(expectedValues.Length, values.Length);
            for (var index = 0; index < expectedValues.Length; index++)
            {
                var expectedValue = expectedValues[index];
                Assert.Equal(expectedValue, values[index]);
            }
        }

        [Theory]
        [InlineData("    ,\t,abc,,def, , ", new[] { "abc", "def" })]
        [InlineData("  \"  \"  ,\t,abc,,def, , ", new[] { "  ", "abc", "def" })]
        public void ParseInterpreted_WhitespaceValuesAreRemovedWhenNonDelimited(string headerValue, string[] expectedValues)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            Assert.Equal(expectedValues.Length, values.Length);
            for (var index = 0; index < expectedValues.Length; index++)
            {
                var expectedValue = expectedValues[index];
                Assert.Equal(expectedValue, values[index]);
            }
        }

        [Fact]
        public void ParseInterpreted_ValuesAreTrimmedWhenNonDelimited()
        {
            var values = _headerValuesParser.ParseInterpreted("     This Is a Value   , def,ghi   ,  another Value \t ").ToArray();
            Assert.Equal(4, values.Length);
            Assert.Equal("This Is a Value", values[0]);
            Assert.Equal("def", values[1]);
            Assert.Equal("ghi", values[2]);
            Assert.Equal("another Value", values[3]);
        }

        [Fact]
        public void ParseInterpreted_IgnoresDelimitedStrings()
        {
            var values = _headerValuesParser.ParseInterpreted("     This \"Is\" a Value   , \" def \", \t  \" df,,,  \" \t ").ToArray();
            Assert.Equal(3, values.Length);
            Assert.Equal("This Is a Value", values[0]);
            Assert.Equal(" def ", values[1]);
            Assert.Equal(" df,,,  ", values[2]);
        }

        [Theory]
        [InlineData("\" A \"", " A ")]
        [InlineData(" \" A \" ", " A ")]
        [InlineData(" \"  A b C  \" ", "  A b C  ")]
        [InlineData(" some text \"  A b C  \" some text ", "some text   A b C   some text")]
        [InlineData(" some text \"  A b C  \"       ", "some text   A b C  ")]
        [InlineData("      \t \" A b C \" \t some text ", " A b C  \t some text")]
        public void ParseInterpreted_WhitespacesAroundDelimitedStringsArePreserved(string headerValue, string expectedValue)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            Assert.Single(values);
            Assert.Equal(expectedValue, values[0]);
        }

        [Theory]
        [InlineData("\" Ok, you, are, ready \"", new[] { " Ok, you, are, ready " })]
        [InlineData("abc,d\",e,\"f, \"This, is some text\", hey", new[] { "abc", "d,e,f", "This, is some text", "hey" })]
        public void ParseInterpreted_IgnoresCommasInDelimitedStrings(string headerValue, string[] expectedValues)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();

            Assert.Equal(expectedValues.Length, values.Length);

            for (var index = 0; index < expectedValues.Length; index++)
            {
                var expectedValue = expectedValues[index];
                Assert.Equal(expectedValue, values[index]);
            }
        }

        [Theory]
        [InlineData("\"a\\\"b\"", new[] { "a\"b" })]
        [InlineData("\" a\\\"b \"", new[] { " a\"b " })]
        public void ParseInterpreted_StripsBackSlashOfEscapedDoubleQuotes(string headerValue, string[] expectedValues)
        {
            var values = _headerValuesParser.ParseInterpreted(headerValue).ToArray();

            Assert.Equal(expectedValues.Length, values.Length);

            for (var index = 0; index < expectedValues.Length; index++)
            {
                var expectedValue = expectedValues[index];
                Assert.Equal(expectedValue, values[index]);
            }
        }

        [Theory]
        [InlineData("\"")]
        [InlineData("\"\\\"")]
        [InlineData("a\"some\\\"text")]
        public void ParseInterpreted_ThrowsWhenDelimitedStringIsNotTerminated(string headerValue)
        {
            var headerFormatException = Assert.Throws<HeaderFormatException>(() =>
            {
                _headerValuesParser.Parse(headerValue).ToArray();
            });

            Assert.Equal($"Header value «{headerValue}» is missing a string end delimiter.", headerFormatException.Message);
        }

    }
}
