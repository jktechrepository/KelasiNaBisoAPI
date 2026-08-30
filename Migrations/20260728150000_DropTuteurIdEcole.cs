using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Drop Tuteurs.IdEcole (école via Inscription des enfants uniquement).
    /// Idempotent pour environnements où la colonne a déjà été supprimée manuellement.
    /// </summary>
    public partial class DropTuteurIdEcole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk_name = (
    SELECT CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Tuteurs'
      AND COLUMN_NAME = 'IdEcole'
      AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_name IS NOT NULL,
    CONCAT('ALTER TABLE `Tuteurs` DROP FOREIGN KEY `', @fk_name, '`'),
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx_exists = (
    SELECT COUNT(1)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Tuteurs'
      AND INDEX_NAME = 'IX_Tuteurs_IdEcole'
);
SET @sql = IF(@idx_exists > 0,
    'DROP INDEX `IX_Tuteurs_IdEcole` ON `Tuteurs`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Index auto-généré EF parfois nommé FK_Tuteurs_Ecoles_IdEcole ou IX sans préfixe
            migrationBuilder.Sql(@"
SET @idx_exists = (
    SELECT COUNT(1)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Tuteurs'
      AND INDEX_NAME = 'IX_Tuteurs_Ecoles_IdEcole'
);
SET @sql = IF(@idx_exists > 0,
    'DROP INDEX `IX_Tuteurs_Ecoles_IdEcole` ON `Tuteurs`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @col_exists = (
    SELECT COUNT(1)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Tuteurs'
      AND COLUMN_NAME = 'IdEcole'
);
SET @sql = IF(@col_exists > 0,
    'ALTER TABLE `Tuteurs` DROP COLUMN `IdEcole`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Vues : école via inscription, jamais t.IdEcole
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `EleveParEcole`;");
            migrationBuilder.Sql(@"
CREATE VIEW `EleveParEcole` AS
SELECT
    e.IdEleve,
    e.ReferenceEleve,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletEleve,
    e.Genre,
    e.DateNaissance,
    YEAR(CURDATE()) - YEAR(e.DateNaissance) AS Age,
    e.LieuNaissance,
    e.PhotoUrl,
    e.Nationalite,
    e.Matricule,
    e.Province AS ProvinceEleve,
    e.Ville AS VilleEleve,
    e.Commune AS CommuneEleve,
    e.Quartier AS QuartierEleve,
    e.Avenue AS AvenueEleve,
    e.Numero AS NumeroEleve,
    e.Commentaire,
    e.Statut,
    ins.IdClasse AS IdClasse,
    c.NomClasse,
    d.IdDirection,
    d.NomDirection,
    o.IdOption,
    o.NomOption,
    t.IdTuteur,
    t.NomComplet AS NomCompletTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    t.Statut AS StatutTuteur,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    ins.IdEcole AS IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute,
    ec.Latitude,
    ec.Type,
    ec.Logo AS LogoUrl,
    ec.Telephone AS TelephoneEcole,
    ec.EmailContact,
    ec.SiteWeb,
    ec.ProvinceEducationnel,
    ec.NomCompletResponsable,
    ec.Description,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole
FROM Eleves e
LEFT JOIN (
    SELECT i.*
    FROM Inscriptions i
    INNER JOIN (
        SELECT IdEleve, MAX(DateInscription) AS MaxDate
        FROM Inscriptions
        WHERE Statut = 1
          AND (StatutInscription = 'Confirmé' OR StatutInscription = 'Confirme' OR StatutInscription LIKE 'Confirm%')
        GROUP BY IdEleve
    ) latest ON i.IdEleve = latest.IdEleve AND i.DateInscription = latest.MaxDate
    WHERE i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
) ins ON ins.IdEleve = e.IdEleve
LEFT JOIN Classes c ON ins.IdClasse = c.IdClasse
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole
WHERE e.SerialNumber IS NULL AND e.STATUT IS TRUE;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `V_Eleve`;");
            migrationBuilder.Sql(@"
CREATE VIEW `V_Eleve` AS
SELECT
    e.IdEleve,
    e.ReferenceEleve,
    e.Matricule,
    e.Nom,
    e.Postnom,
    e.Prenom,
    e.NomComplet,
    e.Genre,
    e.DateNaissance,
    e.LieuNaissance,
    e.PhotoUrl,
    e.Nationalite,
    e.Commentaire,
    e.Statut,
    e.DateCreation,
    e.Province,
    e.Ville,
    e.Commune,
    e.Quartier,
    e.Avenue,
    e.Numero,
    ins.IdClasse AS IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,
    t.IdTuteur,
    t.NomComplet AS NomCompletTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    t.Statut AS StatutTuteur,
    t.DateCreation AS DateCreationTuteur,
    ins.IdEcole AS IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan AS SloganEcole,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrlEcole,
    ec.Telephone AS TelephoneEcole,
    ec.EmailContact AS EmailContactEcole,
    ec.SiteWeb AS SiteWebEcole,
    ec.ProvinceEducationnel AS ProvinceEducationnel,
    ec.NomCompletResponsable AS NomCompletResponsable,
    ec.Description AS DescriptionEcole,
    ec.DateCreation AS DateCreationEcole,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole
FROM Eleves e
LEFT JOIN (
    SELECT i.*
    FROM Inscriptions i
    INNER JOIN (
        SELECT IdEleve, MAX(DateInscription) AS MaxDate
        FROM Inscriptions
        WHERE Statut = 1
          AND (StatutInscription = 'Confirmé' OR StatutInscription = 'Confirme' OR StatutInscription LIKE 'Confirm%')
        GROUP BY IdEleve
    ) latest ON i.IdEleve = latest.IdEleve AND i.DateInscription = latest.MaxDate
    WHERE i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
) ins ON ins.IdEleve = e.IdEleve
LEFT JOIN Classes c ON ins.IdClasse = c.IdClasse
LEFT JOIN Sections s ON c.IdSection = s.IdSection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @col_exists = (
    SELECT COUNT(1)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Tuteurs'
      AND COLUMN_NAME = 'IdEcole'
);
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE `Tuteurs` ADD COLUMN `IdEcole` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
