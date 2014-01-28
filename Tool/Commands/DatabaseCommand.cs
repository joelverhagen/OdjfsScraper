using System.Data.Entity.Migrations;
using System.Linq;
using NLog;
using OdjfsScraper.Database.Migrations;

namespace OdjfsScraper.Tool.Commands
{
    public abstract class DatabaseCommand : Command
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);

            // make sure the database is initialized
            var configuration = new Configuration();
            var migrator = new DbMigrator(configuration);
            string[] pendingMigrations = migrator.GetPendingMigrations().ToArray();
            string[] databaseMigrations = migrator.GetDatabaseMigrations().ToArray();
            if (pendingMigrations.Length > 0)
            {
                if (databaseMigrations.Length == 0)
                {
                    Logger.Trace("The database has not been created yet. This will happen now.");
                }
                else
                {
                    Logger.Trace("The database schema is out of data and must be migrated. This will happen now.");
                }
                migrator.Update();
            }

            return null;
        }
    }
}