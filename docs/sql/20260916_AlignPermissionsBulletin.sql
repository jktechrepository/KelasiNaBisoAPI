-- =============================================================================
-- KelasiNaBiso — Permissions Bulletin.* (phpMyAdmin-safe)
-- Fichier : docs/sql/20260916_AlignPermissionsBulletin.sql
-- Date    : 2026-09-16
-- =============================================================================

SET NAMES utf8mb4;

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.Read', 'Bulletin', 'Read',
       'Voir les bulletins (classe / école)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.Read');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.ReadOwn', 'Bulletin', 'ReadOwn',
       'Voir son propre bulletin (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.ReadChildren', 'Bulletin', 'ReadChildren',
       'Voir les bulletins de ses enfants (parent)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.ReadChildren');

-- Super-Admin / Admin / Directeur : toutes Bulletin.*
INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Super-Admin', 'Admin', 'Directeur')
  AND p.`Categorie` = 'Bulletin'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );

-- Enseignant / Sous-Directeur : Bulletin.Read
INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Enseignant', 'Sous-Directeur')
  AND p.`Nom` = 'Bulletin.Read'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );

-- Parent : Bulletin.ReadChildren
INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` = 'Parent'
  AND p.`Nom` = 'Bulletin.ReadChildren'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );

-- Eleve : Bulletin.ReadOwn
INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Eleve', 'Élève')
  AND p.`Nom` = 'Bulletin.ReadOwn'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );

SELECT p.`Nom`, r.`Nom` AS Role
FROM `RolePermissions` rp
JOIN `Permissions` p ON p.`IdPermission` = rp.`IdPermission`
JOIN `Roles` r ON r.`IdRole` = rp.`IdRole`
WHERE p.`Categorie` = 'Bulletin'
ORDER BY p.`Nom`, r.`Nom`;
