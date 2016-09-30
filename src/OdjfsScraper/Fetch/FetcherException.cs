using System;

namespace OdjfsScraper.Fetch
{
    public class FetcherException : Exception
    {
        public FetcherException(string message) : base(message)
        {
        }
    }
}