using System.Linq;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Utils;
using Xunit;

namespace ElMariachi.Http.Server.Test.Services
{
    public class HeaderValuesParserTest
    {
        private readonly HeaderValuesParser _headerValuesParser;

        public HeaderValuesParserTest()
        {
            _headerValuesParser = new HeaderValuesParser();
        }


        #region Parse

        [Fact]
        public void Parse_EmptyStringReturnsOneEmptyRawValue()
        {
            var values = _headerValuesParser.Parse("").ToArray();

            Assert.Single(values);
            Assert.Equal(null, values[0].Interpreted);
            Assert.Equal("", values[0].Raw);
        }

        [Theory]
        [InlineData("a", new[] { "a" }, new[] { "a" })]
        [InlineData("a,b,c", new[] { "a", "b", "c" }, new[] { "a", "b", "c" })]
        [InlineData(" a ,b,\tc\t", new[] { " a ", "b", "\tc\t" }, new[] { "a", "b", "c" })]
        public void Parse_WithWhiteSpaces(string headerValue, string[] expectedRawValues, string[] expectedInterpretedValues)
        {
            var values = _headerValuesParser.Parse(headerValue).ToArray();

            Assert.Equal(expectedRawValues.Length, values.Length);

            for (var index = 0; index < expectedRawValues.Length; index++)
            {
                Assert.Equal(expectedRawValues[index], values[index].Raw);
                Assert.Equal(expectedInterpretedValues[index], values[index].Interpreted);
            }
        }

        [Theory]
        [InlineData("a\",\"", new[] { "a\",\"" }, new[] { "a," })]
        [InlineData("\"a,b,c\"", new[] { "\"a,b,c\"" }, new[] { "a,b,c" })]
        [InlineData("\"a,b,c\", \",some , text,\" ", new[] { "\"a,b,c\"", " \",some , text,\" " }, new[] { "a,b,c", ",some , text," })]
        public void Parse_IgnoresCommasInDelimitedStrings(string headerValue, string[] expectedRawValues, string[] expectedInterpretedValues)
        {
            var values = _headerValuesParser.Parse(headerValue).ToArray();

            Assert.Equal(expectedRawValues.Length, values.Length);

            for (var index = 0; index < expectedRawValues.Length; index++)
            {
                Assert.Equal(expectedRawValues[index], values[index].Raw);
                Assert.Equal(expectedInterpretedValues[index], values[index].Interpreted);
            }
        }

        [Theory]
        [InlineData("\"a,\\\",c\"", new[] { "\"a,\\\",c\"" }, new[] { "a,\",c" })]
        [InlineData("\"\\\",\\\",\\\"\", \",some \\\" text,\" ", new[] { "\"\\\",\\\",\\\"\"", " \",some \\\" text,\" " }, new[] { "\",\",\"", ",some \" text," })]
        public void Parse_EscapesEscapedDoubleQuotes(string headerValue, string[] expectedRawValues, string[] expectedInterpretedValues)
        {
            var values = _headerValuesParser.Parse(headerValue).ToArray();

            Assert.Equal(expectedRawValues.Length, values.Length);

            for (var index = 0; index < expectedRawValues.Length; index++)
            {
                Assert.Equal(expectedRawValues[index], values[index].Raw);
                Assert.Equal(expectedInterpretedValues[index], values[index].Interpreted);
            }
        }

        [Theory]
        [InlineData("a\\", new[] { "a\\" }, new[] { "a\\" })]
        [InlineData("a\\b", new[] { "a\\b" }, new[] { "a\\b" })]
        [InlineData("\"a\\b\"", new[] { "\"a\\b\"" }, new[] { "a\\b" })]
        [InlineData("\"a\\\\b\"", new[] { "\"a\\\\b\"" }, new[] { "a\\\\b" })]
        [InlineData("\"a\\\\b\\\\\"\"", new[] { "\"a\\\\b\\\\\"\"" }, new[] { "a\\\\b\\\"" })] // literal header => "a\\b\\""
        [InlineData("\"a\\\\b\\\\\"\"\\\\", new[] { "\"a\\\\b\\\\\"\"\\\\" }, new[] { "a\\\\b\\\"\\\\" })] // literal header => "a\\b\\""\\
        [InlineData("\"a\\\\b\\\\\"\"\\\\\\b", new[] { "\"a\\\\b\\\\\"\"\\\\\\b" }, new[] { "a\\\\b\\\"\\\\\\b" })] // literal header => "a\\b\\""\\\b
        public void Parse_PreservesNonEscapingBackslashes(string headerValue, string[] expectedRawValues, string[] expectedInterpretedValues)
        {
            var values = _headerValuesParser.Parse(headerValue).ToArray();

            Assert.Equal(expectedRawValues.Length, values.Length);

            for (var index = 0; index < expectedRawValues.Length; index++)
            {
                Assert.Equal(expectedRawValues[index], values[index].Raw);
                Assert.Equal(expectedInterpretedValues[index], values[index].Interpreted);
            }
        }

        [Theory]
        [InlineData("a,", new[] { "a", "" })]
        [InlineData("a,b ,  ", new[] { "a", "b ", "  " })]
        [InlineData("a,b , \t ", new[] { "a", "b ", " \t " })]
        public void Parse_ReturnsLastDelimitedWhitespaceStringAfterLastComma(string headerValue, string[] expectedRawValues)
        {
            var values = _headerValuesParser.Parse(headerValue).ToArray();

            Assert.Equal(expectedRawValues.Length, values.Length);

            for (var index = 0; index < expectedRawValues.Length; index++)
            {
                var expectedValue = expectedRawValues[index];
                Assert.Equal(expectedValue, values[index].Raw);
            }
        }

        [Theory]
        [InlineData("\"")]
        [InlineData("\"\\\"")]
        [InlineData("a\"some\\\"text")]
        public void Parse_ThrowsWhenDelimitedStringIsNotTerminated(string headerValue)
        {
            var headerFormatException = Assert.Throws<HeaderFormatException>(() =>
            {
                _headerValuesParser.Parse(headerValue).ToArray();
            });

            Assert.Equal($"Header value «{headerValue}» is missing a string end delimiter.", headerFormatException.Message);
        }

        #endregion
    }
}
