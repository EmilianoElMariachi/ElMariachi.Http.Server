using System;

namespace ElMariachi.Http.Exceptions
{
    public class StreamLimitException : Exception
    {
        public StreamLimitException(in int limit) : base($"Maximum number of readable byte(s) reached (limit={limit}).")
        {
            Limit = limit;
        }

        /// <summary>
        /// Get the limit
        /// </summary>
        public int Limit { get; }
    }
}