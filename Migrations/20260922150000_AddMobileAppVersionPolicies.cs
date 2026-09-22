using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Policy versions app mobile + AppVersion sur UserDevices.
    /// </summary>
    [DbContext(typeof(KelasiNaBiso.Data.KelasiNaBisoDbContext))]
    [Migration("20260922150000_AddMobileAppVersionPolicies")]
    public partial class AddMobileAppVersionPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobileAppVersionPolicies",
                columns: table => new
                {
                    IdMobileAppVersionPolicy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinSupportedVersion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LatestVersion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecommendFromVersion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoreUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IdAuteur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileAppVersionPolicies", x => x.IdMobileAppVersionPolicy);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UX_MobileAppVersionPolicies_Platform",
                table: "MobileAppVersionPolicies",
                column: "Platform",
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "UserDevices",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            migrationBuilder.Sql($@"
INSERT INTO `MobileAppVersionPolicies`
(`Platform`, `MinSupportedVersion`, `LatestVersion`, `RecommendFromVersion`, `Message`, `StoreUrl`, `IsActive`, `DateCreation`, `DateModification`, `IdAuteur`)
VALUES
('Android', '1.0.0', '1.0.0', '1.0.0', 'Une nouvelle version est disponible.', NULL, 0, '{now}', NULL, NULL),
('iOS', '1.0.0', '1.0.0', '1.0.0', 'Une nouvelle version est disponible.', NULL, 0, '{now}', NULL, NULL);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MobileAppVersionPolicies");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "UserDevices");
        }
    }
}
