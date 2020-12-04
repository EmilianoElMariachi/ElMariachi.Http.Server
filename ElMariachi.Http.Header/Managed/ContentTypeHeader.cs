using System;
using System.Collections.Generic;
using System.Text;
using ElMariachi.Http.Header.Utils;

namespace ElMariachi.Http.Header.Managed
{
    public class ContentTypeHeader : ManagedHeader, IContentTypeHeader
    {

        private const char Delimiter = ';';

        private string? _mediaType;
        private Encoding? _charset;
        private string? _boundary;
        private string? _rawValue;

        public override string Name => HttpConst.Headers.ContentType;

        public string? MediaType
        {
            get => _mediaType;
            set
            {
                _mediaType = value;

                TryParseMediaType(value, out var _, out var type, out var subType);
                Type = type;
                SubType = subType;
                IsDirty = true;
            }
        }

        public string? Type { get; private set; }

        public string? SubType { get; private set; }

        public Encoding? Charset
        {
            get => _charset;
            set
            {
                _charset = value;
                IsDirty = true;
            }
        }

        public string? Boundary
        {
            get => _boundary;
            set
            {
                _boundary = value;
                IsDirty = true;
            }
        }

        private bool IsDirty { get; set; } = false;

        protected override string? GetRawValue()
        {
            if (!IsDirty)
                return _rawValue;

            var items = new List<string>();

            if (_mediaType != null)
                items.Add(_mediaType);

            if (_charset != null)
                items.Add($"charset={_charset.WebName}");

            if (_boundary != null)
                items.Add($"boundary={_boundary}");

            _rawValue = string.Join(Delimiter, items);

            IsDirty = false;

            return _rawValue;
        }

        protected override void SetRawValue(string? rawValue)
        {
            _rawValue = rawValue;

            _mediaType = null;
            _charset = null;
            _boundary = null;
            Type = null;
            SubType = null;

            if (rawValue == null)
                return;

            foreach (var interpreted in HeaderValuesParser.ParseInterpretedStatic(rawValue, Delimiter))
            {
                if (_mediaType == null && TryParseMediaType(interpreted, out var mediaType, out var type, out var subType))
                {
                    _mediaType = mediaType;
                    Type = type;
                    SubType = subType;
                    continue;
                }

                if (_charset == null && TryParseCharset(interpreted, out var encoding))
                {
                    _charset = encoding;
                    continue;
                }

                if (_boundary == null && TryParseBoundary(interpreted, out var boundary))
                {
                    _boundary = boundary;
                }
            }
        }

        /// <summary>
        /// boundary=THIS_STRING_SEPARATES
        /// </summary>
        /// <param name="interpreted"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private static bool TryParseBoundary(string interpreted, out string? boundary)
        {
            boundary = null;

            var parts = interpreted.Split('=', 2);
            if (parts.Length != 2 || !string.Equals(parts[0].Trim(), "boundary", StringComparison.OrdinalIgnoreCase))
                return false;

            var boundaryTmp = parts[1].Trim();
            if (boundaryTmp.Length < 1 || boundaryTmp.Length > 70)
                return false;

            boundary = boundaryTmp;
            return true;
        }

        /// <summary>
        /// charset=utf-8
        /// </summary>
        /// <param name="interpreted"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static bool TryParseCharset(string interpreted, out Encoding? encoding)
        {
            encoding = null;

            var parts = interpreted.Split('=', 2);
            if (parts.Length != 2 || !string.Equals(parts[0].Trim(), "charset", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                encoding = Encoding.GetEncoding(parts[1].Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// type/subtype
        /// </summary>
        /// <param name="mediaTypeRaw"></param>
        /// <param name="mediaType"></param>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        /// <returns></returns>
        private static bool TryParseMediaType(string? mediaTypeRaw, out string? mediaType, out string? type, out string? subType)
        {
            mediaType = null;
            type = null;
            subType = null;

            if (mediaTypeRaw == null)
                return false;

            var parts = mediaTypeRaw.Split('/', 2);
            if (parts.Length == 2 && HeaderUtil.IsToken(parts[0]) && HeaderUtil.IsToken(parts[1]))
            {
                type = parts[0];
                subType = parts[1];
                mediaType = mediaTypeRaw;
                return true;
            }
            return false;
        }

    }
}