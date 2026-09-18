-- =============================================================================
-- KelasiNaBiso — Débloquer mysqldump : vue cassée extraire_all_eleve
-- Fichier : docs/sql/20260917_FixOrDrop_extraire_all_eleve.sql
-- Base    : knb_db (prod) / MariaDB-MySQL
-- =============================================================================
--
-- Symptôme panneau hébergeur / mysqldump :
--   Unable to backup database 'knb_db'
--   mysqldump: Couldn't execute 'SHOW FIELDS FROM extraire_all_eleve':
--   View 'knb_db.extraire_all_eleve' references invalid table(s) or column(s)...
--   (Error 1356)
--
-- Cause : vue LEGACY (hors repo API) invalide après évolutions schéma
--   (souvent Notes.IdCours / Eleves.IdClasse / Notes.Session).
--
-- Action de ce script : DIAGNOSTIC + DROP de la vue pour débloquer l’export.
--   Pas de CREATE ici (définition absente du repo). Pour une liste élèves
--   à jour, utiliser V_Eleve / EleveParEcole :
--     Scripts/PRODUCTION_RECREATE_V_ELEVE.sql
--
-- Alternative dump SANS drop (CLI) :
--   mysqldump -u USER -p knb_db --ignore-table=knb_db.extraire_all_eleve > backup.sql
--
-- Prérequis : USE knb_db;  (ou sélectionner knb_db dans phpMyAdmin)
-- =============================================================================

SET NAMES utf8mb4;

SELECT DATABASE() AS base_courante;

-- -----------------------------------------------------------------------------
-- 1. Lister les vues
-- -----------------------------------------------------------------------------

SHOW FULL TABLES WHERE Table_type = 'VIEW';

-- -----------------------------------------------------------------------------
-- 2. Métadonnées extraire_all_eleve (peut rester lisible même si la vue est cassée)
-- -----------------------------------------------------------------------------

SELECT
  TABLE_NAME,
  VIEW_DEFINITION,
  CHECK_OPTION,
  IS_UPDATABLE,
  DEFINER,
  SECURITY_TYPE
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'extraire_all_eleve';

-- SHOW CREATE VIEW peut échouer (1356) si la vue est trop cassée — normal.
-- SHOW CREATE VIEW `extraire_all_eleve`;

-- -----------------------------------------------------------------------------
-- 3. DROP — débloque « Exporter le dump »
-- -----------------------------------------------------------------------------

DROP VIEW IF EXISTS `extraire_all_eleve`;

SELECT
  CASE
    WHEN COUNT(*) = 0 THEN 'OK — extraire_all_eleve supprimée (dump possible)'
    ELSE 'ATTENTION — la vue existe encore'
  END AS statut_drop
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'extraire_all_eleve';

-- -----------------------------------------------------------------------------
-- 4. Autres vues à tester manuellement si le dump échoue encore
--    (exécuter une par une : SELECT 1 FROM `NomVue` LIMIT 1;)
-- -----------------------------------------------------------------------------

SELECT TABLE_NAME AS vue
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE()
ORDER BY TABLE_NAME;

-- Si une autre vue échoue au dump, soit :
--   DROP VIEW IF EXISTS `NomVue`;
-- soit ignore-table au mysqldump, puis recréer via Scripts/PRODUCTION_RECREATE_V_ELEVE.sql
-- pour V_Eleve / EleveParEcole.

-- -----------------------------------------------------------------------------
-- 5. Rappels schéma (erreurs API Notes / bulletin)
-- -----------------------------------------------------------------------------
-- Si GET /api/Note/... → Unknown column IdEvaluation :
--   docs/sql/20260917_MigrateNotes_IdCours_To_IdEvaluation.sql
-- Si shadow CoursIdCours (code déjà corrigé) côté DB résiduelle :
--   docs/sql/20260917_DropNotesCoursIdCours.sql

-- =============================================================================
-- Après ce script : relancer « Exporter le dump » sur knb_db
-- =============================================================================
