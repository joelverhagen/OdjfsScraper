using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;

namespace OdjfsScraper.Database
{
    public class Entities : DbContext
    {
        public Entities(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public Entities(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public Entities(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public Entities(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
        }

        public Entities(string nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model)
        {
        }

        public Entities(DbCompiledModel model) : base(model)
        {
        }

        public Entities()
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // name all tables
            modelBuilder.Types().Configure(c => c.ToTable(c.ClrType.Name));

            // set up some columns to have unique constraints
            modelBuilder.Entity<ChildCare>()
                .Property(c => c.ExternalUrlId).IsRequired().HasMaxLength(18);

            modelBuilder.Entity<ChildCareStub>()
                .Property(c => c.ExternalUrlId).IsRequired().HasMaxLength(18);

            modelBuilder.Entity<County>()
                .Property(c => c.Name).IsRequired().HasMaxLength(10);

            // name the identity columns in a sane fashion
            modelBuilder.Entity<County>().Property(c => c.Id).HasColumnName("CountyId");
            modelBuilder.Entity<ChildCare>().Property(c => c.Id).HasColumnName("ChildCareId");
            modelBuilder.Entity<ChildCareStub>().Property(c => c.Id).HasColumnName("ChildCareStubId");
        }

        #region County Entities

        public IDbSet<County> Counties { get; set; }

        #endregion

        #region ChildCareStub Entities

        public IDbSet<ChildCareStub> ChildCareStubs { get; set; }
        public IDbSet<TypeAHomeStub> TypeAHomeStubs { get; set; }
        public IDbSet<TypeBHomeStub> TypeBHomeStubs { get; set; }
        public IDbSet<LicensedCenterStub> LicensedCenterStubs { get; set; }
        public IDbSet<DayCampStub> DayCampStubs { get; set; }

        #endregion

        #region ChildCare Entities

        public IDbSet<ChildCare> ChildCares { get; set; }
        public IDbSet<DetailedChildCare> DetailedChildCares { get; set; }
        public IDbSet<TypeAHome> TypeAHomes { get; set; }
        public IDbSet<TypeBHome> TypeBHomes { get; set; }
        public IDbSet<LicensedCenter> LicensedCenters { get; set; }
        public IDbSet<DayCamp> DayCamps { get; set; }

        #endregion
    }
}