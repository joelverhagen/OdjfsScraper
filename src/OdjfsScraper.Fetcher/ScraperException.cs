using System;

namespace OdjfsScraper.Fetchers
{
    public class FetcherException : Exception
    {
        public FetcherException(string message) : base(message)
        {
        }
    }
}