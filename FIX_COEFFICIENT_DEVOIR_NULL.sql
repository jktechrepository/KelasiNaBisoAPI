-- ============================================================
-- FIX : Correction des valeurs NULL pour CoefficientDevoir
-- Date : 2024-12-04
-- Description : 
--   1. Vérifie si la colonne CoefficientDevoir existe
--   2. Si elle existe et contient des NULL, les remplace par 1
--   3. Si elle n'existe pas, la crée avec NOT NULL DEFAULT 1
-- ============================================================

-- Pour MySQL/MariaDB
USE `dev-knb_db`;

-- ============================================================
-- 1. Vérifier si la colonne existe
-- ============================================================

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME = 'CoefficientDevoir';

-- ============================================================
-- 2. Si la colonne n'existe pas, la créer
-- ============================================================

SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'DevoirsADomicile'
      AND COLUMN_NAME = 'CoefficientDevoir'
);

SET @sql = IF(@col_exists = 0,
    'ALTER TABLE `DevoirsADomicile` ADD COLUMN `CoefficientDevoir` INT NOT NULL DEFAULT 1 AFTER `IdCours`',
    'SELECT "La colonne CoefficientDevoir existe déjà" AS Message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- 3. Corriger les valeurs NULL existantes
-- ============================================================

UPDATE `DevoirsADomicile`
SET `CoefficientDevoir` = 1
WHERE `CoefficientDevoir` IS NULL OR `CoefficientDevoir` = 0;

-- ============================================================
-- 4. Vérification finale
-- ============================================================

SELECT 
    COUNT(*) AS TotalDevoirs,
    SUM(CASE WHEN CoefficientDevoir IS NULL THEN 1 ELSE 0 END) AS AvecNULL,
    SUM(CASE WHEN CoefficientDevoir = 0 THEN 1 ELSE 0 END) AS AvecZero,
    SUM(CASE WHEN CoefficientDevoir > 0 THEN 1 ELSE 0 END) AS Valides
FROM `DevoirsADomicile`;

SELECT '✅ Correction terminée' AS Status;

