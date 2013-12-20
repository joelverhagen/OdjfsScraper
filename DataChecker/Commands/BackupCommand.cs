using System;
using System.Data.Entity;
using System.Data.SqlClient;
using OdjfsScraper.Database;

namespace OdjfsScraper.DataChecker.Commands
{
    public class BackupCommand : Command
    {
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
            Console.WriteLine("Backing up to:");
            Console.WriteLine("  {0}", fullPath);

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