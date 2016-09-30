using System.IO;
using Microsoft.EntityFrameworkCore;
using OdjfsScraper.Models;

namespace OdjfsScraper.Database
{
    public class OdjfsContext : DbContext
    {
        public OdjfsContext(DbContextOptions<OdjfsContext> options) : base(options)
        {
        }

        public OdjfsContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "OdjfsScraper.sqlite3");
            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<County>()
                .HasIndex(x => x.Name)
                .IsUnique(true);

            modelBuilder
                .Entity<ChildCareStub>()
                .HasIndex(x => x.ExternalUrlId)
                .IsUnique(true);

            modelBuilder
                .Entity<ChildCare>()
                .HasIndex(x => x.ExternalUrlId)
                .IsUnique(true);
        }

        public DbSet<County> Counties { get; set; }

        public DbSet<ChildCareStub> ChildCareStubs { get; set; }
        public DbSet<TypeAHomeStub> TypeAHomeStubs { get; set; }
        public DbSet<TypeBHomeStub> TypeBHomeStubs { get; set; }
        public DbSet<LicensedCenterStub> LicensedCenterStubs { get; set; }
        public DbSet<DayCampStub> DayCampStubs { get; set; }

        public DbSet<ChildCare> ChildCares { get; set; }
        public DbSet<DetailedChildCare> DetailedChildCares { get; set; }
        public DbSet<TypeAHome> TypeAHomes { get; set; }
        public DbSet<TypeBHome> TypeBHomes { get; set; }
        public DbSet<LicensedCenter> LicensedCenters { get; set; }
        public DbSet<DayCamp> DayCamps { get; set; }
    }
}