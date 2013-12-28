using System;
using System.Data.Entity;
using System.Data.SqlClient;
using NLog;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Commands
{
    public class BackupCommand : Command
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string BackupDatabaseQuery = "BACKUP DATABASE @Database TO DISK = @Path WITH COPY_ONLY, INIT";

        public BackupCommand()
        {
            IsCommand("backup", "backup the ODJFS SQL Server database");
            HasRequiredOption("path=", "the file path of where the backup will be written to; SQL Server must have access to this location", v => Path = v);
        }

        public string Path { get; set; }

        public override int Run(string[] remainingArguments)
        {
            string fullPath = System.IO.Path.GetFullPath(Path);
            Logger.Trace("Backing up to:{0}  {1}", Environment.NewLine, fullPath);

            using (var ctx = new Entities())
            {
                ctx.Database.ExecuteSqlCommand(
                    TransactionalBehavior.DoNotEnsureTransaction,
                    BackupDatabaseQuery,
                    new SqlParameter("@Database", ctx.Database.Connection.Database),
                    new SqlParameter("@Path", fullPath));
            }

            return 0;
        }
    }
}