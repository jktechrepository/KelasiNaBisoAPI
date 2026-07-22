using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ecoles",
                columns: table => new
                {
                    IdEcole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slogan = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitute = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Logo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telephone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailContact = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SiteWeb = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProvinceEducationnel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomCompletResponsable = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GenreResponsable = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Province = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ville = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commune = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quartier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avenue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecoles", x => x.IdEcole);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    IdPermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categorie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.IdPermission);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Niveau = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRole);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    IdAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Matricule = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Postnom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Genre = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TelephoneAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAgent = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EtatCivil = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerialNumber = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fonction = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Province = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ville = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commune = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quartier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avenue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.IdAgent);
                    table.ForeignKey(
                        name: "FK_Agents_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AnneeScolaires",
                columns: table => new
                {
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LibelleAnneeScolaire = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnneeScolaires", x => x.IdAnneeScolaire);
                    table.ForeignKey(
                        name: "FK_AnneeScolaires_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    IdDirection = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomDirection = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    NiveauEnseignement = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.IdDirection);
                    table.ForeignKey(
                        name: "FK_Directions_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    IdSection = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomSection = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.IdSection);
                    table.ForeignKey(
                        name: "FK_Sections_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tuteurs",
                columns: table => new
                {
                    IdTuteur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomComplet = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Genre = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telephone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomCompletRepresentant = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelephoneRepresentant = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoTuteurUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PieceIdentiteTuteur = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SerialNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tuteurs", x => x.IdTuteur);
                    table.ForeignKey(
                        name: "FK_Tuteurs_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Vacations",
                columns: table => new
                {
                    IdVacation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomVacation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeureDebut = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HeureDebutPause = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HeureFinPause = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    NombreJoursParSemaine = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacations", x => x.IdVacation);
                    table.ForeignKey(
                        name: "FK_Vacations_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    IdRolePermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdRole = table.Column<int>(type: "int", nullable: false),
                    IdPermission = table.Column<int>(type: "int", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdUtilisateurAttribution = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.IdRolePermission);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_IdPermission",
                        column: x => x.IdPermission,
                        principalTable: "Permissions",
                        principalColumn: "IdPermission",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_IdRole",
                        column: x => x.IdRole,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    IdFrais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LibelleFrais = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<double>(type: "double", nullable: false),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeFrais = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Periodicite = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdDirection = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.IdFrais);
                    table.ForeignKey(
                        name: "FK_Frais_Directions_IdDirection",
                        column: x => x.IdDirection,
                        principalTable: "Directions",
                        principalColumn: "IdDirection");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    IdOption = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomOption = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdSection = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.IdOption);
                    table.ForeignKey(
                        name: "FK_Options_Sections_IdSection",
                        column: x => x.IdSection,
                        principalTable: "Sections",
                        principalColumn: "IdSection",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReferenceUtilisateur = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomUtilisateur = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostNomUtilisateur = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrenomUtilisateur = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telephone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LieuNaissance = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Genre = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotDePasseHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultUsername = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoitChangerMotDePasse = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdRole = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsConnecte = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdAgent = table.Column<int>(type: "int", nullable: true),
                    IdTuteur = table.Column<int>(type: "int", nullable: true),
                    Province = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ville = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commune = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quartier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avenue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Roles_IdRole",
                        column: x => x.IdRole,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Tuteurs_IdTuteur",
                        column: x => x.IdTuteur,
                        principalTable: "Tuteurs",
                        principalColumn: "IdTuteur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    IdClasse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomClasse = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdDirection = table.Column<int>(type: "int", nullable: true),
                    IdSection = table.Column<int>(type: "int", nullable: true),
                    IdOption = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.IdClasse);
                    table.ForeignKey(
                        name: "FK_Classes_Directions_IdDirection",
                        column: x => x.IdDirection,
                        principalTable: "Directions",
                        principalColumn: "IdDirection",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Options_IdOption",
                        column: x => x.IdOption,
                        principalTable: "Options",
                        principalColumn: "IdOption",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Sections_IdSection",
                        column: x => x.IdSection,
                        principalTable: "Sections",
                        principalColumn: "IdSection",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupeMessages",
                columns: table => new
                {
                    IdGroupe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomGroupe = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreePar = table.Column<int>(type: "int", nullable: true),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupeMessages", x => x.IdGroupe);
                    table.ForeignKey(
                        name: "FK_GroupeMessages_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupeMessages_Utilisateurs_CreePar",
                        column: x => x.CreePar,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SmsLogs",
                columns: table => new
                {
                    IdSmsLog = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroDestinataire = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "varchar(1600)", maxLength: 1600, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeNotification = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageSid = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageErreur = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeErreur = table.Column<int>(type: "int", nullable: true),
                    CoutUsd = table.Column<double>(type: "double", nullable: false),
                    CoutFc = table.Column<double>(type: "double", nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateEchec = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NombreSegments = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroExpediteur = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UtilisateurIdUtilisateur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsLogs", x => x.IdSmsLog);
                    table.ForeignKey(
                        name: "FK_SmsLogs_Utilisateurs_UtilisateurIdUtilisateur",
                        column: x => x.UtilisateurIdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    IdUserDevice = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    FcmToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModel = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OsVersion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultDevice = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateEnregistrement = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateDerniereUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.IdUserDevice);
                    table.ForeignKey(
                        name: "FK_UserDevices_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    IdUserPermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    IdPermission = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttribueParIdUtilisateur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.IdUserPermission);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_IdPermission",
                        column: x => x.IdPermission,
                        principalTable: "Permissions",
                        principalColumn: "IdPermission",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Utilisateurs_AttribueParIdUtilisateur",
                        column: x => x.AttribueParIdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_UserPermissions_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cours",
                columns: table => new
                {
                    IdCours = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomCours = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ponderation = table.Column<int>(type: "int", nullable: true),
                    IdClasse = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cours", x => x.IdCours);
                    table.ForeignKey(
                        name: "FK_Cours_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Eleves",
                columns: table => new
                {
                    IdEleve = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReferenceEleve = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Matricule = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Postnom = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenom = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomComplet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Genre = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LieuNaissance = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nationalite = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commentaire = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdClasse = table.Column<int>(type: "int", nullable: true),
                    IdTuteur = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Province = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ville = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commune = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quartier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avenue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eleves", x => x.IdEleve);
                    table.ForeignKey(
                        name: "FK_Eleves_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                    table.ForeignKey(
                        name: "FK_Eleves_Tuteurs_IdTuteur",
                        column: x => x.IdTuteur,
                        principalTable: "Tuteurs",
                        principalColumn: "IdTuteur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Horaires",
                columns: table => new
                {
                    IdHoraire = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Vacation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeureDebut = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HeureDebutPause = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HeureFinPause = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    IdClasse = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horaires", x => x.IdHoraire);
                    table.ForeignKey(
                        name: "FK_Horaires_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TitulairesClasses",
                columns: table => new
                {
                    IdTitulaireClasse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdAgent = table.Column<int>(type: "int", nullable: false),
                    IdClasse = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitulairesClasses", x => x.IdTitulaireClasse);
                    table.ForeignKey(
                        name: "FK_TitulairesClasses_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_TitulairesClasses_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire");
                    table.ForeignKey(
                        name: "FK_TitulairesClasses_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    IdMessage = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdExpediteur = table.Column<int>(type: "int", nullable: true),
                    IdDestinateur = table.Column<int>(type: "int", nullable: true),
                    IdGroupe = table.Column<int>(type: "int", nullable: true),
                    ContenuMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FichierUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.IdMessage);
                    table.ForeignKey(
                        name: "FK_Messages_GroupeMessages_IdGroupe",
                        column: x => x.IdGroupe,
                        principalTable: "GroupeMessages",
                        principalColumn: "IdGroupe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Utilisateurs_IdDestinateur",
                        column: x => x.IdDestinateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_Messages_Utilisateurs_IdExpediteur",
                        column: x => x.IdExpediteur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AffectationsCours",
                columns: table => new
                {
                    IdAffectationCours = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdAgent = table.Column<int>(type: "int", nullable: false),
                    IdCours = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    DateAffectation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateFinAffectation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationsCours", x => x.IdAffectationCours);
                    table.ForeignKey(
                        name: "FK_AffectationsCours_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_AffectationsCours_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire");
                    table.ForeignKey(
                        name: "FK_AffectationsCours_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    IdEvaluation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TypeEvaluation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Coefficient = table.Column<double>(type: "double", nullable: false),
                    IdCours = table.Column<int>(type: "int", nullable: false),
                    IdClasse = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.IdEvaluation);
                    table.ForeignKey(
                        name: "FK_Evaluations_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                    table.ForeignKey(
                        name: "FK_Evaluations_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RessourcePedagogiques",
                columns: table => new
                {
                    IdRessourcePedagogique = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TitreRessourcePedagogique = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormatRessourcePedagogique = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlRessourcePedagogique = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdCours = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RessourcePedagogiques", x => x.IdRessourcePedagogique);
                    table.ForeignKey(
                        name: "FK_RessourcePedagogiques_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    IdDocument = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    TypeDocument = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.IdDocument);
                    table.ForeignKey(
                        name: "FK_Documents_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_Documents_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inscriptions",
                columns: table => new
                {
                    IdInscription = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdClasse = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatutInscription = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscriptions", x => x.IdInscription);
                    table.ForeignKey(
                        name: "FK_Inscriptions_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire");
                    table.ForeignKey(
                        name: "FK_Inscriptions_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                    table.ForeignKey(
                        name: "FK_Inscriptions_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole");
                    table.ForeignKey(
                        name: "FK_Inscriptions_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    IdNote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Session = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NoteObtenue = table.Column<double>(type: "double", nullable: false),
                    Appreciation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEvaluation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdProfesseur = table.Column<int>(type: "int", nullable: false),
                    IdEleve = table.Column<int>(type: "int", nullable: false),
                    IdCours = table.Column<int>(type: "int", nullable: false),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.IdNote);
                    table.ForeignKey(
                        name: "FK_Notes_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire");
                    table.ForeignKey(
                        name: "FK_Notes_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                    table.ForeignKey(
                        name: "FK_Notes_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_Notes_Utilisateurs_IdProfesseur",
                        column: x => x.IdProfesseur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    IdNotification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Contenu = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeNotification = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstLue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LienAction = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IdExpediteur = table.Column<int>(type: "int", nullable: true),
                    IdDestinataire = table.Column<int>(type: "int", nullable: true),
                    IdEcole = table.Column<int>(type: "int", nullable: true),
                    IdClasse = table.Column<int>(type: "int", nullable: true),
                    IdEleve = table.Column<int>(type: "int", nullable: true),
                    IdAgent = table.Column<int>(type: "int", nullable: true),
                    IdCours = table.Column<int>(type: "int", nullable: true),
                    IdAnneeScolaire = table.Column<int>(type: "int", nullable: true),
                    AgentIdAgent = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.IdNotification);
                    table.ForeignKey(
                        name: "FK_Notifications_Agents_AgentIdAgent",
                        column: x => x.AgentIdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_Notifications_AnneeScolaires_IdAnneeScolaire",
                        column: x => x.IdAnneeScolaire,
                        principalTable: "AnneeScolaires",
                        principalColumn: "IdAnneeScolaire");
                    table.ForeignKey(
                        name: "FK_Notifications_Classes_IdClasse",
                        column: x => x.IdClasse,
                        principalTable: "Classes",
                        principalColumn: "IdClasse");
                    table.ForeignKey(
                        name: "FK_Notifications_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                    table.ForeignKey(
                        name: "FK_Notifications_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole");
                    table.ForeignKey(
                        name: "FK_Notifications_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_Notifications_Utilisateurs_IdDestinataire",
                        column: x => x.IdDestinataire,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_Notifications_Utilisateurs_IdExpediteur",
                        column: x => x.IdExpediteur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    IdPaiement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DatePaiement = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Montant = table.Column<double>(type: "double", nullable: false),
                    Devise = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModePaiement = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatutPaiement = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceTransaction = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JustificatifUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commentaire = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEnregistrement = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReferencePaiemenet = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdFrais = table.Column<int>(type: "int", nullable: true),
                    IdEleve = table.Column<int>(type: "int", nullable: true),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.IdPaiement);
                    table.ForeignKey(
                        name: "FK_Paiements_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_Paiements_Frais_IdFrais",
                        column: x => x.IdFrais,
                        principalTable: "Frais",
                        principalColumn: "IdFrais");
                    table.ForeignKey(
                        name: "FK_Paiements_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Presences",
                columns: table => new
                {
                    IdPresence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEleve = table.Column<int>(type: "int", nullable: true),
                    IdAgent = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPresent = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    TypePresence = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeureArrivee = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HeureDepart = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    DateDuJour = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitute = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdVacation = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HoraireIdHoraire = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presences", x => x.IdPresence);
                    table.ForeignKey(
                        name: "FK_Presences_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_Presences_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_Presences_Horaires_HoraireIdHoraire",
                        column: x => x.HoraireIdHoraire,
                        principalTable: "Horaires",
                        principalColumn: "IdHoraire");
                    table.ForeignKey(
                        name: "FK_Presences_Vacations_IdVacation",
                        column: x => x.IdVacation,
                        principalTable: "Vacations",
                        principalColumn: "IdVacation");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsCours_IdAgent",
                table: "AffectationsCours",
                column: "IdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsCours_IdAnneeScolaire",
                table: "AffectationsCours",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsCours_IdCours",
                table: "AffectationsCours",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Email_Unique",
                table: "Agents",
                column: "EmailAgent",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_IdEcole",
                table: "Agents",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Matricule_Unique",
                table: "Agents",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_SerialNumber_Unique",
                table: "Agents",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnneeScolaires_IdEcole",
                table: "AnneeScolaires",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_IdDirection",
                table: "Classes",
                column: "IdDirection");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_IdOption",
                table: "Classes",
                column: "IdOption");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_IdSection",
                table: "Classes",
                column: "IdSection");

            migrationBuilder.CreateIndex(
                name: "IX_Cours_IdClasse",
                table: "Cours",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Directions_IdEcole",
                table: "Directions",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Directions_NomDirection_IdEcole",
                table: "Directions",
                columns: new[] { "NomDirection", "IdEcole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IdEleve",
                table: "Documents",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IdUtilisateur",
                table: "Documents",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdClasse",
                table: "Eleves",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_IdTuteur",
                table: "Eleves",
                column: "IdTuteur");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_Matricule_Unique",
                table: "Eleves",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_SerialNumber_Unique",
                table: "Eleves",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_IdClasse",
                table: "Evaluations",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_IdCours",
                table: "Evaluations",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_IdDirection",
                table: "Frais",
                column: "IdDirection");

            migrationBuilder.CreateIndex(
                name: "IX_GroupeMessages_CreePar",
                table: "GroupeMessages",
                column: "CreePar");

            migrationBuilder.CreateIndex(
                name: "IX_GroupeMessages_IdEcole",
                table: "GroupeMessages",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Horaires_IdClasse",
                table: "Horaires",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdAnneeScolaire",
                table: "Inscriptions",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdClasse",
                table: "Inscriptions",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdEcole",
                table: "Inscriptions",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IdEleve",
                table: "Inscriptions",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdDestinateur",
                table: "Messages",
                column: "IdDestinateur");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdExpediteur",
                table: "Messages",
                column: "IdExpediteur");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdGroupe",
                table: "Messages",
                column: "IdGroupe");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdAnneeScolaire",
                table: "Notes",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdCours",
                table: "Notes",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdEleve",
                table: "Notes",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdProfesseur",
                table: "Notes",
                column: "IdProfesseur");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AgentIdAgent",
                table: "Notifications",
                column: "AgentIdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdAnneeScolaire",
                table: "Notifications",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdClasse",
                table: "Notifications",
                column: "IdClasse");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdCours",
                table: "Notifications",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdDestinataire",
                table: "Notifications",
                column: "IdDestinataire");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdEcole",
                table: "Notifications",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdEleve",
                table: "Notifications",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdExpediteur",
                table: "Notifications",
                column: "IdExpediteur");

            migrationBuilder.CreateIndex(
                name: "IX_Options_IdSection",
                table: "Options",
                column: "IdSection");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_IdEleve",
                table: "Paiements",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_IdFrais",
                table: "Paiements",
                column: "IdFrais");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_IdUtilisateur",
                table: "Paiements",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_HoraireIdHoraire",
                table: "Presences",
                column: "HoraireIdHoraire");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_IdAgent",
                table: "Presences",
                column: "IdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_IdEleve",
                table: "Presences",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_IdVacation",
                table: "Presences",
                column: "IdVacation");

            migrationBuilder.CreateIndex(
                name: "IX_RessourcePedagogiques_IdCours",
                table: "RessourcePedagogiques",
                column: "IdCours");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_IdPermission",
                table: "RolePermissions",
                column: "IdPermission");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_IdRole",
                table: "RolePermissions",
                column: "IdRole");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nom",
                table: "Roles",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_IdEcole",
                table: "Sections",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_UtilisateurIdUtilisateur",
                table: "SmsLogs",
                column: "UtilisateurIdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_TitulairesClasses_IdAgent",
                table: "TitulairesClasses",
                column: "IdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_TitulairesClasses_IdAnneeScolaire",
                table: "TitulairesClasses",
                column: "IdAnneeScolaire");

            migrationBuilder.CreateIndex(
                name: "IX_TitulairesClasses_IdClasse_IdAnneeScolaire_Statut",
                table: "TitulairesClasses",
                columns: new[] { "IdClasse", "IdAnneeScolaire", "Statut" },
                unique: true,
                filter: "[Statut] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tuteurs_Email_Unique",
                table: "Tuteurs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tuteurs_IdEcole",
                table: "Tuteurs",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_IdUtilisateur",
                table: "UserDevices",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_AttribueParIdUtilisateur",
                table: "UserPermissions",
                column: "AttribueParIdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_IdPermission",
                table: "UserPermissions",
                column: "IdPermission");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_IdUtilisateur",
                table: "UserPermissions",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email_Unique",
                table: "Utilisateurs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_IdAgent",
                table: "Utilisateurs",
                column: "IdAgent");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_IdEcole",
                table: "Utilisateurs",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_IdRole",
                table: "Utilisateurs",
                column: "IdRole");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_IdTuteur",
                table: "Utilisateurs",
                column: "IdTuteur");

            migrationBuilder.CreateIndex(
                name: "IX_Vacations_IdEcole",
                table: "Vacations",
                column: "IdEcole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffectationsCours");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "Inscriptions");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Presences");

            migrationBuilder.DropTable(
                name: "RessourcePedagogiques");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SmsLogs");

            migrationBuilder.DropTable(
                name: "TitulairesClasses");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "GroupeMessages");

            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "Eleves");

            migrationBuilder.DropTable(
                name: "Horaires");

            migrationBuilder.DropTable(
                name: "Vacations");

            migrationBuilder.DropTable(
                name: "Cours");

            migrationBuilder.DropTable(
                name: "AnneeScolaires");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Tuteurs");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Ecoles");
        }
    }
}
