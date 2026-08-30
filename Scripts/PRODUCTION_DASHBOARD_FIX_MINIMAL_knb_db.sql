-- =============================================================================
-- KelasiNaBiso — Correction MINIMALE Dashboard (knb_db)
-- Si le script complet échoue (procédure refusée), exécuter ces lignes
-- UNE PAR UNE dans phpMyAdmin. Ignorer l'erreur #1060 (colonne déjà présente).
-- =============================================================================

USE knb_db;

ALTER TABLE `Paiements` ADD COLUMN `MontantNet` DECIMAL(18,2) NULL AFTER `Montant`;
ALTER TABLE `Paiements` ADD COLUMN `MontantCollecte` DECIMAL(18,2) NULL AFTER `MontantNet`;
ALTER TABLE `Paiements` ADD COLUMN `OperateurMobileMoney` VARCHAR(20) NULL AFTER `ModePaiement`;

-- Optionnel (dashboard filtre les frais par année) :
-- ALTER TABLE `Frais` ADD COLUMN `IdAnneeScolaire` INT NULL;
-- ALTER TABLE `Frais` ADD COLUMN `IdClasse` INT NULL;
