using System;

namespace OdjfsScraper.Parsers
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}