-- =============================================================================
-- KelasiNaBiso — Migration Notes : IdCours → IdEvaluation (MySQL / phpMyAdmin)
-- Fichier : docs/sql/20260917_MigrateNotes_IdCours_To_IdEvaluation.sql
-- Date    : 2026-09-17
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
--
-- Corrige : Unknown column 'n.IdEvaluation' in 'SELECT'
--
-- Prérequis :
--   1) BACKUP de la base avant exécution
--   2) Fenêtre de maintenance recommandée
--
-- Idempotent : ré-exécutable si IdEvaluation existe déjà.
-- Ne crée JAMAIS la colonne fantôme CoursIdCours.
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- 0. Diagnostic
-- -----------------------------------------------------------------------------

SELECT
  SUM(COLUMN_NAME = 'IdCours') AS HasIdCours,
  SUM(COLUMN_NAME = 'IdEvaluation') AS HasIdEvaluation,
  SUM(COLUMN_NAME = 'Session') AS HasSession,
  SUM(COLUMN_NAME = 'CoursIdCours') AS HasCoursIdCours
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Notes';

SELECT COUNT(*) AS NbNotes FROM `Notes`;

-- -----------------------------------------------------------------------------
-- 1. Si IdEvaluation absent et IdCours présent : migration données + schéma
-- -----------------------------------------------------------------------------

-- 1.a Colonnes optionnelles Evaluations (alignement modèle API)
SET @col := (
  SELECT COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Evaluations' AND COLUMN_NAME = 'TitreEvaluation'
  LIMIT 1
);
SET @sql := IF(@col IS NULL,
  'ALTER TABLE `Evaluations` ADD COLUMN `TitreEvaluation` LONGTEXT NULL',
  'SELECT ''Evaluations.TitreEvaluation déjà présent'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col := (
  SELECT COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Evaluations' AND COLUMN_NAME = 'Periode'
  LIMIT 1
);
SET @sql := IF(@col IS NULL,
  'ALTER TABLE `Evaluations` ADD COLUMN `Periode` VARCHAR(255) NULL',
  'SELECT ''Evaluations.Periode déjà présent'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.b Ajouter IdEvaluation (nullable) seulement si absent
SET @hasEval := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND COLUMN_NAME = 'IdEvaluation'
);
SET @hasCours := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND COLUMN_NAME = 'IdCours'
);

