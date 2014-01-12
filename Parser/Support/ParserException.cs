using System;

namespace OdjfsScraper.Parser.Support
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}