-- =============================================================================
-- KelasiNaBiso — Script production lot Élève (compatible phpMyAdmin)
-- Fichier : docs/sql/20260915_AddIdEleve_And_RoleEleve.sql
-- Date    : 2026-09-15 (corrigé : sans PREPARE / SQL dynamique)
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
--
-- Contenu :
--   1) Colonne utilisateurs.IdEleve (nullable) + index unique + FK eleves
--   2) Alignement rôle : 'Élève' -> 'Eleve' (ou INSERT si absent)
--   3) Entrée __EFMigrationsHistory
--
-- PRÉREQUIS : backup complet avant exécution.
--
-- IDEMPOTENCE :
--   - Étapes déjà appliquées : ignorer l'erreur MySQL indiquée en commentaire
--     (1060 colonne, 1061 index, 1826/121 FK déjà existante)
--   - Rôle / history : réellement idempotents (UPDATE/INSERT conditionnels)
--
-- Backfill comptes : docs/sql/20260915_BackfillComptesEleve.sql
--   (alt. API : POST /api/Eleve/backfill-comptes?idEcole=…&dryRun=false)
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- 0. Contrôles (lecture seule)
-- -----------------------------------------------------------------------------



-- -----------------------------------------------------------------------------
-- 1. SCHÉMA : IdEleve + index + FK
--    Tables en minuscules (convention serveur / MySQL macOS).
--    Si votre prod utilise Utilisateurs/Eleves, remplacez les noms ci-dessous.
-- -----------------------------------------------------------------------------

-- 1.a Colonne (ignorer #1060 Duplicate column name si déjà présente)
ALTER TABLE `utilisateurs`
  ADD COLUMN `IdEleve` INT NULL;

-- 1.b Index unique (ignorer #1061 Duplicate key name si déjà présent)
CREATE UNIQUE INDEX `IX_Utilisateurs_IdEleve_Unique`
  ON `utilisateurs` (`IdEleve`);

-- 1.c FK (ignorer si contrainte déjà existante : #1826 / #121 / #1005)
ALTER TABLE `utilisateurs`
  ADD CONSTRAINT `FK_Utilisateurs_Eleves_IdEleve`
  FOREIGN KEY (`IdEleve`) REFERENCES `eleves` (`IdEleve`)
  ON DELETE RESTRICT;

-- -----------------------------------------------------------------------------
-- 2. RÔLE Eleve (idempotent)
-- -----------------------------------------------------------------------------

-- 2.a Renommer l'éventuel libellé accentué
UPDATE `Roles`
SET `Nom` = 'Eleve',
    `Description` = COALESCE(NULLIF(`Description`, ''), 'Élève')
WHERE `Nom` = 'Élève';

-- 2.b Créer le rôle s'il n'existe pas
INSERT INTO `Roles` (`Nom`, `Description`, `Niveau`, `Statut`, `DateCreation`)
SELECT 'Eleve', 'Élève', 6, 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM `Roles` WHERE `Nom` = 'Eleve'
);





SELECT `IdRole`, `Nom`, `Niveau`, `Statut`
FROM `Roles`
WHERE `Nom` IN ('Eleve', 'Élève');

