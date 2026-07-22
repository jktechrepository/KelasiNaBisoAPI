-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Correction des valeurs NULL pour DateEchec et DateCreation dans PaiementsCrashed
-- Date : 2024-12-XX
-- Description : 
--   Met à jour les enregistrements qui ont des valeurs NULL pour DateEchec ou DateCreation
--   en leur assignant une valeur par défaut (DateEchec ou DateCreation actuelle)
-- ============================================================
-- ⚠️ IMPORTANT : Exécuter ce script pendant une fenêtre de maintenance
-- ⚠️ IMPORTANT : Faire une sauvegarde de la base de données avant l'exécution
-- ============================================================

USE `dev-knb_db`;

-- ============================================================
-- 1. Vérifier l'état actuel des enregistrements avec NULL
-- ============================================================
SELECT 
    COUNT(*) AS TotalEnregistrements,
    SUM(CASE WHEN DateEchec IS NULL THEN 1 ELSE 0 END) AS DateEchecNull,
    SUM(CASE WHEN DateCreation IS NULL THEN 1 ELSE 0 END) AS DateCreationNull
FROM 
    `PaiementsCrashed`;

-- ============================================================
-- 2. Afficher les enregistrements avec NULL (pour vérification)
-- ============================================================
SELECT 
    IdPaiementCrashed,
    DateEchec,
    DateCreation,
    DatePaiement,
    NomFichierOriginal
FROM 
    `PaiementsCrashed`
WHERE 
    DateEchec IS NULL OR DateCreation IS NULL
ORDER BY 
    IdPaiementCrashed;

-- ============================================================
-- 3. Corriger DateEchec NULL
-- ============================================================
-- Si DateEchec est NULL, utiliser DateCreation si disponible, sinon la date actuelle
UPDATE `PaiementsCrashed`
SET 
    DateEchec = COALESCE(DateCreation, NOW())
WHERE 
    DateEchec IS NULL;

-- ============================================================
-- 4. Corriger DateCreation NULL
-- ============================================================
-- Si DateCreation est NULL, utiliser DateEchec si disponible, sinon la date actuelle
UPDATE `PaiementsCrashed`
SET 
    DateCreation = COALESCE(DateEchec, NOW())
WHERE 
    DateCreation IS NULL;

-- ============================================================
-- 5. Vérification finale
-- ============================================================
SELECT 
    COUNT(*) AS TotalEnregistrements,
    SUM(CASE WHEN DateEchec IS NULL THEN 1 ELSE 0 END) AS DateEchecNull,
    SUM(CASE WHEN DateCreation IS NULL THEN 1 ELSE 0 END) AS DateCreationNull
FROM 
    `PaiementsCrashed`;

SELECT '✅ Correction terminée avec succès' AS Status;

