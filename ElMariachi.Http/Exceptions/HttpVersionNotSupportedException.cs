using System;

namespace ElMariachi.Http.Exceptions
{
    public class HttpVersionNotSupportedException : Exception
    {
        public HttpVersionNotSupportedException(string?[]? supportedVersions, string? notSupportedVersion)
        {
            SupportedVersions = supportedVersions;
            NotSupportedVersion = notSupportedVersion;
        }

        public string?[]? SupportedVersions { get; }

        public string? NotSupportedVersion { get; }
    }
}