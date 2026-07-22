using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    public partial class AddMokoAfrikaPaymentEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Cours_IdCours",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Session",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "IdCours",
                table: "Notes",
                newName: "IdEvaluation");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_IdCours",
                table: "Notes",
                newName: "IX_Notes_IdEvaluation");

            migrationBuilder.AddColumn<decimal>(
                name: "MontantCollecte",
                table: "Paiements",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantNet",
                table: "Paiements",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperateurMobileMoney",
                table: "Paiements",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CoursIdCours",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Coefficient",
                table: "Evaluations",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<string>(
                name: "Periode",
                table: "Evaluations",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TitreEvaluation",
                table: "Evaluations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Eleves",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Postnom",
                table: "Eleves",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Eleves",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CoefficientDevoir",
                table: "DevoirsADomicile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EcolesInfoPaiementMobile",
                columns: table => new
                {
                    IdEcoleInfoPaiementMobile = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    MobileMoneyActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CarteActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DelaiReglementMinutes = table.Column<int>(type: "int", nullable: false),
                    PayoutAutomatique = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcolesInfoPaiementMobile", x => x.IdEcoleInfoPaiementMobile);
                    table.ForeignKey(
                        name: "FK_EcolesInfoPaiementMobile_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaiementsCrashed",
                columns: table => new
                {
                    IdPaiementCrashed = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DatePaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Montant = table.Column<double>(type: "double", nullable: true),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModePaiement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    StatutPaiement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceTransaction = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JustificatifUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commentaire = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdEleve = table.Column<int>(type: "int", nullable: true),
                    IdFrais = table.Column<int>(type: "int", nullable: true),
                    IdUtilisateur = table.Column<int>(type: "int", nullable: true),
                    NomCompletEleve = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LibelleFrais = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErreursJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroLigne = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    NomFichierOriginal = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEchec = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateCorrection = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateReinjection = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IdPaiementCree = table.Column<int>(type: "int", nullable: true),
                    EstResolu = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsCrashed", x => x.IdPaiementCrashed);
                    table.ForeignKey(
                        name: "FK_PaiementsCrashed_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole");
                    table.ForeignKey(
                        name: "FK_PaiementsCrashed_Eleves_IdEleve",
                        column: x => x.IdEleve,
                        principalTable: "Eleves",
                        principalColumn: "IdEleve");
                    table.ForeignKey(
                        name: "FK_PaiementsCrashed_Frais_IdFrais",
                        column: x => x.IdFrais,
                        principalTable: "Frais",
                        principalColumn: "IdFrais");
                    table.ForeignKey(
                        name: "FK_PaiementsCrashed_Paiements_IdPaiementCree",
                        column: x => x.IdPaiementCree,
                        principalTable: "Paiements",
                        principalColumn: "IdPaiement",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaiementsCrashed_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TransactionsMoko",
                columns: table => new
                {
                    IdTransactionMoko = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentReference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdPaiement = table.Column<int>(type: "int", nullable: true),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    AmountNet = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FraisCollecte = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FraisDecaissement = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Method = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayTransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawRequest = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawResponse = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawCallback = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionsMoko", x => x.IdTransactionMoko);
                    table.ForeignKey(
                        name: "FK_TransactionsMoko_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionsMoko_Paiements_IdPaiement",
                        column: x => x.IdPaiement,
                        principalTable: "Paiements",
                        principalColumn: "IdPaiement",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EcolesBeneficiairesMomo",
                columns: table => new
                {
                    IdEcoleBeneficiaireMomo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcoleInfoPaiementMobile = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    Methode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomTitulaire = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstPrincipal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcolesBeneficiairesMomo", x => x.IdEcoleBeneficiaireMomo);
                    table.ForeignKey(
                        name: "FK_EcolesBeneficiairesMomo_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EcolesBeneficiairesMomo_EcolesInfoPaiementMobile_IdEcoleInfo~",
                        column: x => x.IdEcoleInfoPaiementMobile,
                        principalTable: "EcolesInfoPaiementMobile",
                        principalColumn: "IdEcoleInfoPaiementMobile",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EcolesWallets",
                columns: table => new
                {
                    IdEcoleWallet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdEcoleInfoPaiementMobile = table.Column<int>(type: "int", nullable: false),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoldeEnAttente = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SoldeDisponible = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalRecu = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalReverse = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcolesWallets", x => x.IdEcoleWallet);
                    table.ForeignKey(
                        name: "FK_EcolesWallets_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EcolesWallets_EcolesInfoPaiementMobile_IdEcoleInfoPaiementMo~",
                        column: x => x.IdEcoleInfoPaiementMobile,
                        principalTable: "EcolesInfoPaiementMobile",
                        principalColumn: "IdEcoleInfoPaiementMobile",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FilePayoutsMoko",
                columns: table => new
                {
                    IdFilePayoutMoko = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PayInReference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdTransactionMokoPayIn = table.Column<int>(type: "int", nullable: true),
                    IdTransactionMokoPayOut = table.Column<int>(type: "int", nullable: true),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdPaiement = table.Column<int>(type: "int", nullable: true),
                    MontantNet = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Devise = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Methode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroBeneficiaire = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PayOutReference = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateTraitement = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePayoutsMoko", x => x.IdFilePayoutMoko);
                    table.ForeignKey(
                        name: "FK_FilePayoutsMoko_Ecoles_IdEcole",
                        column: x => x.IdEcole,
                        principalTable: "Ecoles",
                        principalColumn: "IdEcole",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilePayoutsMoko_Paiements_IdPaiement",
                        column: x => x.IdPaiement,
                        principalTable: "Paiements",
                        principalColumn: "IdPaiement",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FilePayoutsMoko_TransactionsMoko_IdTransactionMokoPayIn",
                        column: x => x.IdTransactionMokoPayIn,
                        principalTable: "TransactionsMoko",
                        principalColumn: "IdTransactionMoko",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FilePayoutsMoko_TransactionsMoko_IdTransactionMokoPayOut",
                        column: x => x.IdTransactionMokoPayOut,
                        principalTable: "TransactionsMoko",
                        principalColumn: "IdTransactionMoko",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EcolesWalletMouvements",
                columns: table => new
                {
                    IdEcoleWalletMouvement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdEcoleWallet = table.Column<int>(type: "int", nullable: false),
                    IdEcole = table.Column<int>(type: "int", nullable: false),
                    IdTransactionMoko = table.Column<int>(type: "int", nullable: true),
                    IdPaiement = table.Column<int>(type: "int", nullable: true),
                    TypeMouvement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SoldeEnAttenteApres = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SoldeDisponibleApres = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcolesWalletMouvements", x => x.IdEcoleWalletMouvement);
                    table.ForeignKey(
                        name: "FK_EcolesWalletMouvements_EcolesWallets_IdEcoleWallet",
                        column: x => x.IdEcoleWallet,
                        principalTable: "EcolesWallets",
                        principalColumn: "IdEcoleWallet",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EcolesWalletMouvements_Paiements_IdPaiement",
                        column: x => x.IdPaiement,
                        principalTable: "Paiements",
                        principalColumn: "IdPaiement",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EcolesWalletMouvements_TransactionsMoko_IdTransactionMoko",
                        column: x => x.IdTransactionMoko,
                        principalTable: "TransactionsMoko",
                        principalColumn: "IdTransactionMoko",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CoursIdCours",
                table: "Notes",
                column: "CoursIdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe",
                table: "Eleves",
                columns: new[] { "Nom", "Postnom", "Prenom", "DateNaissance", "IdTuteur", "IdClasse" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoleBeneficiaireMomo_Ecole_Methode_Numero",
                table: "EcolesBeneficiairesMomo",
                columns: new[] { "IdEcole", "Methode", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcolesBeneficiairesMomo_IdEcoleInfoPaiementMobile",
                table: "EcolesBeneficiairesMomo",
                column: "IdEcoleInfoPaiementMobile");

            migrationBuilder.CreateIndex(
                name: "IX_EcoleInfoPaiementMobile_IdEcole_Unique",
                table: "EcolesInfoPaiementMobile",
                column: "IdEcole",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcolesWalletMouvements_IdEcoleWallet",
                table: "EcolesWalletMouvements",
                column: "IdEcoleWallet");

            migrationBuilder.CreateIndex(
                name: "IX_EcolesWalletMouvements_IdPaiement",
                table: "EcolesWalletMouvements",
                column: "IdPaiement");

            migrationBuilder.CreateIndex(
                name: "IX_EcolesWalletMouvements_IdTransactionMoko",
                table: "EcolesWalletMouvements",
                column: "IdTransactionMoko");

            migrationBuilder.CreateIndex(
                name: "IX_EcoleWalletMouvement_IdEcole",
                table: "EcolesWalletMouvements",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_EcolesWallets_IdEcoleInfoPaiementMobile",
                table: "EcolesWallets",
                column: "IdEcoleInfoPaiementMobile",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoleWallet_IdEcole_Unique",
                table: "EcolesWallets",
                column: "IdEcole",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutMoko_PayInReference",
                table: "FilePayoutsMoko",
                column: "PayInReference");

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutMoko_Status_ScheduledAt",
                table: "FilePayoutsMoko",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutsMoko_IdEcole",
                table: "FilePayoutsMoko",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutsMoko_IdPaiement",
                table: "FilePayoutsMoko",
                column: "IdPaiement");

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutsMoko_IdTransactionMokoPayIn",
                table: "FilePayoutsMoko",
                column: "IdTransactionMokoPayIn");

            migrationBuilder.CreateIndex(
                name: "IX_FilePayoutsMoko_IdTransactionMokoPayOut",
                table: "FilePayoutsMoko",
                column: "IdTransactionMokoPayOut");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCrashed_IdEcole",
                table: "PaiementsCrashed",
                column: "IdEcole");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCrashed_IdEleve",
                table: "PaiementsCrashed",
                column: "IdEleve");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCrashed_IdFrais",
                table: "PaiementsCrashed",
                column: "IdFrais");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCrashed_IdPaiementCree",
                table: "PaiementsCrashed",
                column: "IdPaiementCree");

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsCrashed_IdUtilisateur",
                table: "PaiementsCrashed",
                column: "IdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMoko_IdPaiement",
                table: "TransactionsMoko",
                column: "IdPaiement");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMoko_ParentReference",
                table: "TransactionsMoko",
                column: "ParentReference");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMoko_Reference_Unique",
                table: "TransactionsMoko",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsMoko_IdEcole",
                table: "TransactionsMoko",
                column: "IdEcole");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Cours_CoursIdCours",
                table: "Notes",
                column: "CoursIdCours",
                principalTable: "Cours",
                principalColumn: "IdCours");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Evaluations_IdEvaluation",
                table: "Notes",
                column: "IdEvaluation",
                principalTable: "Evaluations",
                principalColumn: "IdEvaluation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Cours_CoursIdCours",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Evaluations_IdEvaluation",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "EcolesBeneficiairesMomo");

            migrationBuilder.DropTable(
                name: "EcolesWalletMouvements");

            migrationBuilder.DropTable(
                name: "FilePayoutsMoko");

            migrationBuilder.DropTable(
                name: "PaiementsCrashed");

            migrationBuilder.DropTable(
                name: "EcolesWallets");

            migrationBuilder.DropTable(
                name: "TransactionsMoko");

            migrationBuilder.DropTable(
                name: "EcolesInfoPaiementMobile");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CoursIdCours",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe",
                table: "Eleves");

            migrationBuilder.DropColumn(
                name: "MontantCollecte",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "MontantNet",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "OperateurMobileMoney",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "CoursIdCours",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Periode",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "TitreEvaluation",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "CoefficientDevoir",
                table: "DevoirsADomicile");

            migrationBuilder.RenameColumn(
                name: "IdEvaluation",
                table: "Notes",
                newName: "IdCours");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_IdEvaluation",
                table: "Notes",
                newName: "IX_Notes_IdCours");

            migrationBuilder.AddColumn<string>(
                name: "Session",
                table: "Notes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "Coefficient",
                table: "Evaluations",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Eleves",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Postnom",
                table: "Eleves",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Eleves",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Cours_IdCours",
                table: "Notes",
                column: "IdCours",
                principalTable: "Cours",
                principalColumn: "IdCours");
        }
    }
}
