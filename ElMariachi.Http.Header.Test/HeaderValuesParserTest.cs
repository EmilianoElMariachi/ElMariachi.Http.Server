using System.Linq;
using ElMariachi.Http.Header.Exceptions;
using ElMariachi.Http.Header.Utils;
using Xunit;

namespace ElMariachi.Http.Header.Test
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

        #region ParseInterpreted

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
                _headerValuesParser.ParseInterpreted(headerValue).ToArray();
            });

            Assert.Equal($"Header value «{headerValue}» is missing a string end delimiter.", headerFormatException.Message);
        }

        #endregion
    }
}
