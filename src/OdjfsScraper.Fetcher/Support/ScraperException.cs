using System;

namespace OdjfsScraper.Fetcher.Support
{
    public class ScraperException : Exception
    {
        public ScraperException(string message) : base(message)
        {
        }
    }
}