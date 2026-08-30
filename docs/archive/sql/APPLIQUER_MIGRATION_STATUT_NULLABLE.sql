-- =========================================================================
-- 🔧 SCRIPT COMPLET POUR APPLIQUER LA MIGRATION "MakeStatutNullable"
-- =========================================================================
-- Ce script doit être exécuté dans HeidiSQL, phpMyAdmin ou MySQL Workbench
-- 
-- ✅ OBJECTIF : Rendre tous les champs "Statut" nullables (bool?)
-- 
-- 📋 ÉTAPES :
--   1. Créer la table __EFMigrationsHistory si elle n'existe pas
--   2. Marquer les anciennes migrations comme déjà appliquées
--   3. Modifier toutes les colonnes Statut pour accepter NULL
--   4. Enregistrer la nouvelle migration MakeStatutNullable
-- =========================================================================

USE `kelasinabiso`;

START TRANSACTION;

-- =========================================================================
-- ÉTAPE 1 : Créer la table de tracking des migrations si elle n'existe pas
-- =========================================================================
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =========================================================================
-- ÉTAPE 2 : Marquer les anciennes migrations comme déjà appliquées
-- =========================================================================
-- Ces migrations ont déjà créé les tables dans votre base de données

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES
    ('20251028103016_InitialCreate', '6.0.25'),
    ('20251031052322_AddAcceptNotificationToEcole', '6.0.25'),
    ('20251031054943_UpdateAcceptNotificationToTrue', '6.0.25'),
    ('20251101191812_AddAuditLogTable', '6.0.25');

-- =========================================================================
-- ÉTAPE 3 : Modifier toutes les colonnes Statut pour accepter NULL
-- =========================================================================
-- Cette section rend tous les champs "Statut" nullables (bool? au lieu de bool)

ALTER TABLE `Vacations` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Utilisateurs` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `UserDevices` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Tuteurs` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `TitulairesClasses` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Sections` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Roles` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `RessourcePedagogiques` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Presences` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Permissions` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Paiements` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Options` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Notifications` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Notes` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Messages` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Inscriptions` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Horaires` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `GroupeMessages` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Frais` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Evaluations` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Eleves` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Ecoles` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Ecoles` MODIFY COLUMN `AcceptNotification` tinyint(1) NULL;

ALTER TABLE `Documents` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Directions` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Cours` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Classes` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `AnneeScolaires` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `Agents` MODIFY COLUMN `Statut` tinyint(1) NULL;

ALTER TABLE `AffectationsCours` MODIFY COLUMN `Statut` tinyint(1) NULL;

-- =========================================================================
-- BONUS : Rendre le champ Niveau de la table Roles nullable aussi
-- =========================================================================
ALTER TABLE `Roles` MODIFY COLUMN `Niveau` int NULL;

-- =========================================================================
-- CORRECTION : Ajouter la colonne ReferenceTransaction manquante dans Paiements
-- =========================================================================
-- Vérifier si la colonne existe avant de l'ajouter
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists 
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = 'kelasinabiso' 
  AND TABLE_NAME = 'Paiements' 
  AND COLUMN_NAME = 'ReferenceTransaction';

SET @query = IF(@col_exists = 0,
    'ALTER TABLE `Paiements` ADD COLUMN `ReferenceTransaction` longtext NULL AFTER `StatutPaiement`',
    'SELECT "Colonne ReferenceTransaction existe déjà" AS Message');

PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- =========================================================================
-- CORRECTION 2 : Rendre les champs Guid (Reference) nullables
-- =========================================================================
ALTER TABLE `Eleves` MODIFY COLUMN `ReferenceEleve` char(36) NULL;
ALTER TABLE `Utilisateurs` MODIFY COLUMN `ReferenceUtilisateur` char(36) NULL;

-- =========================================================================
-- CORRECTION 3 : Nettoyer les valeurs Guid invalides (chaînes vides → NULL)
-- =========================================================================
-- Convertir les chaînes vides, invalides ou '00000000-0000-0000-0000-000000000000' en NULL
UPDATE `Eleves` 
SET `ReferenceEleve` = NULL 
WHERE `ReferenceEleve` = '' 
   OR `ReferenceEleve` = '00000000-0000-0000-0000-000000000000'
   OR LENGTH(`ReferenceEleve`) != 36;

UPDATE `Utilisateurs` 
SET `ReferenceUtilisateur` = NULL 
WHERE `ReferenceUtilisateur` = '' 
   OR `ReferenceUtilisateur` = '00000000-0000-0000-0000-000000000000'
   OR LENGTH(`ReferenceUtilisateur`) != 36;

-- =========================================================================
-- ÉTAPE 4 : Enregistrer toutes les migrations comme appliquées
-- =========================================================================
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES 
    ('20251103133630_MakeStatutNullable', '6.0.25'),
    ('20251103202416_UpdateRoleNiveauToNullable', '6.0.25'),
    ('20251103212506_MakeGuidFieldsNullable', '6.0.25');

-- =========================================================================
-- ✅ FINALISATION : Valider les changements
-- =========================================================================
COMMIT;

-- =========================================================================
-- 🎉 SUCCÈS ! Migration appliquée avec succès !
-- =========================================================================
-- Vous pouvez maintenant :
--   1. Démarrer votre application ASP.NET Core
--   2. Les champs "Statut" acceptent désormais true, false, ou null
--   3. Entity Framework reconnaîtra que toutes les migrations sont appliquées
-- =========================================================================

-- Vérification : Afficher toutes les migrations appliquées
SELECT * FROM `__EFMigrationsHistory` ORDER BY `MigrationId`;

