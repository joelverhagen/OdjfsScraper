using System.Data.Entity.Migrations;

namespace OdjfsScraper.Database.Migrations
{
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.County",
                c => new
                {
                    CountyId = c.Int(false, true),
                    Name = c.String(false, 10),
                    LastScrapedOn = c.DateTime(),
                })
                .PrimaryKey(t => t.CountyId);

            CreateTable(
                "dbo.ChildCare",
                c => new
                {
                    ChildCareId = c.Int(false, true),
                    ChildCareType = c.String(),
                    LastScrapedOn = c.DateTime(false),
                    Latitude = c.Double(),
                    Longitude = c.Double(),
                    LastGeocodedOn = c.DateTime(),
                    CountyId = c.Int(false),
                    ExternalUrlId = c.String(false, 18),
                    ExternalId = c.String(),
                    Name = c.String(),
                    Address = c.String(),
                    City = c.String(),
                    State = c.String(),
                    ZipCode = c.Int(false),
                    PhoneNumber = c.String(),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.County", t => t.CountyId, true)
                .Index(t => t.CountyId);

            CreateTable(
                "dbo.ChildCareStub",
                c => new
                {
                    ChildCareStubId = c.Int(false, true),
                    CountyId = c.Int(),
                    LastScrapedOn = c.DateTime(),
                    ExternalUrlId = c.String(false, 18),
                    Name = c.String(),
                    Address = c.String(),
                    City = c.String(),
                    ChildCareType = c.String(false, 128),
                })
                .PrimaryKey(t => t.ChildCareStubId)
                .ForeignKey("dbo.County", t => t.CountyId)
                .Index(t => t.CountyId);

            CreateTable(
                "dbo.DayCamp",
                c => new
                {
                    ChildCareId = c.Int(false),
                    Owner = c.String(),
                    RegistrationStatus = c.String(),
                    RegistrationBeginDate = c.String(),
                    RegistrationEndDate = c.String(),
                    ChildCareType = c.String(false, 128),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.ChildCare", t => t.ChildCareId)
                .Index(t => t.ChildCareId);

            CreateTable(
                "dbo.DetailedChildCare",
                c => new
                {
                    ChildCareId = c.Int(false),
                    ProviderAgreement = c.String(),
                    Administrators = c.String(),
                    CenterStatus = c.String(),
                    InitialApplicationDate = c.String(),
                    LicenseBeginDate = c.String(),
                    LicenseExpirationDate = c.String(),
                    SutqRating = c.Int(),
                    Infants = c.Boolean(false),
                    YoungToddlers = c.Boolean(false),
                    OlderToddlers = c.Boolean(false),
                    Preschoolers = c.Boolean(false),
                    Gradeschoolers = c.Boolean(false),
                    ChildCareFoodProgram = c.Boolean(false),
                    Naeyc = c.Boolean(false),
                    Necpa = c.Boolean(false),
                    Naccp = c.Boolean(false),
                    Nafcc = c.Boolean(false),
                    Coa = c.Boolean(false),
                    Acsi = c.Boolean(false),
                    MondayReported = c.Boolean(false),
                    MondayBegin = c.DateTime(),
                    MondayEnd = c.DateTime(),
                    TuesdayReported = c.Boolean(false),
                    TuesdayBegin = c.DateTime(),
                    TuesdayEnd = c.DateTime(),
                    WednesdayReported = c.Boolean(false),
                    WednesdayBegin = c.DateTime(),
                    WednesdayEnd = c.DateTime(),
                    ThursdayReported = c.Boolean(false),
                    ThursdayBegin = c.DateTime(),
                    ThursdayEnd = c.DateTime(),
                    FridayReported = c.Boolean(false),
                    FridayBegin = c.DateTime(),
                    FridayEnd = c.DateTime(),
                    SaturdayReported = c.Boolean(false),
                    SaturdayBegin = c.DateTime(),
                    SaturdayEnd = c.DateTime(),
                    SundayReported = c.Boolean(false),
                    SundayBegin = c.DateTime(),
                    SundayEnd = c.DateTime(),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.ChildCare", t => t.ChildCareId)
                .Index(t => t.ChildCareId);

            CreateTable(
                "dbo.LicensedCenter",
                c => new
                {
                    ChildCareId = c.Int(false),
                    DetailedChildCareType = c.String(false, 128),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.DetailedChildCare", t => t.ChildCareId)
                .Index(t => t.ChildCareId);

            CreateTable(
                "dbo.TypeAHome",
                c => new
                {
                    ChildCareId = c.Int(false),
                    DetailedChildCareType = c.String(false, 128),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.DetailedChildCare", t => t.ChildCareId)
                .Index(t => t.ChildCareId);

            CreateTable(
                "dbo.TypeBHome",
                c => new
                {
                    ChildCareId = c.Int(false),
                    CertificationBeginDate = c.String(),
                    CertificationExpirationDate = c.String(),
                    ChildCareType = c.String(false, 128),
                })
                .PrimaryKey(t => t.ChildCareId)
                .ForeignKey("dbo.ChildCare", t => t.ChildCareId)
                .Index(t => t.ChildCareId);

            // add some unique constraints
            CreateIndex("dbo.County", new[] {"Name"}, true);
            CreateIndex("dbo.ChildCare", new[] {"ExternalUrlId"}, true);
            CreateIndex("dbo.ChildCareStub", new[] {"ExternalUrlId"}, true);
        }

        public override void Down()
        {
            // drop some unique constraints
            DropIndex("dbo.ChildCareStub", new[] {"ExternalUrlId"});
            DropIndex("dbo.ChildCare", new[] {"ExternalUrlId"});
            DropIndex("dbo.County", new[] {"Name"});

            DropForeignKey("dbo.TypeBHome", "ChildCareId", "dbo.ChildCare");
            DropForeignKey("dbo.TypeAHome", "ChildCareId", "dbo.DetailedChildCare");
            DropForeignKey("dbo.LicensedCenter", "ChildCareId", "dbo.DetailedChildCare");
            DropForeignKey("dbo.DetailedChildCare", "ChildCareId", "dbo.ChildCare");
            DropForeignKey("dbo.DayCamp", "ChildCareId", "dbo.ChildCare");
            DropForeignKey("dbo.ChildCareStub", "CountyId", "dbo.County");
            DropForeignKey("dbo.ChildCare", "CountyId", "dbo.County");
            DropIndex("dbo.TypeBHome", new[] {"ChildCareId"});
            DropIndex("dbo.TypeAHome", new[] {"ChildCareId"});
            DropIndex("dbo.LicensedCenter", new[] {"ChildCareId"});
            DropIndex("dbo.DetailedChildCare", new[] {"ChildCareId"});
            DropIndex("dbo.DayCamp", new[] {"ChildCareId"});
            DropIndex("dbo.ChildCareStub", new[] {"CountyId"});
            DropIndex("dbo.ChildCare", new[] {"CountyId"});
            DropTable("dbo.TypeBHome");
            DropTable("dbo.TypeAHome");
            DropTable("dbo.LicensedCenter");
            DropTable("dbo.DetailedChildCare");
            DropTable("dbo.DayCamp");
            DropTable("dbo.ChildCareStub");
            DropTable("dbo.ChildCare");
            DropTable("dbo.County");
        }
    }
}