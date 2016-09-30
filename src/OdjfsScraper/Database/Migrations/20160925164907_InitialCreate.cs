using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OdjfsScraper.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    LastScrapedOn = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChildCares",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Address = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    CountyId = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: true),
                    ExternalUrlId = table.Column<string>(nullable: true),
                    LastGeocodedOn = table.Column<DateTime>(nullable: true),
                    LastScrapedOn = table.Column<DateTime>(nullable: false),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    ZipCode = table.Column<int>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    RegistrationBeginDate = table.Column<string>(nullable: true),
                    RegistrationEndDate = table.Column<string>(nullable: true),
                    RegistrationStatus = table.Column<string>(nullable: true),
                    Acsi = table.Column<bool>(nullable: true),
                    Administrators = table.Column<string>(nullable: true),
                    CenterStatus = table.Column<string>(nullable: true),
                    ChildCareFoodProgram = table.Column<bool>(nullable: true),
                    Coa = table.Column<bool>(nullable: true),
                    FridayBegin = table.Column<DateTime>(nullable: true),
                    FridayEnd = table.Column<DateTime>(nullable: true),
                    FridayReported = table.Column<bool>(nullable: true),
                    Gradeschoolers = table.Column<bool>(nullable: true),
                    Infants = table.Column<bool>(nullable: true),
                    InitialApplicationDate = table.Column<string>(nullable: true),
                    LicenseBeginDate = table.Column<string>(nullable: true),
                    LicenseExpirationDate = table.Column<string>(nullable: true),
                    MondayBegin = table.Column<DateTime>(nullable: true),
                    MondayEnd = table.Column<DateTime>(nullable: true),
                    MondayReported = table.Column<bool>(nullable: true),
                    Naccp = table.Column<bool>(nullable: true),
                    Naeyc = table.Column<bool>(nullable: true),
                    Nafcc = table.Column<bool>(nullable: true),
                    Necpa = table.Column<bool>(nullable: true),
                    OlderToddlers = table.Column<bool>(nullable: true),
                    Preschoolers = table.Column<bool>(nullable: true),
                    ProviderAgreement = table.Column<string>(nullable: true),
                    SaturdayBegin = table.Column<DateTime>(nullable: true),
                    SaturdayEnd = table.Column<DateTime>(nullable: true),
                    SaturdayReported = table.Column<bool>(nullable: true),
                    SundayBegin = table.Column<DateTime>(nullable: true),
                    SundayEnd = table.Column<DateTime>(nullable: true),
                    SundayReported = table.Column<bool>(nullable: true),
                    SutqRating = table.Column<int>(nullable: true),
                    ThursdayBegin = table.Column<DateTime>(nullable: true),
                    ThursdayEnd = table.Column<DateTime>(nullable: true),
                    ThursdayReported = table.Column<bool>(nullable: true),
                    TuesdayBegin = table.Column<DateTime>(nullable: true),
                    TuesdayEnd = table.Column<DateTime>(nullable: true),
                    TuesdayReported = table.Column<bool>(nullable: true),
                    WednesdayBegin = table.Column<DateTime>(nullable: true),
                    WednesdayEnd = table.Column<DateTime>(nullable: true),
                    WednesdayReported = table.Column<bool>(nullable: true),
                    YoungToddlers = table.Column<bool>(nullable: true),
                    CertificationBeginDate = table.Column<string>(nullable: true),
                    CertificationExpirationDate = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildCares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildCares_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChildCareStubs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Address = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    CountyId = table.Column<int>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    ExternalUrlId = table.Column<string>(nullable: true),
                    LastScrapedOn = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildCareStubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildCareStubs_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildCares_CountyId",
                table: "ChildCares",
                column: "CountyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildCares_ExternalUrlId",
                table: "ChildCares",
                column: "ExternalUrlId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChildCareStubs_CountyId",
                table: "ChildCareStubs",
                column: "CountyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildCareStubs_ExternalUrlId",
                table: "ChildCareStubs",
                column: "ExternalUrlId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Counties_Name",
                table: "Counties",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildCares");

            migrationBuilder.DropTable(
                name: "ChildCareStubs");

            migrationBuilder.DropTable(
                name: "Counties");
        }
    }
}
