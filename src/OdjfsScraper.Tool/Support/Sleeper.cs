using System.Threading;

namespace OdjfsScraper.Tool.Support
{
    public class Sleeper
    {
        private readonly int _milliseconds;
        private bool _first;

        public Sleeper(int milliseconds)
        {
            _milliseconds = milliseconds;
            _first = true;
        }

        public void Sleep()
        {
            if (_first)
            {
                _first = false;
            }
            else
            {
                Thread.Sleep(_milliseconds);
            }
        }
    }
}