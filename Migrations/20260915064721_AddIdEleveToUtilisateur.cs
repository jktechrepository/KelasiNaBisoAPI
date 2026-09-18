using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Ajoute Utilisateur.IdEleve (nullable) + index unique + FK Eleves.
    /// </summary>
    public partial class AddIdEleveToUtilisateur : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdEleve",
                table: "Utilisateurs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_IdEleve_Unique",
                table: "Utilisateurs",
                column: "IdEleve",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Eleves_IdEleve",
                table: "Utilisateurs",
                column: "IdEleve",
                principalTable: "Eleves",
                principalColumn: "IdEleve",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Eleves_IdEleve",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_IdEleve_Unique",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "IdEleve",
                table: "Utilisateurs");
        }
    }
}
