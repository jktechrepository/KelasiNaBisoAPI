-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Rendre DateEchec et DateCreation nullable dans PaiementsCrashed
-- Date : 2024-12-XX
-- Description : 
--   Modifie le schéma pour permettre NULL sur DateEchec et DateCreation
--   Corrige les enregistrements existants avec NULL en leur assignant une valeur par défaut
-- ============================================================
-- ⚠️ IMPORTANT : Exécuter ce script pendant une fenêtre de maintenance
-- ⚠️ IMPORTANT : Faire une sauvegarde de la base de données avant l'exécution
-- ============================================================

USE `dev-knb_db`;

-- ============================================================
-- 1. Vérifier l'état actuel des colonnes
-- ============================================================
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    TABLE_SCHEMA = 'dev-knb_db' AND 
    TABLE_NAME = 'PaiementsCrashed' AND 
    COLUMN_NAME IN ('DateEchec', 'DateCreation');

-- ============================================================
-- 2. Compter les enregistrements avec NULL
-- ============================================================
SELECT 
    COUNT(*) AS TotalEnregistrements,
    SUM(CASE WHEN DateEchec IS NULL THEN 1 ELSE 0 END) AS DateEchecNull,
    SUM(CASE WHEN DateCreation IS NULL THEN 1 ELSE 0 END) AS DateCreationNull
FROM 
    `PaiementsCrashed`;

-- ============================================================
-- 3. Corriger les données existantes avec NULL
-- ============================================================
-- Si DateEchec est NULL, utiliser DateCreation si disponible, sinon la date actuelle
UPDATE `PaiementsCrashed`
SET 
    DateEchec = COALESCE(DateCreation, NOW())
WHERE 
    DateEchec IS NULL;

-- Si DateCreation est NULL, utiliser DateEchec si disponible, sinon la date actuelle
UPDATE `PaiementsCrashed`
SET 
    DateCreation = COALESCE(DateEchec, NOW())
WHERE 
    DateCreation IS NULL;

-- ============================================================
-- 4. Modifier le schéma pour permettre NULL
-- ============================================================

-- Modifier DateEchec pour permettre NULL
ALTER TABLE `PaiementsCrashed` 
MODIFY COLUMN `DateEchec` DATETIME NULL;

-- Modifier DateCreation pour permettre NULL
ALTER TABLE `PaiementsCrashed` 
MODIFY COLUMN `DateCreation` DATETIME NULL;

-- ============================================================
-- 5. Vérification finale
-- ============================================================
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    TABLE_SCHEMA = 'dev-knb_db' AND 
    TABLE_NAME = 'PaiementsCrashed' AND 
    COLUMN_NAME IN ('DateEchec', 'DateCreation');

SELECT '✅ Migration terminée avec succès' AS Status;

