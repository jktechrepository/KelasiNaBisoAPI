-- =============================================================================
-- KelasiNaBiso — BulletinDecisions + Bulletin.Update (phpMyAdmin-safe)
-- Fichier : docs/sql/20260916_AddBulletinDecisions.sql
-- Date    : 2026-09-16
-- =============================================================================
--
-- Contenu :
--   1) Table BulletinDecisions (unique Eleve+Annee+Periode)
--   2) Permission Bulletin.Update + assignation rôles
--   3) Entrée __EFMigrationsHistory
--
-- Prérequis : PeriodesCotation déjà créée.
-- IDEMPOTENCE : ignorer #1050 / #1061 / #1826 si déjà appliqué.
-- =============================================================================

SET NAMES utf8mb4;

-- 1. Table (ignorer #1050 si existe)
CREATE TABLE `bulletindecisions` (
  `IdBulletinDecision` INT NOT NULL AUTO_INCREMENT,
  `IdEleve` INT NOT NULL,
  `IdAnneeScolaire` INT NOT NULL,
  `IdPeriode` INT NOT NULL,
  `Decision` VARCHAR(100) NULL,
  `AppreciationGenerale` VARCHAR(1000) NULL,
  `IdAuteur` INT NULL,
  `DateCreation` DATETIME(6) NOT NULL,
  `DateModification` DATETIME(6) NULL,
  PRIMARY KEY (`IdBulletinDecision`),
  UNIQUE KEY `IX_BulletinDecisions_Eleve_Annee_Periode` (`IdEleve`, `IdAnneeScolaire`, `IdPeriode`),
  KEY `IX_BulletinDecisions_IdAnneeScolaire` (`IdAnneeScolaire`),
  KEY `IX_BulletinDecisions_IdPeriode` (`IdPeriode`),
  CONSTRAINT `FK_BulletinDecisions_Eleves_IdEleve`
    FOREIGN KEY (`IdEleve`) REFERENCES `eleves` (`IdEleve`) ON DELETE RESTRICT,
  CONSTRAINT `FK_BulletinDecisions_AnneeScolaires_IdAnneeScolaire`
    FOREIGN KEY (`IdAnneeScolaire`) REFERENCES `anneescolaires` (`IdAnneeScolaire`) ON DELETE RESTRICT,
  CONSTRAINT `FK_BulletinDecisions_PeriodesCotation_IdPeriode`
    FOREIGN KEY (`IdPeriode`) REFERENCES `periodescotation` (`IdPeriode`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Permission Bulletin.Update
INSERT INTO `Permissions` (`Nom`, `Categorie`, `Action`, `Description`, `Statut`, `DateCreation`)
SELECT 'Bulletin.Update', 'Bulletin', 'Update',
       'Saisir décision / appréciation de bulletin', 1, UTC_TIMESTAMP(6)
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `Permissions` WHERE `Nom` = 'Bulletin.Update');

INSERT INTO `RolePermissions` (`IdRole`, `IdPermission`, `DateAttribution`, `IdUtilisateurAttribution`)
SELECT r.`IdRole`, p.`IdPermission`, UTC_TIMESTAMP(6), NULL
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.`Nom` IN ('Super-Admin', 'Admin', 'Directeur', 'Sous-Directeur', 'Enseignant')
  AND p.`Nom` = 'Bulletin.Update'
  AND NOT EXISTS (
    SELECT 1 FROM `RolePermissions` rp
    WHERE rp.`IdRole` = r.`IdRole` AND rp.`IdPermission` = p.`IdPermission`
  );


