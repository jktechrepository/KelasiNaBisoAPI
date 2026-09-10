-- ==========================================================
-- Script SQL idempotent pour appliquer les changements multi-devises
-- MySQL / MariaDB
-- Base de données de production
-- ==========================================================

START TRANSACTION;

-- 1) Ajouter la colonne CodeDevisePrincipale si elle n'existe pas
ALTER TABLE Ecoles
  ADD COLUMN IF NOT EXISTS CodeDevisePrincipale VARCHAR(10) NULL;

-- 2) Backfill des écoles existantes pour garantir une valeur de devise principale
--    Le backend utilise un fallback 'USD' si la colonne est vide, mais on remplit
--    explicitement afin d'avoir un état cohérent en base.
UPDATE Ecoles
SET CodeDevisePrincipale = 'USD'
WHERE CodeDevisePrincipale IS NULL
   OR TRIM(CodeDevisePrincipale) = '';

-- 3) Ajouter les snapshots de conversion sur les paiements
ALTER TABLE Paiements
  ADD COLUMN IF NOT EXISTS CodeDevisePaiement VARCHAR(10) NULL,
  ADD COLUMN IF NOT EXISTS CodeDevisePrincipale VARCHAR(10) NULL,
  ADD COLUMN IF NOT EXISTS TauxVersDevisePrincipale DECIMAL(18,8) NULL,
  ADD COLUMN IF NOT EXISTS MontantPayeDevisePrincipale DECIMAL(18,2) NULL;

-- 4) Créer la table des taux de change si elle n'existe pas
CREATE TABLE IF NOT EXISTS TauxChanges (
    IdTauxChange INT NOT NULL AUTO_INCREMENT,
    IdEcole INT NOT NULL,
    CodeDeviseSource VARCHAR(10) NOT NULL,
    CodeDeviseCible VARCHAR(10) NOT NULL,
    Taux DECIMAL(65,30) NOT NULL,
    DateEffet DATETIME(6) NOT NULL,
    Statut TINYINT(1) NOT NULL,
    DateCreation DATETIME(6) NOT NULL,
    PRIMARY KEY (IdTauxChange)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

COMMIT;

-- ==========================================================
-- Vérifications post-migration
-- ==========================================================

-- Vérifier la présence de la colonne
-- SHOW COLUMNS FROM Ecoles LIKE 'CodeDevisePrincipale';

-- Vérifier la présence de la table
-- SHOW TABLES LIKE 'TauxChanges';

-- Vérifier la population initiale
-- SELECT IdEcole, CodeDevisePrincipale FROM Ecoles ORDER BY IdEcole LIMIT 20;
