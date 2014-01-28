using OdjfsScraper.Tool.Options;

namespace OdjfsScraper.Tool.Commands
{
    public abstract class OdfjsSleepNextCommand : OdfjsSleepCommand
    {
        private readonly NextOption _nextOption;

        protected OdfjsSleepNextCommand(int minimumSleep, int defaultSleep, string nextAction, string pluralEntityName) : base(minimumSleep, defaultSleep)
        {
            _nextOption = new NextOption(nextAction, pluralEntityName);
            HasOption(_nextOption);
        }

        public int? Next
        {
            get { return _nextOption.Value; }
        }
    }
}