SET @sql := IF(@hasEval = 0,
  'ALTER TABLE `Notes` ADD COLUMN `IdEvaluation` INT NULL',
  'SELECT ''Notes.IdEvaluation déjà présent — skip ADD'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.c Créer évaluations génériques manquantes (uniquement si IdCours existe encore)
--     pour chaque couple (IdCours, IdClasse) utiliséé par des notes sans IdEvaluation
SET @sql := IF(@hasCours > 0,
  'INSERT INTO `Evaluations` (`TypeEvaluation`, `TitreEvaluation`, `Coefficient`, `IdCours`, `IdClasse`, `Statut`, `DateCreation`)
   SELECT DISTINCT
     ''Note Generique'',
     ''Évaluation générée pour migration IdCours→IdEvaluation'',
     1.0,
     n.`IdCours`,
     c.`IdClasse`,
     1,
     UTC_TIMESTAMP(6)
   FROM `Notes` n
   INNER JOIN `Cours` c ON c.`IdCours` = n.`IdCours`
   WHERE n.`IdCours` IS NOT NULL AND n.`IdCours` > 0
     AND c.`IdClasse` IS NOT NULL
     AND (n.`IdEvaluation` IS NULL)
     AND NOT EXISTS (
       SELECT 1 FROM `Evaluations` e
       WHERE e.`IdCours` = n.`IdCours`
         AND e.`IdClasse` = c.`IdClasse`
     )',
  'SELECT ''Pas de colonne IdCours — skip création évaluations'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.d Backfill IdEvaluation depuis une évaluation du même IdCours
SET @sql := IF(@hasCours > 0,
  'UPDATE `Notes` n
   SET n.`IdEvaluation` = (
     SELECT e.`IdEvaluation`
     FROM `Evaluations` e
     WHERE e.`IdCours` = n.`IdCours`
     ORDER BY e.`IdEvaluation`
     LIMIT 1
   )
   WHERE n.`IdEvaluation` IS NULL
     AND n.`IdCours` IS NOT NULL
     AND n.`IdCours` > 0',
  'SELECT ''Pas de colonne IdCours — skip backfill'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.e Contrôle : notes encore sans IdEvaluation
SELECT COUNT(*) AS NotesSansIdEvaluation
FROM `Notes`
WHERE `IdEvaluation` IS NULL;

-- Si ce COUNT > 0 : ARRÊTER ici, corriger manuellement (notes orphelines / cours sans classe),
-- puis relancer le script. Ne pas forcer NOT NULL.

-- 1.f NOT NULL + index + FK (seulement si aucune note orpheline et IdEvaluation présent)
SET @orphans := (SELECT COUNT(*) FROM `Notes` WHERE `IdEvaluation` IS NULL);
SET @hasEval := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND COLUMN_NAME = 'IdEvaluation'
);

SET @sql := IF(@hasEval > 0 AND @orphans = 0,
  'ALTER TABLE `Notes` MODIFY COLUMN `IdEvaluation` INT NOT NULL',
  IF(@orphans > 0,
    'SELECT ''STOP: notes sans IdEvaluation — corriger avant NOT NULL'' AS Erreur, (SELECT COUNT(*) FROM `Notes` WHERE `IdEvaluation` IS NULL) AS NbOrphelines',
    'SELECT ''Skip MODIFY IdEvaluation'' AS Info'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx := (
  SELECT INDEX_NAME FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND INDEX_NAME = 'IX_Notes_IdEvaluation'
  LIMIT 1
);
SET @sql := IF(@hasEval > 0 AND @orphans = 0 AND @idx IS NULL,
  'CREATE INDEX `IX_Notes_IdEvaluation` ON `Notes` (`IdEvaluation`)',
  'SELECT ''Index IX_Notes_IdEvaluation OK/skip'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk := (
  SELECT CONSTRAINT_NAME FROM information_schema.TABLE_CONSTRAINTS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes'
    AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    AND CONSTRAINT_NAME = 'FK_Notes_Evaluations_IdEvaluation'
  LIMIT 1
);
SET @sql := IF(@hasEval > 0 AND @orphans = 0 AND @fk IS NULL,
  'ALTER TABLE `Notes`
     ADD CONSTRAINT `FK_Notes_Evaluations_IdEvaluation`
     FOREIGN KEY (`IdEvaluation`) REFERENCES `Evaluations` (`IdEvaluation`)',
  'SELECT ''FK IdEvaluation OK/skip'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.g Drop FK / index / colonne IdCours
SET @fkCours := (
  SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes'
    AND COLUMN_NAME = 'IdCours' AND REFERENCED_TABLE_NAME IS NOT NULL
  LIMIT 1
);
SET @sql := IF(@fkCours IS NOT NULL,
  CONCAT('ALTER TABLE `Notes` DROP FOREIGN KEY `', @fkCours, '`'),
  'SELECT ''FK IdCours absente'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idxCours := (
  SELECT INDEX_NAME FROM information_schema.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes'
    AND INDEX_NAME IN ('IX_Notes_IdCours', 'FK_Notes_Cours_IdCours')
  LIMIT 1
);
SET @sql := IF(@idxCours IS NOT NULL,
  CONCAT('ALTER TABLE `Notes` DROP INDEX `', @idxCours, '`'),
  'SELECT ''Index IdCours absent'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCours := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND COLUMN_NAME = 'IdCours'
);
SET @sql := IF(@hasCours > 0 AND @orphans = 0,
  'ALTER TABLE `Notes` DROP COLUMN `IdCours`',
  'SELECT ''Drop IdCours skip (absent ou orphelines)'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 2. Drop Session sur Notes si présent
-- -----------------------------------------------------------------------------

SET @hasSession := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Notes' AND COLUMN_NAME = 'Session'
);
SET @sql := IF(@hasSession > 0,
  'ALTER TABLE `Notes` DROP COLUMN `Session`',
  'SELECT ''Notes.Session absente'' AS Info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 3. Ne JAMAIS créer CoursIdCours — si présent, le signaler seulement
-- -----------------------------------------------------------------------------

SELECT
  CASE WHEN COUNT(*) > 0
    THEN 'ATTENTION: CoursIdCours existe encore — exécuter 20260917_DropNotesCoursIdCours.sql'
    ELSE 'OK: pas de CoursIdCours'
  END AS StatutCoursIdCours
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Notes'
  AND COLUMN_NAME = 'CoursIdCours';

-- -----------------------------------------------------------------------------
-- 4. Vérifications finales
-- -----------------------------------------------------------------------------

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_KEY
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Notes'
ORDER BY ORDINAL_POSITION;

SELECT
  COUNT(*) AS NbNotes,
  SUM(CASE WHEN `IdEvaluation` IS NOT NULL THEN 1 ELSE 0 END) AS AvecIdEvaluation
FROM `Notes`;

-- =============================================================================
-- Fin — après succès : retester GET /api/Note/eleve/{id} et bulletins
-- =============================================================================
