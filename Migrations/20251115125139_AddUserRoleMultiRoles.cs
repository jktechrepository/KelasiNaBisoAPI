using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddUserRoleMultiRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceUtilisateur",
                table: "Utilisateurs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Longitute",
                table: "Presences",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Presences",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "IdVacation",
                table: "Presences",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "StatutPaiement",
                table: "Paiements",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceTransaction",
                table: "Paiements",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "JustificatifUrl",
                table: "Paiements",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Commentaire",
                table: "Paiements",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CanalUtilise",
                table: "Notifications",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdCampaign",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadJson",
                table: "Notifications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Priorite",
                table: "Notifications",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StatutEnvoi",
                table: "Notifications",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TrackingId",
                table: "Notifications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceEleve",
                table: "Eleves",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Postnom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "Agents",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EtatCivil",
                table: "Agents",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationCampaigns",
                columns: table => new
                {
                    IdCampaign = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdAuteur = table.Column<int>(type: "int", nullable: false),
                    Titre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContenuMarkdown = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Importance = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChannelsJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PlanifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RappelAuto = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ValidationAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ValidatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationCampaigns", x => x.IdCampaign);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaigns_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaigns_Utilisateurs_IdAuteur",
                        column: x => x.IdAuteur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationCampaigns_Utilisateurs_ValidatedBy",
                        column: x => x.ValidatedBy,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationTemplates",
                columns: table => new
                {
                    IdTemplate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Langue = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Titre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContenuMarkdown = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaceholdersAutorises = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateMaj = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTemplates", x => x.IdTemplate);
                    table.ForeignKey(
                        name: "FK_CommunicationTemplates_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommunicationTemplates_Utilisateurs_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationTemplates_Utilisateurs_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParentCommunicationPreferences",
                columns: table => new
                {
                    IdPreference = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdTuteur = table.Column<int>(type: "int", nullable: false),
                    Canal = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptIn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAcceptation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateRefus = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Source = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentCommunicationPreferences", x => x.IdPreference);
                    table.ForeignKey(
                        name: "FK_ParentCommunicationPreferences_Tuteurs_IdTuteur",
                        column: x => x.IdTuteur,
                        principalTable: "Tuteurs",
                        principalColumn: "IdTuteur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    IdPasswordResetToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.IdPasswordResetToken);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    IdUserRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    IdRole = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdUtilisateurAttribution = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.IdUserRole);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_IdRole",
                        column: x => x.IdRole,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CampaignRecipients",
                columns: table => new
                {
                    IdRecipient = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdCampaign = table.Column<int>(type: "int", nullable: false),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    IdTuteur = table.Column<int>(type: "int", nullable: true),
                    IdEleve = table.Column<int>(type: "int", nullable: true),
                    IdNotification = table.Column<int>(type: "int", nullable: true),
                    PreferredChannel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinalChannel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatePlanifie = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRecipients", x => x.IdRecipient);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_CommunicationCampaigns_IdCampaign",
                        column: x => x.IdCampaign,
                        principalTable: "CommunicationCampaigns",
                        principalColumn: "IdCampaign",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Notifications_IdNotification",
                        column: x => x.IdNotification,
                        principalTable: "Notifications",
                        principalColumn: "IdNotification",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Tuteurs_IdTuteur",
                        column: x => x.IdTuteur,
                        principalTable: "Tuteurs",
                        principalColumn: "IdTuteur",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignRecipients_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationHistory",
                columns: table => new
                {
                    IdHistory = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdCampaign = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateAction = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AdresseIP = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationHistory", x => x.IdHistory);
                    table.ForeignKey(
                        name: "FK_CommunicationHistory_CommunicationCampaigns_IdCampaign",
                        column: x => x.IdCampaign,
                        principalTable: "CommunicationCampaigns",
                        principalColumn: "IdCampaign",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationHistory_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommunicationSegments",
                columns: table => new
                {
                    IdSegment = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdCampaign = table.Column<int>(type: "int", nullable: true),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    NomSegment = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeSegment = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriteriaJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsReusable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationSegments", x => x.IdSegment);
                    table.ForeignKey(
                        name: "FK_CommunicationSegments_CommunicationCampaigns_IdCampaign",
                        column: x => x.IdCampaign,
                        principalTable: "CommunicationCampaigns",
                        principalColumn: "IdCampaign",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationSegments_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommunicationSegments_Utilisateurs_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdCampaign",
                table: "Notifications",
                column: "IdCampaign");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_IdCampaign",
                table: "CampaignRecipients",
                column: "IdCampaign");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_IdEleve",
                table: "CampaignRecipients",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_IdNotification",
                table: "CampaignRecipients",
                column: "IdNotification");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_IdTuteur",
                table: "CampaignRecipients",
                column: "IdTuteur");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_IdUtilisateur",
                table: "CampaignRecipients",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRecipients_Status",
                table: "CampaignRecipients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaigns_IdAuteur",
                table: "CommunicationCampaigns",
                column: "IdAuteur");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaigns_IdEcole_Statut",
                table: "CommunicationCampaigns",
                columns: new[] { "IdEcole", "Statut" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationCampaigns_ValidatedBy",
                table: "CommunicationCampaigns",
                column: "ValidatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationHistory_IdCampaign_Action",
                table: "CommunicationHistory",
                columns: new[] { "IdCampaign", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationHistory_UserId",
                table: "CommunicationHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationSegments_CreatedBy",
                table: "CommunicationSegments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationSegments_IdCampaign",
                table: "CommunicationSegments",
                column: "IdCampaign");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationSegments_IdEcole",
                table: "CommunicationSegments",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_CreatedBy",
                table: "CommunicationTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_IdEcole_IsActif",
                table: "CommunicationTemplates",
                columns: new[] { "IdEcole", "IsActif" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_UpdatedBy",
                table: "CommunicationTemplates",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ParentCommunicationPreferences_IdTuteur_Canal",
                table: "ParentCommunicationPreferences",
                columns: new[] { "IdTuteur", "Canal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_IdUtilisateur",
                table: "PasswordResetTokens",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_IdRole",
                table: "UserRoles",
                column: "IdRole");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_IdUtilisateur",
                table: "UserRoles",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Utilisateur_Role_Unique",
                table: "UserRoles",
                columns: new[] { "IdUtilisateur", "IdRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Utilisateur_Statut",
                table: "UserRoles",
                columns: new[] { "IdUtilisateur", "Statut" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CommunicationCampaigns_IdCampaign",
                table: "Notifications",
                column: "IdCampaign",
                principalTable: "CommunicationCampaigns",
                principalColumn: "IdCampaign");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CommunicationCampaigns_IdCampaign",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "CampaignRecipients");

            migrationBuilder.DropTable(
                name: "CommunicationHistory");

            migrationBuilder.DropTable(
                name: "CommunicationSegments");

            migrationBuilder.DropTable(
                name: "CommunicationTemplates");

            migrationBuilder.DropTable(
                name: "ParentCommunicationPreferences");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "CommunicationCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IdCampaign",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CanalUtilise",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IdCampaign",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PayloadJson",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priorite",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "StatutEnvoi",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TrackingId",
                table: "Notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceUtilisateur",
                table: "Utilisateurs",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Presences",
                keyColumn: "Longitute",
                keyValue: null,
                column: "Longitute",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Longitute",
                table: "Presences",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Presences",
                keyColumn: "Latitude",
                keyValue: null,
                column: "Latitude",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Presences",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "IdVacation",
                table: "Presences",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Paiements",
                keyColumn: "StatutPaiement",
                keyValue: null,
                column: "StatutPaiement",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "StatutPaiement",
                table: "Paiements",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Paiements",
                keyColumn: "ReferenceTransaction",
                keyValue: null,
                column: "ReferenceTransaction",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceTransaction",
                table: "Paiements",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Paiements",
                keyColumn: "JustificatifUrl",
                keyValue: null,
                column: "JustificatifUrl",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "JustificatifUrl",
                table: "Paiements",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Paiements",
                keyColumn: "Commentaire",
                keyValue: null,
                column: "Commentaire",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Commentaire",
                table: "Paiements",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferenceEleve",
                table: "Eleves",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Agents",
                keyColumn: "Prenom",
                keyValue: null,
                column: "Prenom",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Agents",
                keyColumn: "Postnom",
                keyValue: null,
                column: "Postnom",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Postnom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Agents",
                keyColumn: "Nom",
                keyValue: null,
                column: "Nom",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Agents",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Agents",
                keyColumn: "Genre",
                keyValue: null,
                column: "Genre",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "Agents",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Agents",
                keyColumn: "EtatCivil",
                keyValue: null,
                column: "EtatCivil",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EtatCivil",
                table: "Agents",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
