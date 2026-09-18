-- =============================================================================
-- KelasiNaBiso — Nettoyage colonne fantôme Notes.CoursIdCours
-- Fichier : docs/sql/20260917_DropNotesCoursIdCours.sql
-- SGBD    : MySQL 8 / MariaDB
-- =============================================================================
-- Contexte : EF créait une shadow FK CoursIdCours via Cours.Notes (obsolète).
-- Les notes passent par IdEvaluation. En prod la colonne n'existe souvent pas
-- (c'est le bug). Ce script ne fait rien de dangereux si la colonne est absente.
-- =============================================================================

SET NAMES utf8mb4;

-- Drop FK si présente (noms possibles selon migrations)
SET @fk := (
  SELECT CONSTRAINT_NAME
  FROM information_schema.TABLE_CONSTRAINTS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Notes'
    AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    AND CONSTRAINT_NAME IN ('FK_Notes_Cours_CoursIdCours', 'FK_Notes_Cours_CoursIdCours1')
  LIMIT 1
);
SET @sql := IF(@fk IS NOT NULL,
  CONCAT('ALTER TABLE `Notes` DROP FOREIGN KEY `', @fk, '`'),
  'SELECT ''FK CoursIdCours absente'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Drop index si présent
SET @idx := (
  SELECT INDEX_NAME
  FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Notes'
    AND INDEX_NAME = 'IX_Notes_CoursIdCours'
  LIMIT 1
);
SET @sql := IF(@idx IS NOT NULL,
  'ALTER TABLE `Notes` DROP INDEX `IX_Notes_CoursIdCours`',
  'SELECT ''Index IX_Notes_CoursIdCours absent'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Drop colonne si présente
SET @col := (
  SELECT COLUMN_NAME
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Notes'
    AND COLUMN_NAME = 'CoursIdCours'
  LIMIT 1
);
SET @sql := IF(@col IS NOT NULL,
  'ALTER TABLE `Notes` DROP COLUMN `CoursIdCours`',
  'SELECT ''Colonne CoursIdCours absente (OK pour fix code)'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT COLUMN_NAME
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Notes'
ORDER BY ORDINAL_POSITION;
