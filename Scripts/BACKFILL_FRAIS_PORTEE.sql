-- Backfill / alignement prod : portée Frais (IdEcole + FraisDirections / FraisClasses).
-- Préférer `dotnet ef database update` (migration 20260903120000_FraisPorteeDirectionsClasses).
-- Ce script est idempotent si la migration a déjà tourné (colonnes IdDirection/IdClasse absentes).

-- 1. Colonnes
SET @col = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdEcole');
SET @sql = IF(@col = 0, 'ALTER TABLE `Frais` ADD COLUMN `IdEcole` int NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'Portee');
SET @sql = IF(@col = 0, 'ALTER TABLE `Frais` ADD COLUMN `Portee` int NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 2. Backfill depuis l'ancienne FK direction (si encore présente)
SET @has_dir = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdDirection');
SET @sql = IF(@has_dir > 0,
    'UPDATE Frais f INNER JOIN Directions d ON d.IdDirection = f.IdDirection SET f.IdEcole = d.IdEcole WHERE f.IdEcole IS NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @has_classe = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdClasse');
SET @sql = IF(@has_classe > 0,
    'UPDATE Frais SET Portee = CASE WHEN IdClasse IS NOT NULL AND IdClasse > 0 THEN 2 ELSE 1 END WHERE Portee IS NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE Frais SET Portee = 1 WHERE Portee IS NULL;

CREATE TABLE IF NOT EXISTS `FraisDirections` (
    `IdFrais` int NOT NULL,
    `IdDirection` int NOT NULL,
    PRIMARY KEY (`IdFrais`, `IdDirection`),
    CONSTRAINT `FK_FraisDirections_Frais` FOREIGN KEY (`IdFrais`) REFERENCES `Frais` (`IdFrais`) ON DELETE CASCADE,
    CONSTRAINT `FK_FraisDirections_Directions` FOREIGN KEY (`IdDirection`) REFERENCES `Directions` (`IdDirection`) ON DELETE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `FraisClasses` (
    `IdFrais` int NOT NULL,
    `IdClasse` int NOT NULL,
    PRIMARY KEY (`IdFrais`, `IdClasse`),
    CONSTRAINT `FK_FraisClasses_Frais` FOREIGN KEY (`IdFrais`) REFERENCES `Frais` (`IdFrais`) ON DELETE CASCADE,
    CONSTRAINT `FK_FraisClasses_Classes` FOREIGN KEY (`IdClasse`) REFERENCES `Classes` (`IdClasse`) ON DELETE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SET @sql = IF(@has_dir > 0 AND @has_classe > 0,
    'INSERT IGNORE INTO FraisDirections (IdFrais, IdDirection) SELECT IdFrais, IdDirection FROM Frais WHERE IdDirection IS NOT NULL AND IdDirection > 0 AND (IdClasse IS NULL OR IdClasse = 0)',
    IF(@has_dir > 0,
        'INSERT IGNORE INTO FraisDirections (IdFrais, IdDirection) SELECT IdFrais, IdDirection FROM Frais WHERE IdDirection IS NOT NULL AND IdDirection > 0',
        'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(@has_classe > 0,
    'INSERT IGNORE INTO FraisClasses (IdFrais, IdClasse) SELECT IdFrais, IdClasse FROM Frais WHERE IdClasse IS NOT NULL AND IdClasse > 0',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

ALTER TABLE `Frais`
    MODIFY COLUMN `IdEcole` int NOT NULL,
    MODIFY COLUMN `Portee` int NOT NULL;

SET @fk = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdEcole' AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1);
SET @sql = IF(@fk IS NULL,
    'ALTER TABLE `Frais` ADD CONSTRAINT `FK_Frais_Ecoles_IdEcole` FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE NO ACTION',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx = (SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND INDEX_NAME = 'IX_Frais_Ecole_Annee_Libelle');
SET @sql = IF(@idx = 0,
    'CREATE INDEX `IX_Frais_Ecole_Annee_Libelle` ON `Frais` (`IdEcole`, `IdAnneeScolaire`, `LibelleFrais`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Drop anciennes FK / index / colonnes
SET @fk = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdClasse' AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1);
SET @sql = IF(@fk IS NOT NULL, CONCAT('ALTER TABLE `Frais` DROP FOREIGN KEY `', @fk, '`'), 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk = (
    SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais'
      AND COLUMN_NAME = 'IdDirection' AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1);
SET @sql = IF(@fk IS NOT NULL, CONCAT('ALTER TABLE `Frais` DROP FOREIGN KEY `', @fk, '`'), 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx = (SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND INDEX_NAME = 'IX_Frais_Direction_Annee_Libelle_Classe');
SET @sql = IF(@idx > 0, 'DROP INDEX `IX_Frais_Direction_Annee_Libelle_Classe` ON `Frais`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx = (SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND INDEX_NAME = 'IX_Frais_IdClasse');
SET @sql = IF(@idx > 0, 'DROP INDEX `IX_Frais_IdClasse` ON `Frais`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx = (SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND INDEX_NAME = 'IX_Frais_IdDirection');
SET @sql = IF(@idx > 0, 'DROP INDEX `IX_Frais_IdDirection` ON `Frais`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdClasse');
SET @sql = IF(@col > 0, 'ALTER TABLE `Frais` DROP COLUMN `IdClasse`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col = (SELECT COUNT(1) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Frais' AND COLUMN_NAME = 'IdDirection');
SET @sql = IF(@col > 0, 'ALTER TABLE `Frais` DROP COLUMN `IdDirection`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
