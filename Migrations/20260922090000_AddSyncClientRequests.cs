using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Étape 0 offline sync : journal d'idempotence SyncClientRequests.
    /// Migration manuelle (évite dotnet ef long / interrompu).
    /// </summary>
    [DbContext(typeof(KelasiNaBiso.Data.KelasiNaBisoDbContext))]
    [Migration("20260922090000_AddSyncClientRequests")]
    public partial class AddSyncClientRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncClientRequests",
                columns: table => new
                {
                    IdSyncClientRequest = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    ClientRequestId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResourceType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResourceId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncClientRequests", x => x.IdSyncClientRequest);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UX_SyncClientRequests_IdEcole_ClientRequestId",
                table: "SyncClientRequests",
                columns: new[] { "IdEcole", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncClientRequests_DateCreation",
                table: "SyncClientRequests",
                column: "DateCreation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncClientRequests");
        }
    }
}
