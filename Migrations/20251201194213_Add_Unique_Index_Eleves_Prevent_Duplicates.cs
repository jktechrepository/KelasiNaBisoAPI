using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class Add_Unique_Index_Eleves_Prevent_Duplicates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // ✅ INDEX UNIQUE COMPOSITE : Prévention des doublons d'élèves
            // ═══════════════════════════════════════════════════════════════════
            // 
            // Cet index unique empêche la création de doublons basés sur :
            // - Nom + Postnom + Prenom + DateNaissance + IdTuteur + IdClasse
            // 
            // Uniquement pour les élèves actifs (Statut = 1)
            // 
            // IMPORTANT : Avant d'appliquer cette migration en production,
            // nettoyer les doublons existants avec le script SQL fourni.
            // ═══════════════════════════════════════════════════════════════════

            // Note : MariaDB/MySQL ne supporte pas les index filtrés avec WHERE
            // Le filtre par Statut = 1 est géré dans le code C#
            migrationBuilder.CreateIndex(
                name: "IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe",
                table: "Eleves",
                columns: new[] { "Nom", "Postnom", "Prenom", "DateNaissance", "IdTuteur", "IdClasse" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer l'index unique
            migrationBuilder.DropIndex(
                name: "IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe",
                table: "Eleves");
        }
    }
}
