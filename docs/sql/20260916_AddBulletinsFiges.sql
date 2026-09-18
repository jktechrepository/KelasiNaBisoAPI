-- =============================================================================
-- KelasiNaBiso — BulletinsFiges + Bulletin.Unlock (phpMyAdmin-safe)
-- Fichier : docs/sql/20260916_AddBulletinsFiges.sql
-- Date    : 2026-09-16 (corrigé 2026-09-17 : FK Errcode 150 / casse tables)
-- =============================================================================
--
-- Contenu :
--   1) Table bulletinsfiges (unique Eleve+Annee+Periode)
--   2) Permission Bulletin.Unlock + assignation rôles direction
--   3) Entrée __EFMigrationsHistory
--
-- Prérequis :
--   - eleves, anneescolaires existent
--   - periodescotation déjà créée
--     → docs/sql/20260916_AddPeriodesCotation_And_IdPeriode.sql
--
-- IDEMPOTENCE : ignorer #1050 / #1061 / #1826 si déjà appliqué.
-- Tables en minuscules (convention serveur / MariaDB) — comme AddBulletinDecisions.
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- 0. Contrôles (lecture seule) — les 3 parentes doivent apparaître
--    Si periodescotation absente : exécuter d'abord AddPeriodesCotation…
-- -----------------------------------------------------------------------------


-- -----------------------------------------------------------------------------
-- 1. Table (ignorer #1050 si existe)
-- -----------------------------------------------------------------------------
CREATE TABLE `bulletinsfiges` (
  `IdBulletinFige` INT NOT NULL AUTO_INCREMENT,
  `IdEleve` INT NOT NULL,
  `IdAnneeScolaire` INT NOT NULL,
  `IdPeriode` INT NOT NULL,
  `PayloadJson` LONGTEXT CHARACTER SET utf8mb4 NOT NULL,
  `MoyenneGenerale` DOUBLE NULL,
  `Rang` INT NULL,
  `EffectifClasse` INT NOT NULL,
  `Decision` VARCHAR(100) CHARACTER SET utf8mb4 NULL,
  `AppreciationGenerale` VARCHAR(1000) CHARACTER SET utf8mb4 NULL,
  `IdAuteurValidation` INT NULL,
  `DateValidation` DATETIME(6) NOT NULL,
  PRIMARY KEY (`IdBulletinFige`),
  UNIQUE KEY `IX_BulletinsFiges_Eleve_Annee_Periode` (`IdEleve`, `IdAnneeScolaire`, `IdPeriode`),
  KEY `IX_BulletinsFiges_IdAnneeScolaire` (`IdAnneeScolaire`),
  KEY `IX_BulletinsFiges_IdPeriode` (`IdPeriode`),
  CONSTRAINT `FK_BulletinsFiges_Eleves_IdEleve`
    FOREIGN KEY (`IdEleve`) REFERENCES `eleves` (`IdEleve`) ON DELETE RESTRICT,
  CONSTRAINT `FK_BulletinsFiges_AnneeScolaires_IdAnneeScolaire`
    FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `anneescolaires` (`IdAnneeScolaire`) ON DELETE RESTRICT,
  CONSTRAINT `FK_BulletinsFiges_PeriodesCotation_IdPeriode`
    FOREIGN KEY (`IdPeriode`) REFERENCES `periodescotation` (`IdPeriode`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------------
-- 2. Permission Bulletin.Unlock (direction)
-- -----------------------------------------------------------------------------
INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.Unlock', 'Bulletin', 'Unlock',
       'Déverrouiller un bulletin figé', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.Unlock');

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE p.`Nom` = 'Bulletin.Unlock'
  AND r.`Nom` IN ('Admin', 'Directeur', 'Sous-Directeur', 'Super-Admin')
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );


