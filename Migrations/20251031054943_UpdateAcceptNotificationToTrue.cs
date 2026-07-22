using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class UpdateAcceptNotificationToTrue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mettre à jour toutes les écoles existantes pour accepter les notifications SMS
            migrationBuilder.Sql("UPDATE Ecoles SET AcceptNotification = 1 WHERE AcceptNotification = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
