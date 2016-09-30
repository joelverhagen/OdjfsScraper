using System;

namespace OdjfsScraper.Synchronize
{
    public class SynchronizerException : Exception
    {
        public SynchronizerException(string message) : base(message)
        {
        }
    }
}