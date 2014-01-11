using System;

namespace OdjfsScraper.Synchronizer.Support
{
    public class SynchronizerException : Exception
    {
        public SynchronizerException(string message) : base(message)
        {
        }
    }
}