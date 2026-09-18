-- =============================================================================
-- KelasiNaBiso — Permissions rôle Eleve (comptes existants)
-- Fichier : docs/sql/20260917_AlignPermissionsRoleEleve_PayOwn_Notification.sql
-- Date    : 2026-09-17
-- SGBD    : MySQL 8 / MariaDB 10.11+ (phpMyAdmin-safe)
-- =============================================================================
--
-- Object
-- Les permissions sont attachées au RÔLE (RolePermissions), pas à chaque
-- ligne Utilisateurs. Tout compte déjà lié au rôle Eleve / Élève hérite
-- automatiquement. Reconnexion JWT recommandée après exécution.
--
-- Capacités couvertes :
--   1) Payer ses propres frais     → Paiement.PayOwn
--                                  (+ Frais.ReadOwn, Paiement.ReadOwn)
--   2) Voir ses bulletins          → Bulletin.ReadOwn
--   3) Voir ses notes              → Note.ReadOwn
--   4) Consulter / marquer notifs  → Notification.ReadOwn, Notification.UpdateOwn
--
-- Bonus conservé (hors 4 points stricts, déjà en prod V1) :
--   - Parent : PayOwn + notifications own (PayIn enfants)
--   - Admin / Super-Admin : Notification.ReadOwn / UpdateOwn
--
-- Idempotent : ré-exécutable sans doublon.
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- 0. Contrôle préalable : rôle Eleve présent
-- -----------------------------------------------------------------------------

SELECT `IdRole`, `Nom`, `Niveau`, `Statut`
FROM `Roles`
WHERE `Nom` IN ('Eleve', 'Élève');

-- -----------------------------------------------------------------------------
-- 1. Catalogue Permissions (idempotent)
-- -----------------------------------------------------------------------------

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Paiement.PayOwn', 'Paiement', 'PayOwn',
       'Payer ses propres frais (élève / parent)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Paiement.PayOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Paiement.ReadOwn', 'Paiement', 'ReadOwn',
       'Voir ses propres paiements (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Paiement.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Frais.ReadOwn', 'Frais', 'ReadOwn',
       'Voir ses propres frais / situation de paiement (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Frais.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.ReadOwn', 'Bulletin', 'ReadOwn',
       'Voir son propre bulletin (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Note.ReadOwn', 'Note', 'ReadOwn',
       'Voir ses propres notes (élève)', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Note.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Notification.ReadOwn', 'Notification', 'ReadOwn',
       'Consulter ses propres notifications', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Notification.ReadOwn');

INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Notification.UpdateOwn', 'Notification', 'UpdateOwn',
       'Marquer ses notifications comme lues', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Notification.UpdateOwn');

-- -----------------------------------------------------------------------------
-- 2. Assignation au rôle Eleve / Élève (liste explicite)
-- -----------------------------------------------------------------------------

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Eleve', 'Élève')
  AND p.`Nom` IN (
      'Paiement.PayOwn',
      'Paiement.ReadOwn',
      'Frais.ReadOwn',
      'Bulletin.ReadOwn',
      'Note.ReadOwn',
      'Notification.ReadOwn',
      'Notification.UpdateOwn'
  )
  AND NOT EXISTS (
      SELECT 1 FROM `RolePermissions` rp
      WHERE rp.`IdRole` = r.`IdRole`
        AND rp.`IdPermission` = p.`IdPermission`
  );

-- -----------------------------------------------------------------------------
-- 3. Parent : PayOwn + notifications (PayIn enfants / inbox) — conservé
-- -----------------------------------------------------------------------------

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` = 'Parent'
  AND p.`Nom` IN ('Paiement.PayOwn', 'Notification.ReadOwn', 'Notification.UpdateOwn')
  AND NOT EXISTS (
      SELECT 1 FROM `RolePermissions` rp
      WHERE rp.`IdRole` = r.`IdRole`
        AND rp.`IdPermission` = p.`IdPermission`
  );

-- -----------------------------------------------------------------------------
-- 4. Admin / Super-Admin : Notification.ReadOwn / UpdateOwn — conservé
-- -----------------------------------------------------------------------------

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Admin', 'Super-Admin')
  AND p.`Nom` IN ('Notification.ReadOwn', 'Notification.UpdateOwn')
  AND NOT EXISTS (
      SELECT 1 FROM `RolePermissions` rp
      WHERE rp.`IdRole` = r.`IdRole`
        AND rp.`IdPermission` = p.`IdPermission`
  );

-- -----------------------------------------------------------------------------
-- 5. Vérifications
-- -----------------------------------------------------------------------------

-- 5.a Permissions du rôle Eleve (cibles)
SELECT p.`Nom`, p.`Categorie`, p.`Action`, rp.`DateAttribution`
FROM `RolePermissions` rp
INNER JOIN `Roles` r ON r.`IdRole` = rp.`IdRole`
INNER JOIN `Permissions` p ON p.`IdPermission` = rp.`IdPermission`
WHERE r.`Nom` IN ('Eleve', 'Élève')
  AND p.`Nom` IN (
      'Paiement.PayOwn',
      'Paiement.ReadOwn',
      'Frais.ReadOwn',
      'Bulletin.ReadOwn',
      'Note.ReadOwn',
      'Notification.ReadOwn',
      'Notification.UpdateOwn'
  )
ORDER BY p.`Categorie`, p.`Action`;

-- 5.b Comptes utilisateurs qui héritent via UserRoles
SELECT COUNT(DISTINCT ur.`IdUtilisateur`) AS NbUtilisateursViaUserRoles
FROM `UserRoles` ur
INNER JOIN `Roles` r ON r.`IdRole` = ur.`IdRole`
WHERE r.`Nom` IN ('Eleve', 'Élève');

-- 5.c Comptes utilisateurs qui héritent via Utilisateurs.IdRole (legacy)
SELECT COUNT(*) AS NbUtilisateursViaIdRole
FROM `Utilisateurs` u
INNER JOIN `Roles` r ON r.`IdRole` = u.`IdRole`
WHERE r.`Nom` IN ('Eleve', 'Élève');

-- =============================================================================
-- Fin
-- =============================================================================
