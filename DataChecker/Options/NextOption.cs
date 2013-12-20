using ManyConsole;

namespace OdjfsScraper.DataChecker.Options
{
    public class NextOption : IValueOption<int?>
    {
        private readonly string _action;
        private readonly string _pluralEntityName;

        public NextOption(string action, string pluralEntityName)
        {
            _action = action;
            _pluralEntityName = pluralEntityName;
        }

        public string Prototype
        {
            get { return "next="; }
        }

        public string Description
        {
            get { return string.Format("{0} the specified number of {1} (e.g. 2)", _action, _pluralEntityName); }
        }

        public void Validate()
        {
            if (Value.HasValue && Value < 0)
            {
                throw new ConsoleHelpAsException("the --next value you specify must be a non-negative integer");
            }
        }

        public int? Value { get; set; }
    }
}