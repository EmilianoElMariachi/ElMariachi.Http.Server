using System;
using System.IO;
using System.IO.Compression;
using ElMariachi.Http.Exceptions;
using ElMariachi.Http.Header;
using ElMariachi.Http.Server.Streams.Input;

namespace ElMariachi.Http.Server.Services
{
    internal class HttpInputStreamDecodingStrategy : IHttpInputStreamDecodingStrategy
    {
        public Stream Create(Stream inputStream, IHttpHeader header)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            if (header == null) throw new ArgumentNullException(nameof(header));


            var streamTmp = inputStream;

            var contentLength = header.Headers.ContentLength.Value;
            if (contentLength != null)
            {
                if(contentLength <= 0)
                    return new NullInputStream();

                streamTmp = new FixLengthInputStream(streamTmp, contentLength.Value);
            }

            foreach (var transferEncodingValue in header.Headers.TransferEncoding.Values)
            {
                var cleanedTransferEncoding = transferEncodingValue.ToLower().Trim();

                switch (cleanedTransferEncoding)
                {
                    case "identity":
                        break;
                    case "gzip":
                        streamTmp = new GZipStream(streamTmp, CompressionLevel.Fastest);
                        break;
                    case "deflate":
                        streamTmp = new DeflateStream(streamTmp, CompressionLevel.Fastest);
                        break;
                    case "chunked":
                        streamTmp = new ChunkedInputStream(streamTmp);
                        break;
                    default:
                        throw new InputTransferEncodingNotSupportedException();
                }
            }

            return streamTmp;
        }

    }
}