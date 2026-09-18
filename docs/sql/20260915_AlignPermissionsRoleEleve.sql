-- =============================================================================
-- KelasiNaBiso — Alignement permissions rôle Eleve (phpMyAdmin-safe)
-- Fichier : docs/sql/20260915_AlignPermissionsRoleEleve.sql
-- Date    : 2026-09-15
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
--
-- Contenu (idempotent) :
--   1) Catalogue DevoirADomicile.* si absent
--   2) Catalogue Frais.ReadOwn si absent
--   3) RolePermissions pour Eleve :
--        - *.ReadOwn (Eleve, Note, Paiement, Frais, …)
--        - DevoirADomicile.Read
--        - DevoirADomicile.Download
--
-- Prérequis : rôle 'Eleve' présent (script 20260915_AddIdEleve_And_RoleEleve.sql)
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- 0. Contrôles
-- -----------------------------------------------------------------------------

SELECT `IdRole`, `Nom`, `Niveau`, `Statut`
FROM `Roles`
WHERE `Nom` IN ('Eleve', 'Élève');

-- -----------------------------------------------------------------------------
-- 1. Catalogue DevoirADomicile (idempotent)
-- -----------------------------------------------------------------------------

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'DevoirADomicile.Create', 'DevoirADomicile', 'Create',
       'Créer et publier un devoir à domicile', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'DevoirADomicile.Create');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'DevoirADomicile.Read', 'DevoirADomicile', 'Read',
       'Voir et consulter les devoirs à domicile', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'DevoirADomicile.Read');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'DevoirADomicile.Update', 'DevoirADomicile', 'Update',
       'Modifier un devoir à domicile existant', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'DevoirADomicile.Update');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'DevoirADomicile.Delete', 'DevoirADomicile', 'Delete',
       'Supprimer un devoir à domicile', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'DevoirADomicile.Delete');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'DevoirADomicile.Download', 'DevoirADomicile', 'Download',
       'Télécharger le fichier PDF d''un devoir à domicile', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'DevoirADomicile.Download');

-- -----------------------------------------------------------------------------
-- 1.b Catalogue Frais.ReadOwn (idempotent)
-- -----------------------------------------------------------------------------

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Frais.ReadOwn', 'Frais', 'ReadOwn',
       'Voir ses propres frais / situation de paiement (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Frais.ReadOwn');

-- -----------------------------------------------------------------------------
-- 2. Assignation au rôle Eleve — ReadOwn
-- -----------------------------------------------------------------------------

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` = 'Eleve'
  AND p.`Action` = 'ReadOwn'
  AND NOT EXISTS (
      SELECT 1 FROM `RolePermissions` rp
      WHERE rp.`IdRole` = r.`IdRole`
        AND rp.`IdPermission` = p.`IdPermission`
  );

-- -----------------------------------------------------------------------------
-- 3. Assignation au rôle Eleve — DevoirADomicile Read / Download
-- -----------------------------------------------------------------------------

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` = 'Eleve'
  AND p.`Categorie` = 'DevoirADomicile'
  AND p.`Action` IN ('Read', 'Download')
  AND NOT EXISTS (
      SELECT 1 FROM `RolePermissions` rp
      WHERE rp.`IdRole` = r.`IdRole`
        AND rp.`IdPermission` = p.`IdPermission`
  );

-- -----------------------------------------------------------------------------
-- 4. Vérifications
-- -----------------------------------------------------------------------------

SELECT p.`Nom`, p.`Categorie`, p.`Action`, rp.`DateAttribution`
FROM `RolePermissions` rp
INNER JOIN `Roles` r ON r.`IdRole` = rp.`IdRole`
INNER JOIN `Permissions` p ON p.`IdPermission` = rp.`IdPermission`
WHERE r.`Nom` = 'Eleve'
ORDER BY p.`Categorie`, p.`Action`;

SELECT COUNT(*) AS NbPermissionsEleve
FROM `RolePermissions` rp
INNER JOIN `Roles` r ON r.`IdRole` = rp.`IdRole`
WHERE r.`Nom` = 'Eleve';

SELECT 'Alignement permissions rôle Eleve terminé' AS Status;
