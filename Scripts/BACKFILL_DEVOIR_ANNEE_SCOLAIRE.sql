-- =============================================================================
-- BACKFILL_DEVOIR_ANNEE_SCOLAIRE.sql
-- =============================================================================
-- Problème : GET /api/DevoirADomicile/tuteur/{idTuteur} → 500
--   MySqlException: Unknown column 'd.IdAnneeScolaire' in 'SELECT'
-- Cause : API déployée avec scoping année ; colonne absente en prod.
--
-- Miroir de la migration EF : 20260829120000_AddDevoirADomicileAnneeScolaire
-- Préférer `dotnet ef database update` si l’historique EF est sain.
-- Sinon exécuter ce script manuellement sur la base prod/dev.
--
-- ⚠️ BASE REQUISE (phpMyAdmin / mysql client)
--   Sélectionner d’abord la base MÉTIER de l’API (ex. knb_db, prod-knb_db).
--   NE PAS exécuter sous information_schema, mysql, performance_schema, sys.
--   Erreur typique si mauvaise base :
--     #1109 - Table inconnue 'devoirsadomicile' dans information_schema
--
-- Usage CLI (recommandé — force la bonne base) :
--   mysql -u ... -p NOM_BASE_METIER < Scripts/BACKFILL_DEVOIR_ANNEE_SCOLAIRE.sql
--
-- Usage phpMyAdmin :
--   1) Cliquer la base métier dans le panneau gauche
--   2) SELECT DATABASE();  -- doit afficher le nom métier, pas information_schema
--   3) SHOW TABLES LIKE '%Devoir%';  -- doit lister DevoirsADomicile
--   4) Importer / coller ce script dans l’onglet SQL
--
-- Pré-check (sur la base métier) :
--   SELECT DATABASE();
--   SHOW TABLES LIKE '%Devoir%';
--   SELECT COUNT(*) AS has_col FROM information_schema.COLUMNS
--   WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
--     AND COLUMN_NAME = 'IdAnneeScolaire';
--   SELECT MigrationId FROM __EFMigrationsHistory
--   WHERE MigrationId LIKE '%DevoirADomicileAnnee%';
--
-- Post-check (après) :
--   SHOW COLUMNS FROM `DevoirsADomicile` LIKE 'IdAnneeScolaire';
--   SELECT COUNT(*) AS nulls FROM `DevoirsADomicile` WHERE IdAnneeScolaire IS NULL; -- 0
--   (phpMyAdmin peut ajouter LIMIT 0,25 — inoffensif)
--   Puis : GET /api/DevoirADomicile/tuteur/{idTuteur} → 200
--
-- Idempotent si la colonne / FK / index existent déjà.
-- ATTENTION : MODIFY NOT NULL échoue s’il reste des NULL après backfill
--   (école sans année scolaire) — corriger les AnneeScolaires puis relancer.
-- =============================================================================

-- 0a) Garde : schéma courant = base métier
SET @db = DATABASE();
SET @bad_schema = (
    @db IS NULL
    OR LOWER(@db) IN ('information_schema', 'mysql', 'performance_schema', 'sys')
);
SET @msg = CONCAT(
    'ABORT: mauvais schéma (DATABASE()=', IFNULL(@db, 'NULL'),
    '). Sélectionnez la base métier de l''API avant d''exécuter ce script. ',
    'Dans phpMyAdmin : cliquer la base app, pas information_schema.'
);
-- SIGNAL via procédure préparée (compatible clients qui n’aiment pas SIGNAL nu)
SET @sql = IF(@bad_schema = 1,
    CONCAT('SIGNAL SQLSTATE ''45000'' SET MESSAGE_TEXT = ''', REPLACE(@msg, '''', ''''''), ''''),
    'SELECT DATABASE() AS current_database');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 0b) Garde : table DevoirsADomicile présente
SET @tbl = (
    SELECT COUNT(1) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'DevoirsADomicile'
);
SET @msg = CONCAT(
    'ABORT: table DevoirsADomicile introuvable dans ', IFNULL(DATABASE(), 'NULL'),
    '. Vérifiez SHOW TABLES LIKE ''%Devoir%'' et que vous êtes sur la bonne base.'
);
SET @sql = IF(@tbl = 0,
    CONCAT('SIGNAL SQLSTATE ''45000'' SET MESSAGE_TEXT = ''', REPLACE(@msg, '''', ''''''), ''''),
    'SELECT ''DevoirsADomicile OK'' AS table_check');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 1) Colonne nullable
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

-- 2) Backfill : année contenant DatePublication (même école)
UPDATE `DevoirsADomicile` d
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

-- 3) Fallback : année active la plus récente de l’école
UPDATE `DevoirsADomicile` d
SET d.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE d.IdAnneeScolaire IS NULL;

-- 4) Fallback global (dernier IdAnneeScolaire)
UPDATE `DevoirsADomicile`
SET IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire FROM AnneeScolaires a ORDER BY a.IdAnneeScolaire DESC LIMIT 1
)
WHERE IdAnneeScolaire IS NULL
  AND EXISTS (SELECT 1 FROM AnneeScolaires LIMIT 1);

-- 5) NOT NULL (échoue si des NULL restent)
ALTER TABLE `DevoirsADomicile`
    MODIFY COLUMN `IdAnneeScolaire` int NOT NULL;

-- 6) FK
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

-- 7) Index
SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_IdAnneeScolaire'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_DevoirADomicile_IdAnneeScolaire` ON `DevoirsADomicile` (`IdAnneeScolaire`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx = (
    SELECT COUNT(1) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DevoirsADomicile'
      AND INDEX_NAME = 'IX_DevoirADomicile_Classe_Annee_Statut'
);
SET @sql = IF(@idx = 0, 'CREATE INDEX `IX_DevoirADomicile_Classe_Annee_Statut` ON `DevoirsADomicile` (`IdClasse`, `IdAnneeScolaire`, `Statut`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 8) Historique EF (évite rejeu de la migration)
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260829120000_AddDevoirADomicileAnneeScolaire', '6.0.25'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260829120000_AddDevoirADomicileAnneeScolaire'
);

-- Post-check rapide (sur DATABASE() courant — backticks obligatoires)
SELECT DATABASE() AS current_database;

SELECT COUNT(*) AS has_col
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME = 'IdAnneeScolaire';

SELECT COUNT(*) AS remaining_nulls
FROM `DevoirsADomicile`
WHERE IdAnneeScolaire IS NULL;
