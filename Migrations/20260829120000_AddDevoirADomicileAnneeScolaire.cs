using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Ajoute IdAnneeScolaire (obligatoire après backfill) sur DevoirsADomicile.
    /// </summary>
    public partial class AddDevoirADomicileAnneeScolaire : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @col_annee = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile' AND COLUMN_NAME = 'IdAnneeScolaire'
);
SET @sql = IF(@col_annee = 0,
    'ALTER TABLE `DevoirsADomicile` ADD COLUMN `IdAnneeScolaire` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
UPDATE DevoirsADomicile d
SET d.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
      AND a.DateDebut <= d.DatePublication
      AND a.DateFin >= d.DatePublication
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE d.IdAnneeScolaire IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE DevoirsADomicile d
SET d.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE d.IdAnneeScolaire IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE DevoirsADomicile
SET IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire FROM AnneeScolaires a ORDER BY a.IdAnneeScolaire DESC LIMIT 1
)
WHERE IdAnneeScolaire IS NULL
  AND EXISTS (SELECT 1 FROM AnneeScolaires LIMIT 1);
");

            migrationBuilder.Sql(@"
ALTER TABLE `DevoirsADomicile`
    MODIFY COLUMN `IdAnneeScolaire` int NOT NULL;
");

            migrationBuilder.Sql(@"
SET @fk_annee = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND COLUMN_NAME = 'IdAnneeScolaire' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_annee IS NULL,
    'ALTER TABLE `DevoirsADomicile` ADD CONSTRAINT `FK_DevoirsADomicile_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`) ON DELETE NO ACTION',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_IdAnneeScolaire'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_DevoirADomicile_IdAnneeScolaire` ON `DevoirsADomicile` (`IdAnneeScolaire`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_Classe_Annee_Statut'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_DevoirADomicile_Classe_Annee_Statut` ON `DevoirsADomicile` (`IdClasse`, `IdAnneeScolaire`, `Statut`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND COLUMN_NAME = 'IdAnneeScolaire' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk IS NOT NULL, CONCAT('ALTER TABLE `DevoirsADomicile` DROP FOREIGN KEY `', @fk, '`'), 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_Classe_Annee_Statut'
);
SET @sql = IF(@idx > 0, 'DROP INDEX `IX_DevoirADomicile_Classe_Annee_Statut` ON `DevoirsADomicile`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_IdAnneeScolaire'
);
SET @sql = IF(@idx > 0, 'DROP INDEX `IX_DevoirADomicile_IdAnneeScolaire` ON `DevoirsADomicile`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @col = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile' AND COLUMN_NAME = 'IdAnneeScolaire'
);
SET @sql = IF(@col > 0, 'ALTER TABLE `DevoirsADomicile` DROP COLUMN `IdAnneeScolaire`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
