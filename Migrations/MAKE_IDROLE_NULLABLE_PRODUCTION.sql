-- ═══════════════════════════════════════════════════════════════════════════════
-- 📋 SCRIPT DE MIGRATION : Rendre IdRole nullable dans la table Utilisateurs
-- ═══════════════════════════════════════════════════════════════════════════════
--
-- Description : Rend la colonne IdRole nullable dans la table Utilisateurs
--               pour permettre le système multi-rôles via la table UserRoles
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

-- Vérifier que la table Utilisateurs existe
SELECT COUNT(*) as TableExists
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_name = 'Utilisateurs';

-- Vérifier l'état actuel de la colonne IdRole
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE table_schema = DATABASE()
  AND table_name = 'Utilisateurs'
  AND COLUMN_NAME = 'IdRole';

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : MODIFICATION DE LA COLONNE IdRole POUR PERMETTRE NULL
-- ═══════════════════════════════════════════════════════════════════════════════

-- Modifier la colonne IdRole pour permettre NULL
ALTER TABLE `Utilisateurs` 
    MODIFY COLUMN `IdRole` int NULL;

-- ═══════════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : VÉRIFICATIONS POST-MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════

-- Vérifier que la colonne accepte maintenant NULL
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE,
    COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE table_schema = DATABASE()
  AND table_name = 'Utilisateurs'
  AND COLUMN_NAME = 'IdRole';

-- Vérifier qu'il n'y a pas d'erreurs
SELECT '✅ Migration terminée avec succès' AS Status;

