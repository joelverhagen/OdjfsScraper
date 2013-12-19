using System.Data.Entity;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Database
{
    public class Entities : DbContext
    {
        public Entities() : base("Odjfs")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // put all of the tables in the same table schema ("odjfs.<table name>")
            modelBuilder.Types()
                .Where(t => !typeof (ChildCareStub).IsAssignableFrom(t) || t == typeof (ChildCareStub))
                .Where(t => !typeof (DetailedChildCare).IsAssignableFrom(t) || t == typeof (DetailedChildCare))
                .Configure(c => c.ToTable(c.ClrType.Name));

            // set up some columns to have unique constraints
            modelBuilder.Entity<ChildCare>()
                .Property(c => c.ExternalUrlId).IsRequired().HasMaxLength(18);

            modelBuilder.Entity<ChildCareStub>()
                .Property(c => c.ExternalUrlId).IsRequired().HasMaxLength(18);

            modelBuilder.Entity<County>()
                .Property(c => c.Name).IsRequired().HasMaxLength(10);

            // inheritance: table-per-hierarchy
            modelBuilder.Entity<ChildCareStub>()
                .Map<TypeAHomeStub>(x => x.Requires("ChildCareType").HasValue(TypeAHomeStub.Discriminator))
                .Map<TypeBHomeStub>(x => x.Requires("ChildCareType").HasValue(TypeBHomeStub.Discriminator))
                .Map<LicensedCenterStub>(x => x.Requires("ChildCareType").HasValue(LicensedCenterStub.Discriminator))
                .Map<DayCampStub>(x => x.Requires("ChildCareType").HasValue(DayCampStub.Discriminator));

            // inheritance: table-per-hierarchy
            modelBuilder.Entity<DetailedChildCare>()
                .Ignore(e => e.DetailedChildCareType)
                .Map<TypeAHome>(x => x.Requires("DetailedChildCareType").HasValue(TypeAHome.DetailedDiscriminator))
                .Map<LicensedCenter>(x => x.Requires("DetailedChildCareType").HasValue(LicensedCenter.DetailedDiscriminator));

            // inheritance: table-per-type
            modelBuilder.Entity<ChildCare>()
                .Map<TypeBHome>(x => x.Requires("ChildCareType").HasValue(TypeBHome.Discriminator))
                .Map<DayCamp>(x => x.Requires("ChildCareType").HasValue(DayCamp.Discriminator))
                .Map<DetailedChildCare>(x => x.Requires("ChildCareType").HasValue(DetailedChildCare.Discriminator));

            // name the ID name columns unique per hierarchy
            modelBuilder
                .Properties()
                .Where(p => p.Name == "Id")
                .Where(p => typeof (ChildCareStub).IsAssignableFrom(p.DeclaringType))
                .Configure(c => c.HasColumnName("ChildCareStubId"));

            modelBuilder
                .Properties()
                .Where(p => p.Name == "Id")
                .Where(p => typeof (ChildCare).IsAssignableFrom(p.DeclaringType))
                .Configure(c => c.HasColumnName("ChildCareId"));

            modelBuilder.Entity<County>()
                .Property(c => c.Id).HasColumnName("CountyId");
        }

        #region Dependent Entites

        public IDbSet<County> Counties { get; set; }

        #endregion

        #region ChildCareStubs

        public IDbSet<ChildCareStub> ChildCareStubs { get; set; }
        public IDbSet<TypeAHomeStub> TypeAHomeStubs { get; set; }
        public IDbSet<TypeBHomeStub> TypeBHomeStubs { get; set; }
        public IDbSet<LicensedCenterStub> LicensedCenterStubs { get; set; }
        public IDbSet<DayCampStub> DayCampStubs { get; set; }

        #endregion

        #region ChildCares

        public IDbSet<ChildCare> ChildCares { get; set; }
        public IDbSet<DetailedChildCare> DetailedChildCares { get; set; }
        public IDbSet<TypeAHome> TypeAHomes { get; set; }
        public IDbSet<TypeBHome> TypeBHomes { get; set; }
        public IDbSet<LicensedCenter> LicensedCenters { get; set; }
        public IDbSet<DayCamp> DayCamps { get; set; }

        #endregion
    }
}