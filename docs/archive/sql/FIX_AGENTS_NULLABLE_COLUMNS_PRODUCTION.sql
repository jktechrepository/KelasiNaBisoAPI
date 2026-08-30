-- ═══════════════════════════════════════════════════════════════════════════════
-- 📋 SCRIPT DE CORRECTION : Colonnes NULLABLE pour la table Agents
-- ═══════════════════════════════════════════════════════════════════════════════
--
-- Description : Corrige les colonnes de la table Agents pour permettre les valeurs NULL
--               conformément au modèle C# où ces propriétés sont déclarées comme nullable
-- Date : 2025-11-18
-- Base de données : MariaDB 10.11+
--
-- ⚠️ IMPORTANT :
--   1. Faire une sauvegarde complète de la base de données avant d'exécuter ce script
--   2. Tester d'abord sur une base de données de test
--   3. Exécuter ce script pendant une période de faible activité
--   4. Vérifier les résultats après l'exécution
--
-- ═══════════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATIONS PRÉLIMINAIRES
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que la table Agents existe
SELECT COUNT(*) as TableExists
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_name = 'Agents';

-- Vérifier les colonnes actuelles et leurs contraintes NULL
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE table_schema = DATABASE()
  AND table_name = 'Agents'
  AND COLUMN_NAME IN ('Nom', 'Postnom', 'Prenom', 'Genre', 'EtatCivil', 'Statut')
ORDER BY ORDINAL_POSITION;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : CORRECTION DES COLONNES POUR PERMETTRE NULL
-- ═══════════════════════════════════════════════════════════════════════════════

-- ⚠️ ATTENTION : Si des valeurs NULL existent déjà, elles seront converties en chaînes vides
-- Si vous préférez garder les NULL, commentez les lignes UPDATE et modifiez directement les colonnes

-- 1. Colonne Nom
-- UPDATE `Agents` SET `Nom` = '' WHERE `Nom` IS NULL;
ALTER TABLE `Agents` 
    MODIFY COLUMN `Nom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- 2. Colonne Postnom
-- UPDATE `Agents` SET `Postnom` = '' WHERE `Postnom` IS NULL;
ALTER TABLE `Agents` 
    MODIFY COLUMN `Postnom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- 3. Colonne Prenom
-- UPDATE `Agents` SET `Prenom` = '' WHERE `Prenom` IS NULL;
ALTER TABLE `Agents` 
    MODIFY COLUMN `Prenom` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- 4. Colonne Genre
-- UPDATE `Agents` SET `Genre` = '' WHERE `Genre` IS NULL;
ALTER TABLE `Agents` 
    MODIFY COLUMN `Genre` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- 5. Colonne EtatCivil
-- UPDATE `Agents` SET `EtatCivil` = '' WHERE `EtatCivil` IS NULL;
ALTER TABLE `Agents` 
    MODIFY COLUMN `EtatCivil` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL;

-- 6. Colonne Statut (déjà nullable dans le modèle, mais vérifions)
ALTER TABLE `Agents` 
    MODIFY COLUMN `Statut` tinyint(1) NULL;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : VÉRIFICATIONS POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que les colonnes acceptent maintenant NULL
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE table_schema = DATABASE()
  AND table_name = 'Agents'
  AND COLUMN_NAME IN ('Nom', 'Postnom', 'Prenom', 'Genre', 'EtatCivil', 'Statut')
ORDER BY ORDINAL_POSITION;

-- Vérifier qu'il n'y a pas d'erreurs
SELECT '✅ Migration terminée avec succès' AS Status;
