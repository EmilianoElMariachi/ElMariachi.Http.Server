using System.Collections.Generic;
using ElMariachi.Http.Header.Unmanaged;
using Xunit;

namespace ElMariachi.Http.Header.Test
{
    public class HttpHeadersTest
    {
        [Fact]
        public void HeadersEnumerator_CurrentIsNullConstruction()
        {
            var dictionary = new Dictionary<string, Header>
            {
                {"content-length", new UnmanagedHeader("Content-Length")}
            };

            var headersEnumerator = new HttpHeaders.HeadersEnumerator(dictionary.GetEnumerator());

            var current = headersEnumerator.Current;

            Assert.Null(current);
        }

        [Fact]
        public void HeadersEnumerator_MoveNext()
        {
            var dictionary = new Dictionary<string, Header>
            {
                {"content-length", new UnmanagedHeader("Content-Length")
                {
                    RawValue = "51154",
                }},
                {"content-type", new UnmanagedHeader("Content-Type")
                {
                    RawValue = "text/html",
                }}
            };

            var headersEnumerator = new HttpHeaders.HeadersEnumerator(dictionary.GetEnumerator());

            Assert.True(headersEnumerator.MoveNext());
            Assert.Equal("Content-Length", headersEnumerator.Current.Name);
            Assert.Equal("51154", headersEnumerator.Current.RawValue);

            Assert.True(headersEnumerator.MoveNext());
            Assert.Equal("Content-Type", headersEnumerator.Current.Name);
            Assert.Equal("text/html", headersEnumerator.Current.RawValue);

            Assert.False(headersEnumerator.MoveNext());
        }
    }
}
