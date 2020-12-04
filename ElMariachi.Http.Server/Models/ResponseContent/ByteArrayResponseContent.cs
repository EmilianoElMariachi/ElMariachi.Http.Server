using System;
using System.IO;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Models.ResponseContent
{
    public class ByteArrayResponseContent : IHttpResponseContent
    {
        private readonly byte[] _bytes;

        public ByteArrayResponseContent(byte[] bytes)
        {
            _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public virtual void FillHeaders(IHttpResponseHeaders headers)
        {
            var currentContentLength = headers.ContentLength.Value;
            if (currentContentLength != null)
                throw new InvalidOperationException($"{HttpConst.Headers.ContentLength} header is managed by {this.GetType().Name} and was not expected to be already set.");

            headers.ContentLength.Value = _bytes.Length;
        }

        public void CopyToStream(Stream stream)
        {
            stream.Write(_bytes);
        }

        public void Dispose()
        {
        }
    }
}