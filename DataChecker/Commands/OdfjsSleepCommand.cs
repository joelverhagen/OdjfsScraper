using OdjfsScraper.DataChecker.Options;

namespace OdjfsScraper.DataChecker.Commands
{
    public abstract class OdfjsSleepCommand : DatabaseCommand
    {
        private readonly SleepOption _odjfsSleepOption;

        protected OdfjsSleepCommand(int minimumSleep, int defaultSleep)
        {
            _odjfsSleepOption = new SleepOption("ODJFS", minimumSleep, defaultSleep);
            HasOption(_odjfsSleepOption);
        }

        public int? OdjfsSleep
        {
            get { return _odjfsSleepOption.Value; }
        }
    }
}