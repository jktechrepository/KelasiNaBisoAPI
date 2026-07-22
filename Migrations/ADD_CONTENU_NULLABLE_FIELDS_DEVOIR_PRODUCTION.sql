-- ============================================================================
-- SCRIPT SQL : AJOUT DU CHAMP CONTENU ET RENDRE NULLABLE LES CHAMPS FICHIER
-- ============================================================================
-- Date : 2025-12-01
-- Objectif : Modifier la table DevoirsADomicile pour :
--   1. Ajouter le champ Contenu (textuel)
--   2. Rendre nullable les champs NomFichier, CheminFichier, TailleFichier, TypeMIME
-- 
-- ⚠️ IMPORTANT : Exécuter ce script sur la base de données de PRODUCTION
-- ============================================================================

-- Désactiver temporairement les vérifications de clés étrangères
SET FOREIGN_KEY_CHECKS = 0;
SET SQL_SAFE_UPDATES = 0;

-- ============================================================================
-- ÉTAPE 1 : VÉRIFIER L'ÉTAT ACTUEL
-- ============================================================================

SELECT '🔍 Vérification de l''état actuel de la table...' AS Status;

-- Vérifier la structure actuelle
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME IN ('Contenu', 'NomFichier', 'CheminFichier', 'TailleFichier', 'TypeMIME')
ORDER BY COLUMN_NAME;

-- Compter les devoirs existants
SELECT 
    COUNT(*) AS NombreDevoirs,
    COUNT(NomFichier) AS DevoirsAvecFichier,
    COUNT(*) - COUNT(NomFichier) AS DevoirsSansFichier
FROM DevoirsADomicile;

-- ============================================================================
-- ÉTAPE 2 : AJOUTER LE CHAMP CONTENU
-- ============================================================================

SELECT '📝 Ajout du champ Contenu...' AS Status;

-- Vérifier si la colonne Contenu existe déjà
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'DevoirsADomicile'
      AND COLUMN_NAME = 'Contenu'
);

-- Ajouter la colonne Contenu si elle n'existe pas
SET @sql = IF(@column_exists = 0,
    'ALTER TABLE DevoirsADomicile ADD COLUMN Contenu VARCHAR(5000) CHARACTER SET utf8mb4 NULL AFTER Description',
    'SELECT "✅ La colonne Contenu existe déjà" AS Message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- ÉTAPE 3 : RENDRE NULLABLE LES CHAMPS DE FICHIER
-- ============================================================================

SELECT '🔧 Modification des champs de fichier pour les rendre nullable...' AS Status;

-- 3.1. Modifier TypeMIME
ALTER TABLE DevoirsADomicile 
MODIFY COLUMN TypeMIME VARCHAR(100) CHARACTER SET utf8mb4 NULL;

-- 3.2. Modifier TailleFichier
ALTER TABLE DevoirsADomicile 
MODIFY COLUMN TailleFichier BIGINT NULL;

-- 3.3. Modifier NomFichier
ALTER TABLE DevoirsADomicile 
MODIFY COLUMN NomFichier VARCHAR(500) CHARACTER SET utf8mb4 NULL;

-- 3.4. Modifier CheminFichier
ALTER TABLE DevoirsADomicile 
MODIFY COLUMN CheminFichier VARCHAR(1000) CHARACTER SET utf8mb4 NULL;

-- ============================================================================
-- ÉTAPE 4 : VÉRIFICATION POST-MODIFICATION
-- ============================================================================

SELECT '✅ Vérification des modifications...' AS Status;

-- Vérifier la nouvelle structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'DevoirsADomicile'
  AND COLUMN_NAME IN ('Contenu', 'NomFichier', 'CheminFichier', 'TailleFichier', 'TypeMIME')
ORDER BY COLUMN_NAME;

-- Vérifier que les données existantes sont intactes
SELECT 
    COUNT(*) AS TotalDevoirs,
    COUNT(NomFichier) AS DevoirsAvecFichier,
    COUNT(Contenu) AS DevoirsAvecContenu,
    COUNT(CASE WHEN NomFichier IS NULL AND Contenu IS NULL THEN 1 END) AS DevoirsVides
FROM DevoirsADomicile;

-- ============================================================================
-- ÉTAPE 5 : RÉACTIVER LES VÉRIFICATIONS
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 1;
SET SQL_SAFE_UPDATES = 1;

SELECT '✅ Script terminé avec succès !' AS Status;

-- ============================================================================
-- RÉSUMÉ DES MODIFICATIONS
-- ============================================================================
-- 
-- Modifications appliquées :
--   1. ✅ Colonne Contenu ajoutée (VARCHAR(5000), nullable)
--   2. ✅ TypeMIME rendu nullable
--   3. ✅ TailleFichier rendu nullable
--   4. ✅ NomFichier rendu nullable
--   5. ✅ CheminFichier rendu nullable
--
-- Impact sur les données existantes :
--   - ✅ Aucune perte de données
--   - ✅ Les devoirs existants conservent leurs valeurs
--   - ✅ Les champs deviennent nullable, permettant de nouveaux devoirs sans fichier
--
-- ============================================================================

