using System;

namespace OdjfsScraper.Parse
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}