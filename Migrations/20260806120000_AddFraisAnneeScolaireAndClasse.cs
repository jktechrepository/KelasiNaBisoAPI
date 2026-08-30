using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Ajoute IdAnneeScolaire (obligatoire après backfill) et IdClasse (optionnel) sur Frais.
    /// </summary>
    public partial class AddFraisAnneeScolaireAndClasse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @col_annee = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdAnneeScolaire'
);
SET @sql = IF(@col_annee = 0,
    'ALTER TABLE `Frais` ADD COLUMN `IdAnneeScolaire` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @col_classe = (
    SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdClasse'
);
SET @sql = IF(@col_classe = 0,
    'ALTER TABLE `Frais` ADD COLUMN `IdClasse` int NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Backfill: année courante de l'école de la direction, sinon année la plus récente active
            migrationBuilder.Sql(@"
UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
      AND a.DateDebut <= NOW()
      AND a.DateFin >= NOW()
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;
");

            // Fallback last resort: any année scolaire in DB
            migrationBuilder.Sql(@"
UPDATE Frais
SET IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire FROM AnneeScolaires a ORDER BY a.IdAnneeScolaire DESC LIMIT 1
)
WHERE IdAnneeScolaire IS NULL
  AND EXISTS (SELECT 1 FROM AnneeScolaires LIMIT 1);
");

            migrationBuilder.Sql(@"
ALTER TABLE `Frais`
    MODIFY COLUMN `IdAnneeScolaire` int NOT NULL;
");

            migrationBuilder.Sql(@"
SET @fk_annee = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdAnneeScolaire' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_annee IS NULL,
    'ALTER TABLE `Frais` ADD CONSTRAINT `FK_Frais_AnneeScolaires_IdAnneeScolaire` FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `AnneeScolaires` (`IdAnneeScolaire`) ON DELETE NO ACTION',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @fk_classe = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdClasse' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_classe IS NULL,
    'ALTER TABLE `Frais` ADD CONSTRAINT `FK_Frais_Classes_IdClasse` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`) ON DELETE NO ACTION',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND INDEX_NAME = 'IX_Frais_IdAnneeScolaire'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_Frais_IdAnneeScolaire` ON `Frais` (`IdAnneeScolaire`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND INDEX_NAME = 'IX_Frais_IdClasse'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_Frais_IdClasse` ON `Frais` (`IdClasse`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND INDEX_NAME = 'IX_Frais_Direction_Annee_Libelle_Classe'
);
SET @sql = IF(@idx = 0,
    'CREATE INDEX `IX_Frais_Direction_Annee_Libelle_Classe` ON `Frais` (`IdDirection`, `IdAnneeScolaire`, `LibelleFrais`, `IdClasse`)',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @fk_annee = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdAnneeScolaire' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_annee IS NOT NULL,
    CONCAT('ALTER TABLE `Frais` DROP FOREIGN KEY `', @fk_annee, '`'),
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @fk_classe = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdClasse' AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql = IF(@fk_classe IS NOT NULL,
    CONCAT('ALTER TABLE `Frais` DROP FOREIGN KEY `', @fk_classe, '`'),
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql("ALTER TABLE `Frais` DROP COLUMN IF EXISTS `IdAnneeScolaire`;");
            migrationBuilder.Sql("ALTER TABLE `Frais` DROP COLUMN IF EXISTS `IdClasse`;");
        }
    }
}
