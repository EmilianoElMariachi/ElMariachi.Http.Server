using System;

namespace ElMariachi.Http.Exceptions
{
    public class RequestUriToolLongException : Exception
    {
        public RequestUriToolLongException(int limit)
        {
            Limit = limit;
        }

        public int Limit { get; }
    }
}