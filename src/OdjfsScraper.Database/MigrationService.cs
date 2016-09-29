using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Models;

namespace OdjfsScraper.Database
{
    public class MigrationService : IMigrationService
    {
        private static readonly string[] CountyNames =
        {
            "ADAMS", "ALLEN", "ASHLAND", "ASHTABULA", "ATHENS", "AUGLAIZE", "BELMONT", "BROWN",
            "BUTLER", "CARROLL", "CHAMPAIGN", "CLARK", "CLERMONT", "CLINTON", "COLUMBIANA", "COSHOCTON",
            "CRAWFORD", "CUYAHOGA", "DARKE", "DEFIANCE", "DELAWARE", "ERIE", "FAIRFIELD", "FAYETTE",
            "FRANKLIN", "FULTON", "GALLIA", "GEAUGA", "GREENE", "GUERNSEY", "HAMILTON", "HANCOCK",
            "HARDIN", "HARRISON", "HENRY", "HIGHLAND", "HOCKING", "HOLMES", "HURON", "JACKSON",
            "JEFFERSON", "KNOX", "LAKE", "LAWRENCE", "LICKING", "LOGAN", "LORAIN", "LUCAS",
            "MADISON", "MAHONING", "MARION", "MEDINA", "MEIGS", "MERCER", "MIAMI", "MONROE",
            "MONTGOMERY", "MORGAN", "MORROW", "MUSKINGUM", "NOBLE", "OTTAWA", "PAULDING", "PERRY",
            "PICKAWAY", "PIKE", "PORTAGE", "PREBLE", "PUTNAM", "RICHLAND", "ROSS", "SANDUSKY",
            "SCIOTO", "SENECA", "SHELBY", "STARK", "SUMMIT", "TRUMBULL", "TUSCARAWAS", "UNION",
            "VAN WERT", "VINTON", "WARREN", "WASHINGTON", "WAYNE", "WILLIAMS", "WOOD", "WYANDOT"
        };

        private readonly ILogger<MigrationService> _logger;

        public MigrationService(ILogger<MigrationService> logger)
        {
            _logger = logger;
        }

        public void Migrate()
        {
            using (var context = new OdjfsContext())
            {
                ApplyMigrations(context);
                SeedData(context);
            }
        }

        private void ApplyMigrations(OdjfsContext context)
        {
            var historyRepository = context.Database.GetService<IHistoryRepository>();
            var appliedMigrations = historyRepository
                .GetAppliedMigrations()
                .Select(x => x.MigrationId)
                .ToList();

            var migrationAssembly = context.Database.GetService<IMigrationsAssembly>();
            var allMigrations = migrationAssembly.Migrations.Select(x => x.Key);
            var migrationsToApply = allMigrations.Except(appliedMigrations).OrderBy(x => x).ToList();

            if (appliedMigrations.Count == 0)
            {
                _logger.LogInformation("The database has not been created yet. This will happen now.");
            }
            else if (migrationsToApply.Count > 0)
            {
                _logger.LogInformation("The database schema is out of date and must be migrated. This will happen now.");
            }
            else
            {
                _logger.LogInformation("The database schema is up to date.");
            }

            var migrator = context.Database.GetService<IMigrator>();

            foreach (var migrationId in migrationsToApply)
            {
                _logger.LogInformation("Executing migration '{migrationId}'.", migrationId);
                var migrationStopwatch = Stopwatch.StartNew();
                migrator.Migrate(migrationId);
                _logger.LogInformation("Migration '{migrationId}' completed in {duration}.", migrationId, migrationStopwatch.Elapsed);
            }
        }

        private void SeedData(OdjfsContext context)
        {
            var existingCounties = context
                .Counties
                .Select(x => x.Name)
                .ToList();

            var newCounties = CountyNames
                .Except(existingCounties)
                .OrderBy(x => x)
                .Select(x => new County { Name = x })
                .ToList();

            if (!newCounties.Any())
            {
                return;
            }

            _logger.LogInformation("Seeding {count} counties.", newCounties.Count);
            context.Counties.AddRange(newCounties);
            context.SaveChanges();
        }
    }
}