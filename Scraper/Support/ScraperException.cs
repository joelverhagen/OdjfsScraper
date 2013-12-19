using System;

namespace OdjfsScraper.Scraper.Support
{
    public class ScraperException : Exception
    {
        public ScraperException(string message) : base(message)
        {
        }
    }
}