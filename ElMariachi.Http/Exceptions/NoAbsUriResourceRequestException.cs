using System;

namespace ElMariachi.Http.Exceptions
{
    public class NoAbsUriResourceRequestException : Exception
    {
        public NoAbsUriResourceRequestException(string message) : base(message)
        {
        }
    }
}