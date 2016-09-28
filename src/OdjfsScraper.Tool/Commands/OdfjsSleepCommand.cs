using OdjfsScraper.Database;
using OdjfsScraper.Tool.Options;

namespace OdjfsScraper.Tool.Commands
{
    public abstract class OdfjsSleepCommand : DatabaseCommand
    {
        private readonly SleepOption _odjfsSleepOption;

        protected OdfjsSleepCommand(int minimumSleep, int defaultSleep, IMigrationService migrationService)
            : base(migrationService)
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