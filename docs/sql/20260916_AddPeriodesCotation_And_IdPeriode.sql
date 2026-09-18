-- =============================================================================
-- KelasiNaBiso — PeriodesCotation + Evaluation.IdPeriode (phpMyAdmin / MySQL)
-- Fichier : docs/sql/20260916_AddPeriodesCotation_And_IdPeriode.sql
-- Date    : 2026-09-16
-- SGBD    : MySQL 8 / MariaDB 10.11+
-- =============================================================================
--
-- Contenu :
--   1) Table PeriodesCotation + seed T1/T2/T3
--   2) Colonne Evaluations.IdPeriode + index + FK
--   3) Backfill alias → IdPeriode + normalisation Periode = Libelle
--   4) Entrée __EFMigrationsHistory
--
-- PRÉREQUIS : backup complet avant exécution.
-- IDEMPOTENCE partielle : ignorer #1050/#1060/#1061/#1826 si déjà appliqué.
-- =============================================================================

SET NAMES utf8mb4;



-- -----------------------------------------------------------------------------
-- 1. Table PeriodesCotation (ignorer #1050 si existe)
-- -----------------------------------------------------------------------------
CREATE TABLE `periodescotation` (
  `IdPeriode` INT NOT NULL AUTO_INCREMENT,
  `Code` VARCHAR(20) NOT NULL,
  `Libelle` VARCHAR(100) NOT NULL,
  `Ordre` INT NOT NULL,
  `Statut` TINYINT(1) NOT NULL,
  `DateCreation` DATETIME(6) NOT NULL,
  PRIMARY KEY (`IdPeriode`),
  UNIQUE KEY `IX_PeriodesCotation_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed (idempotent)
INSERT INTO `periodescotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T1', 'Trimestre 1', 1, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `periodescotation` WHERE `Code` = 'T1');

INSERT INTO `periodescotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T2', 'Trimestre 2', 2, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `periodescotation` WHERE `Code` = 'T2');

INSERT INTO `periodescotation` (`Code`, `Libelle`, `Ordre`, `Statut`, `DateCreation`)
SELECT 'T3', 'Trimestre 3', 3, 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM `periodescotation` WHERE `Code` = 'T3');

-- -----------------------------------------------------------------------------
-- 2. Colonne IdPeriode sur Evaluations
-- -----------------------------------------------------------------------------
-- 2.a Colonne (ignorer #1060)
ALTER TABLE `evaluations`
  ADD COLUMN `IdPeriode` INT NULL;

-- 2.b Index (ignorer #1061)
CREATE INDEX `IX_Evaluations_IdPeriode`
  ON `evaluations` (`IdPeriode`);

-- 2.c FK (ignorer #1826 / #121)
ALTER TABLE `evaluations`
  ADD CONSTRAINT `FK_Evaluations_PeriodesCotation_IdPeriode`
  FOREIGN KEY (`IdPeriode`) REFERENCES `periodescotation` (`IdPeriode`)
  ON DELETE RESTRICT;

-- -----------------------------------------------------------------------------
-- 3. Backfill alias → IdPeriode
-- -----------------------------------------------------------------------------
UPDATE `evaluations` e
INNER JOIN `periodescotation` p ON p.`Code` = 'T1'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't1', 'trimestre 1', '1er trimestre', '1ère trimestre', '1ere trimestre',
    'premier trimestre', '1 trimestre', 'trim 1', '1er trim', '1ère trim', '1ere trim', '1e trimestre'
  );

UPDATE `evaluations` e
INNER JOIN `periodescotation` p ON p.`Code` = 'T2'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't2', 'trimestre 2', '2ème trimestre', '2eme trimestre', 'deuxieme trimestre',
    'deuxième trimestre', '2 trimestre', 'trim 2', '2e trimestre', '2eme trim'
  );

UPDATE `evaluations` e
INNER JOIN `periodescotation` p ON p.`Code` = 'T3'
SET e.`IdPeriode` = p.`IdPeriode`, e.`Periode` = p.`Libelle`
WHERE e.`IdPeriode` IS NULL
  AND e.`Periode` IS NOT NULL
  AND LOWER(TRIM(e.`Periode`)) IN (
    't3', 'trimestre 3', '3ème trimestre', '3eme trimestre', 'troisieme trimestre',
    'troisième trimestre', '3 trimestre', 'trim 3', '3e trimestre', '3eme trim'
  );




