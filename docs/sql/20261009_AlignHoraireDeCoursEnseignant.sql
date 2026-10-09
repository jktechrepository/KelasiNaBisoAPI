-- =============================================================================
-- KelasiNaBiso — HoraireDeCours.Read pour le rôle Enseignant (phpMyAdmin-safe)
-- Fichier : docs/sql/20261009_AlignHoraireDeCoursEnseignant.sql
-- Date    : 2026-10-09
--
-- Idempotent. N'accorde pas Create / Update / Delete à Enseignant.
-- =============================================================================

SET NAMES utf8mb4;

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'HoraireDeCours.Read', 'HoraireDeCours', 'Read',
       'Voir les horaires de cours', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'HoraireDeCours.Read');

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` = 'Enseignant'
  AND p.`Nom` = 'HoraireDeCours.Read'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );

SELECT r.`Nom` AS `Role`, p.`Nom` AS `Permission`, p.`Action`
FROM `Roles` r
JOIN `RolePermissions` rp ON rp.`IdRole` = r.`IdRole`
JOIN `Permissions` p ON p.`IdPermission` = rp.`IdPermission`
WHERE r.`Nom` = 'Enseignant'
  AND p.`Categorie` = 'HoraireDeCours'
ORDER BY p.`Action`;
