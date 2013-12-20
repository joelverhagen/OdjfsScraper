using ManyConsole;

namespace OdjfsScraper.DataChecker.Options
{
    public class SleepOption : IValueOption<int?>
    {
        private readonly int _minimum;
        private readonly string _requestName;

        public SleepOption(string requestName, int minimum)
        {
            _requestName = requestName;
            _minimum = minimum;
            Value = minimum;
        }

        public string Prototype
        {
            get { return string.Format("{0}-sleep=", _requestName.ToLower()); }
        }

        public string Description
        {
            get { return string.Format("sleep the specified number of milliseconds between each {0} request (minimum: {1}, default: {1})", _requestName, _minimum); }
        }

        public void Validate()
        {
            if (Value.HasValue && Value < _minimum)
            {
                throw new ConsoleHelpAsException(string.Format("The minimum sleep time between {0} requests is {1} milliseconds.", _requestName, _minimum));
            }
        }

        public int? Value { get; set; }
    }
}