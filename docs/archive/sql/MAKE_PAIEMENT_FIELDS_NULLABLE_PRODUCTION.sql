-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Rendre nullable les champs JustificatifUrl, ReferenceTransaction et Commentaire
-- Date : 2024-12-04
-- Description : 
--   Modifie la table Paiements pour rendre nullable les champs :
--   - JustificatifUrl
--   - ReferenceTransaction
--   - Commentaire
--   Cela résout l'incohérence entre le modèle C# (nullable) et la base de données (NOT NULL)
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
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME IN ('JustificatifUrl', 'ReferenceTransaction', 'Commentaire')
ORDER BY COLUMN_NAME;

-- ============================================================
-- 2. Vérifier s'il y a des valeurs NULL existantes
-- ============================================================

SELECT 
    COUNT(*) AS TotalPaiements,
    SUM(CASE WHEN JustificatifUrl IS NULL THEN 1 ELSE 0 END) AS JustificatifUrl_Null,
    SUM(CASE WHEN ReferenceTransaction IS NULL THEN 1 ELSE 0 END) AS ReferenceTransaction_Null,
    SUM(CASE WHEN Commentaire IS NULL THEN 1 ELSE 0 END) AS Commentaire_Null
FROM Paiements;

-- ============================================================
-- 3. Modifier la colonne JustificatifUrl pour la rendre nullable
-- ============================================================

-- Vérifier si la colonne existe et n'est pas déjà nullable
SELECT 
    CASE 
        WHEN IS_NULLABLE = 'NO' THEN 'Colonne NOT NULL - Modification nécessaire'
        WHEN IS_NULLABLE = 'YES' THEN 'Colonne déjà nullable - Aucune modification nécessaire'
        ELSE 'État inconnu'
    END AS StatutJustificatifUrl
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME = 'JustificatifUrl';

-- Modifier la colonne (si elle n'est pas déjà nullable)
ALTER TABLE `Paiements` 
MODIFY COLUMN `JustificatifUrl` LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- ============================================================
-- 4. Modifier la colonne ReferenceTransaction pour la rendre nullable
-- ============================================================

-- Vérifier si la colonne existe et n'est pas déjà nullable
SELECT 
    CASE 
        WHEN IS_NULLABLE = 'NO' THEN 'Colonne NOT NULL - Modification nécessaire'
        WHEN IS_NULLABLE = 'YES' THEN 'Colonne déjà nullable - Aucune modification nécessaire'
        ELSE 'État inconnu'
    END AS StatutReferenceTransaction
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME = 'ReferenceTransaction';

-- Modifier la colonne (si elle n'est pas déjà nullable)
ALTER TABLE `Paiements` 
MODIFY COLUMN `ReferenceTransaction` LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- ============================================================
-- 5. Modifier la colonne Commentaire pour la rendre nullable
-- ============================================================

-- Vérifier si la colonne existe et n'est pas déjà nullable
SELECT 
    CASE 
        WHEN IS_NULLABLE = 'NO' THEN 'Colonne NOT NULL - Modification nécessaire'
        WHEN IS_NULLABLE = 'YES' THEN 'Colonne déjà nullable - Aucune modification nécessaire'
        ELSE 'État inconnu'
    END AS StatutCommentaire
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME = 'Commentaire';

-- Modifier la colonne (si elle n'est pas déjà nullable)
ALTER TABLE `Paiements` 
MODIFY COLUMN `Commentaire` LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- ============================================================
-- 6. Vérification finale
-- ============================================================

SELECT 
    'Vérification finale - État des colonnes après modification' AS Status;

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME IN ('JustificatifUrl', 'ReferenceTransaction', 'Commentaire')
ORDER BY COLUMN_NAME;

-- ============================================================
-- 7. Vérifier qu'il n'y a pas d'erreurs
-- ============================================================

SELECT 
    CASE 
        WHEN COUNT(*) = 3 AND SUM(CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END) = 3 
        THEN '✅ Toutes les colonnes sont maintenant nullable'
        WHEN COUNT(*) = 3 AND SUM(CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END) < 3
        THEN CONCAT('⚠️ ', SUM(CASE WHEN IS_NULLABLE = 'NO' THEN 1 ELSE 0 END), ' colonne(s) restent NOT NULL')
        ELSE '❌ Erreur : Nombre de colonnes inattendu'
    END AS Resultat
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Paiements'
  AND COLUMN_NAME IN ('JustificatifUrl', 'ReferenceTransaction', 'Commentaire');

SELECT '✅ Migration terminée avec succès' AS Status;

-- ============================================================
-- NOTES IMPORTANTES
-- ============================================================
-- 
-- 1. Ce script rend les colonnes nullable, ce qui correspond au modèle C#
-- 2. Les valeurs existantes (chaînes vides ou autres) sont conservées
-- 3. Les nouvelles insertions peuvent maintenant utiliser NULL pour ces champs
-- 4. Cela résout l'erreur "Column 'JustificatifUrl' cannot be null" lors de la réinjection
-- 
-- ============================================================

