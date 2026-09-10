using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddPaymentCurrencySnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeDevisePaiement",
                table: "Paiements",
                type: "varchar(10)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CodeDevisePrincipale",
                table: "Paiements",
                type: "varchar(10)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MontantPayeDevisePrincipale",
                table: "Paiements",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxVersDevisePrincipale",
                table: "Paiements",
                type: "decimal(18,8)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeDevisePaiement",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "CodeDevisePrincipale",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "MontantPayeDevisePrincipale",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "TauxVersDevisePrincipale",
                table: "Paiements");
        }
    }
}
