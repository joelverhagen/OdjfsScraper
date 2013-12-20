using OdjfsScraper.DataChecker.Options;

namespace OdjfsScraper.DataChecker.Commands
{
    public abstract class OdfjsSleepCommand : Command
    {
        private readonly SleepOption _odjfsSleepOption;

        protected OdfjsSleepCommand(int minimumSleep)
        {
            _odjfsSleepOption = new SleepOption("ODJFS", minimumSleep);
            HasOption(_odjfsSleepOption);
        }

        public int? OdjfsSleep
        {
            get { return _odjfsSleepOption.Value; }
        }
    }
}