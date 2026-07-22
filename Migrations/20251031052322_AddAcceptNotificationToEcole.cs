using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddAcceptNotificationToEcole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptNotification",
                table: "Ecoles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
            
            // Mettre à jour toutes les écoles existantes pour accepter les notifications
            migrationBuilder.Sql("UPDATE Ecoles SET AcceptNotification = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptNotification",
                table: "Ecoles");
        }
    }
}
