using OdjfsScraper.Database;

namespace OdjfsScraper.Tool.Commands
{
    public abstract class DatabaseCommand : Command
    {
        private readonly IMigrationService _migrationService;

        public DatabaseCommand(IMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);
            
            _migrationService.Migrate();

            return null;
        }
    }
}