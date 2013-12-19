using System;

namespace OdjfsScraper.Scraper.Support
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}