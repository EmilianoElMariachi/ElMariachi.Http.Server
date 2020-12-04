using System;

namespace ElMariachi.Http.Exceptions
{
    public class RequestFormatException : Exception
    {
        public RequestFormatException(string message) : base(message)
        {
        }
    }
}