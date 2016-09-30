using System;

namespace OdjfsScraper.Synchronizers
{
    public class SynchronizerException : Exception
    {
        public SynchronizerException(string message) : base(message)
        {
        }
    }
}