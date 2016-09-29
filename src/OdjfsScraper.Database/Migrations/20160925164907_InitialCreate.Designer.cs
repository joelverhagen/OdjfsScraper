using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using OdjfsScraper.Database;

namespace OdjfsScraper.Database.Migrations
{
    [DbContext(typeof(OdjfsContext))]
    [Migration("20160925164907_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("OdjfsScraper.Models.ChildCare", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("City");

                    b.Property<int>("CountyId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("ExternalId");

                    b.Property<string>("ExternalUrlId");

                    b.Property<DateTime?>("LastGeocodedOn");

                    b.Property<DateTime>("LastScrapedOn");

                    b.Property<double?>("Latitude");

                    b.Property<double?>("Longitude");

                    b.Property<string>("Name");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("State");

                    b.Property<int>("ZipCode");

                    b.HasKey("Id");

                    b.HasIndex("CountyId");

                    b.HasIndex("ExternalUrlId")
                        .IsUnique();

                    b.ToTable("ChildCares");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ChildCare");
                });

            modelBuilder.Entity("OdjfsScraper.Models.ChildCareStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("City");

                    b.Property<int?>("CountyId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("ExternalUrlId");

                    b.Property<DateTime?>("LastScrapedOn");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CountyId");

                    b.HasIndex("ExternalUrlId")
                        .IsUnique();

                    b.ToTable("ChildCareStubs");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ChildCareStub");
                });

            modelBuilder.Entity("OdjfsScraper.Model.County", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("LastScrapedOn");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Counties");
                });

            modelBuilder.Entity("OdjfsScraper.Models.DayCamp", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCare");

                    b.Property<string>("Owner");

                    b.Property<string>("RegistrationBeginDate");

                    b.Property<string>("RegistrationEndDate");

                    b.Property<string>("RegistrationStatus");

                    b.ToTable("DayCamp");

                    b.HasDiscriminator().HasValue("DayCamp");
                });

            modelBuilder.Entity("OdjfsScraper.Models.DetailedChildCare", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCare");

                    b.Property<bool>("Acsi");

                    b.Property<string>("Administrators");

                    b.Property<string>("CenterStatus");

                    b.Property<bool>("ChildCareFoodProgram");

                    b.Property<bool>("Coa");

                    b.Property<DateTime?>("FridayBegin");

                    b.Property<DateTime?>("FridayEnd");

                    b.Property<bool>("FridayReported");

                    b.Property<bool>("Gradeschoolers");

                    b.Property<bool>("Infants");

                    b.Property<string>("InitialApplicationDate");

                    b.Property<string>("LicenseBeginDate");

                    b.Property<string>("LicenseExpirationDate");

                    b.Property<DateTime?>("MondayBegin");

                    b.Property<DateTime?>("MondayEnd");

                    b.Property<bool>("MondayReported");

                    b.Property<bool>("Naccp");

                    b.Property<bool>("Naeyc");

                    b.Property<bool>("Nafcc");

                    b.Property<bool>("Necpa");

                    b.Property<bool>("OlderToddlers");

                    b.Property<bool>("Preschoolers");

                    b.Property<string>("ProviderAgreement");

                    b.Property<DateTime?>("SaturdayBegin");

                    b.Property<DateTime?>("SaturdayEnd");

                    b.Property<bool>("SaturdayReported");

                    b.Property<DateTime?>("SundayBegin");

                    b.Property<DateTime?>("SundayEnd");

                    b.Property<bool>("SundayReported");

                    b.Property<int?>("SutqRating");

                    b.Property<DateTime?>("ThursdayBegin");

                    b.Property<DateTime?>("ThursdayEnd");

                    b.Property<bool>("ThursdayReported");

                    b.Property<DateTime?>("TuesdayBegin");

                    b.Property<DateTime?>("TuesdayEnd");

                    b.Property<bool>("TuesdayReported");

                    b.Property<DateTime?>("WednesdayBegin");

                    b.Property<DateTime?>("WednesdayEnd");

                    b.Property<bool>("WednesdayReported");

                    b.Property<bool>("YoungToddlers");

                    b.ToTable("DetailedChildCare");

                    b.HasDiscriminator().HasValue("DetailedChildCare");
                });

            modelBuilder.Entity("OdjfsScraper.Models.TypeBHome", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCare");

                    b.Property<string>("CertificationBeginDate");

                    b.Property<string>("CertificationExpirationDate");

                    b.ToTable("TypeBHome");

                    b.HasDiscriminator().HasValue("TypeBHome");
                });

            modelBuilder.Entity("OdjfsScraper.Models.DayCampStub", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCareStub");


                    b.ToTable("DayCampStub");

                    b.HasDiscriminator().HasValue("DayCampStub");
                });

            modelBuilder.Entity("OdjfsScraper.Models.LicensedCenterStub", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCareStub");


                    b.ToTable("LicensedCenterStub");

                    b.HasDiscriminator().HasValue("LicensedCenterStub");
                });

            modelBuilder.Entity("OdjfsScraper.Models.TypeAHomeStub", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCareStub");


                    b.ToTable("TypeAHomeStub");

                    b.HasDiscriminator().HasValue("TypeAHomeStub");
                });

            modelBuilder.Entity("OdjfsScraper.Models.TypeBHomeStub", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.ChildCareStub");


                    b.ToTable("TypeBHomeStub");

                    b.HasDiscriminator().HasValue("TypeBHomeStub");
                });

            modelBuilder.Entity("OdjfsScraper.Models.LicensedCenter", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.DetailedChildCare");


                    b.ToTable("LicensedCenter");

                    b.HasDiscriminator().HasValue("LicensedCenter");
                });

            modelBuilder.Entity("OdjfsScraper.Models.TypeAHome", b =>
                {
                    b.HasBaseType("OdjfsScraper.Models.DetailedChildCare");


                    b.ToTable("TypeAHome");

                    b.HasDiscriminator().HasValue("TypeAHome");
                });

            modelBuilder.Entity("OdjfsScraper.Models.ChildCare", b =>
                {
                    b.HasOne("OdjfsScraper.Model.County", "County")
                        .WithMany()
                        .HasForeignKey("CountyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("OdjfsScraper.Models.ChildCareStub", b =>
                {
                    b.HasOne("OdjfsScraper.Model.County", "County")
                        .WithMany()
                        .HasForeignKey("CountyId");
                });
        }
    }
}
