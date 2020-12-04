using System;

namespace ElMariachi.Http.Header.Exceptions
{
    public class HeaderFormatException : Exception
    {
        public HeaderFormatException(string message) : base(message)
        {
        }
    }
}