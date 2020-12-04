using System.Linq;
using Xunit;

namespace ElMariachi.Http.Header.Test
{
    public class HttpResponseHeadersTest
    {
        [Fact]
        public void NotHeadersAtConstruction()
        {
            var httpResponseHeaders = new HttpResponseHeaders();

            Assert.Empty(httpResponseHeaders);
        }

        [Fact]
        public void AddedHeadersAreIterated()
        {
            var httpResponseHeaders = new HttpResponseHeaders
            {
                ["SomeHeader"] = "SomeValue",
                ["Some-Header-2"] = "Some Value 2",
            };

            var headers = httpResponseHeaders.ToArray();

            Assert.Equal("SomeHeader", headers[0].Name);
            Assert.Equal("SomeValue", headers[0].RawValue);

            Assert.Equal("Some-Header-2", headers[1].Name);
            Assert.Equal("Some Value 2", headers[1].RawValue);
        }

        [Fact]
        public void CanNullifyHeadersWhileIterating()
        {
            var httpResponseHeaders = new HttpResponseHeaders
            {
                ["SomeHeader"] = "SomeValue",
                ["Some-Header-2"] = "Some Value 2",
                ["Some-Header-3"] = "Some Value 3",
            };

            foreach (var header in httpResponseHeaders)
            {
                httpResponseHeaders[header.Name] = null;
            }

            Assert.Empty(httpResponseHeaders);
        }
    }
}
