-- =============================================================================
-- KelasiNaBiso — Correction Dashboard production (knb_db)
-- Erreur cible : Unknown column 'p.MontantCollecte' in 'SELECT'
--
-- Compatible compte restreint (ex. kelasiUser) : PAS de information_schema.
-- Utilise une procédure temporaire (ignore erreur 1060 = colonne déjà présente).
-- Fonctionne MySQL 5.7+ / MariaDB 10.x.
--
-- Exécuter dans phpMyAdmin sur la base knb_db (prod).
-- =============================================================================

USE knb_db;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- =============================================================================
-- PARTIE A — Colonnes manquantes (ignore si déjà présentes)
-- =============================================================================

DROP PROCEDURE IF EXISTS knb_dashboard_add_columns;

DELIMITER $$
CREATE PROCEDURE knb_dashboard_add_columns()
BEGIN
    DECLARE CONTINUE HANDLER FOR 1060 BEGIN END;

    ALTER TABLE `Paiements` ADD COLUMN `MontantNet` DECIMAL(18,2) NULL AFTER `Montant`;
    ALTER TABLE `Paiements` ADD COLUMN `MontantCollecte` DECIMAL(18,2) NULL AFTER `MontantNet`;
    ALTER TABLE `Paiements` ADD COLUMN `OperateurMobileMoney` VARCHAR(20) NULL AFTER `ModePaiement`;
    ALTER TABLE `Frais` ADD COLUMN `IdAnneeScolaire` INT NULL;
    ALTER TABLE `Frais` ADD COLUMN `IdClasse` INT NULL;
END$$
DELIMITER ;

CALL knb_dashboard_add_columns();
DROP PROCEDURE knb_dashboard_add_columns;

-- =============================================================================
-- PARTIE B — MOKO Afrika (migration 20260704123906 — tables)
-- =============================================================================
CREATE TABLE IF NOT EXISTS `EcolesInfoPaiementMobile` (
    `IdEcoleInfoPaiementMobile` INT NOT NULL AUTO_INCREMENT,
    `IdEcole` INT NOT NULL,
    `MobileMoneyActif` TINYINT(1) NOT NULL DEFAULT 0,
    `CarteActif` TINYINT(1) NOT NULL DEFAULT 0,
    `Devise` VARCHAR(10) NOT NULL DEFAULT 'CDF',
    `DelaiReglementMinutes` INT NOT NULL DEFAULT 3,
    `PayoutAutomatique` TINYINT(1) NOT NULL DEFAULT 1,
    `Statut` TINYINT(1) NOT NULL DEFAULT 1,
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` DATETIME(6) NULL,
    PRIMARY KEY (`IdEcoleInfoPaiementMobile`),
    UNIQUE KEY `IX_EcoleInfoPaiementMobile_IdEcole_Unique` (`IdEcole`),
    CONSTRAINT `FK_EcolesInfoPaiementMobile_Ecoles_IdEcole`
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `EcolesBeneficiairesMomo` (
    `IdEcoleBeneficiaireMomo` INT NOT NULL AUTO_INCREMENT,
    `IdEcoleInfoPaiementMobile` INT NOT NULL,
    `IdEcole` INT NOT NULL,
    `Methode` VARCHAR(20) NOT NULL COMMENT 'airtel|orange|mpesa|africell|card',
    `Numero` VARCHAR(20) NOT NULL,
    `NomTitulaire` VARCHAR(150) NULL,
    `EstPrincipal` TINYINT(1) NOT NULL DEFAULT 0,
    `Statut` VARCHAR(20) NOT NULL DEFAULT 'ACTIF',
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` DATETIME(6) NULL,
    PRIMARY KEY (`IdEcoleBeneficiaireMomo`),
    UNIQUE KEY `IX_EcoleBeneficiaireMomo_Ecole_Methode_Numero` (`IdEcole`, `Methode`, `Numero`),
    KEY `IX_EcolesBeneficiairesMomo_IdEcoleInfoPaiementMobile` (`IdEcoleInfoPaiementMobile`),
    CONSTRAINT `FK_EcolesBeneficiairesMomo_Ecoles_IdEcole`
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE,
    CONSTRAINT `FK_EcolesBeneficiairesMomo_InfoPaiementMobile`
        FOREIGN KEY (`IdEcoleInfoPaiementMobile`) REFERENCES `EcolesInfoPaiementMobile` (`IdEcoleInfoPaiementMobile`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `EcolesWallets` (
    `IdEcoleWallet` INT NOT NULL AUTO_INCREMENT,
    `IdEcole` INT NOT NULL,
    `IdEcoleInfoPaiementMobile` INT NOT NULL,
    `Devise` VARCHAR(10) NOT NULL DEFAULT 'CDF',
    `SoldeEnAttente` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `SoldeDisponible` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalRecu` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalReverse` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` DATETIME(6) NULL,
    PRIMARY KEY (`IdEcoleWallet`),
    UNIQUE KEY `IX_EcoleWallet_IdEcole_Unique` (`IdEcole`),
    UNIQUE KEY `IX_EcolesWallets_IdEcoleInfoPaiementMobile` (`IdEcoleInfoPaiementMobile`),
    CONSTRAINT `FK_EcolesWallets_Ecoles_IdEcole`
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE CASCADE,
    CONSTRAINT `FK_EcolesWallets_InfoPaiementMobile`
        FOREIGN KEY (`IdEcoleInfoPaiementMobile`) REFERENCES `EcolesInfoPaiementMobile` (`IdEcoleInfoPaiementMobile`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `TransactionsMoko` (
    `IdTransactionMoko` INT NOT NULL AUTO_INCREMENT,
    `Reference` VARCHAR(50) NOT NULL,
    `ParentReference` VARCHAR(50) NULL,
    `IdPaiement` INT NULL,
    `IdEcole` INT NOT NULL,
    `Action` VARCHAR(10) NOT NULL COMMENT 'debit|credit|check',
    `Amount` DECIMAL(18,2) NOT NULL,
    `AmountNet` DECIMAL(18,2) NULL,
    `FraisCollecte` DECIMAL(18,2) NULL,
    `FraisDecaissement` DECIMAL(18,2) NULL,
    `Devise` VARCHAR(10) NOT NULL DEFAULT 'CDF',
    `CustomerPhone` VARCHAR(20) NULL,
    `Method` VARCHAR(20) NULL,
    `Status` VARCHAR(30) NOT NULL DEFAULT 'pending',
    `GatewayTransactionId` VARCHAR(100) NULL,
    `StatusDescription` VARCHAR(500) NULL,
    `RawRequest` LONGTEXT NULL,
    `RawResponse` LONGTEXT NULL,
    `RawCallback` LONGTEXT NULL,
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` DATETIME(6) NULL,
    PRIMARY KEY (`IdTransactionMoko`),
    UNIQUE KEY `IX_TransactionMoko_Reference_Unique` (`Reference`),
    KEY `IX_TransactionMoko_IdPaiement` (`IdPaiement`),
    KEY `IX_TransactionMoko_ParentReference` (`ParentReference`),
    KEY `IX_TransactionsMoko_IdEcole` (`IdEcole`),
    CONSTRAINT `FK_TransactionsMoko_Ecoles_IdEcole`
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TransactionsMoko_Paiements_IdPaiement`
        FOREIGN KEY (`IdPaiement`) REFERENCES `Paiements` (`IdPaiement`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `FilePayoutsMoko` (
    `IdFilePayoutMoko` INT NOT NULL AUTO_INCREMENT,
    `PayInReference` VARCHAR(50) NOT NULL,
    `IdTransactionMokoPayIn` INT NULL,
    `IdTransactionMokoPayOut` INT NULL,
    `IdEcole` INT NOT NULL,
    `IdPaiement` INT NULL,
    `MontantNet` DECIMAL(18,2) NOT NULL,
    `Devise` VARCHAR(10) NOT NULL DEFAULT 'CDF',
    `Methode` VARCHAR(20) NULL,
    `NumeroBeneficiaire` VARCHAR(20) NULL,
    `ScheduledAt` DATETIME(6) NOT NULL,
    `PayOutReference` VARCHAR(50) NULL,
    `Status` VARCHAR(30) NOT NULL DEFAULT 'pending',
    `RetryCount` INT NOT NULL DEFAULT 0,
    `ErrorMessage` VARCHAR(1000) NULL,
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` DATETIME(6) NULL,
    `DateTraitement` DATETIME(6) NULL,
    PRIMARY KEY (`IdFilePayoutMoko`),
    KEY `IX_FilePayoutMoko_PayInReference` (`PayInReference`),
    KEY `IX_FilePayoutMoko_Status_ScheduledAt` (`Status`, `ScheduledAt`),
    KEY `IX_FilePayoutsMoko_IdEcole` (`IdEcole`),
    KEY `IX_FilePayoutsMoko_IdPaiement` (`IdPaiement`),
    KEY `IX_FilePayoutsMoko_IdTransactionMokoPayIn` (`IdTransactionMokoPayIn`),
    KEY `IX_FilePayoutsMoko_IdTransactionMokoPayOut` (`IdTransactionMokoPayOut`),
    CONSTRAINT `FK_FilePayoutsMoko_Ecoles_IdEcole`
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) ON DELETE RESTRICT,
    CONSTRAINT `FK_FilePayoutsMoko_Paiements_IdPaiement`
        FOREIGN KEY (`IdPaiement`) REFERENCES `Paiements` (`IdPaiement`) ON DELETE SET NULL,
    CONSTRAINT `FK_FilePayoutsMoko_TransactionsMoko_PayIn`
        FOREIGN KEY (`IdTransactionMokoPayIn`) REFERENCES `TransactionsMoko` (`IdTransactionMoko`) ON DELETE SET NULL,
    CONSTRAINT `FK_FilePayoutsMoko_TransactionsMoko_PayOut`
        FOREIGN KEY (`IdTransactionMokoPayOut`) REFERENCES `TransactionsMoko` (`IdTransactionMoko`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `EcolesWalletMouvements` (
    `IdEcoleWalletMouvement` INT NOT NULL AUTO_INCREMENT,
    `IdEcoleWallet` INT NOT NULL,
    `IdEcole` INT NOT NULL,
    `IdTransactionMoko` INT NULL,
    `IdPaiement` INT NULL,
    `TypeMouvement` VARCHAR(50) NOT NULL,
    `Montant` DECIMAL(18,2) NOT NULL,
    `SoldeEnAttenteApres` DECIMAL(18,2) NOT NULL,
    `SoldeDisponibleApres` DECIMAL(18,2) NOT NULL,
    `Reference` VARCHAR(100) NULL,
    `Commentaire` VARCHAR(500) NULL,
    `DateCreation` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`IdEcoleWalletMouvement`),
    KEY `IX_EcolesWalletMouvements_IdEcoleWallet` (`IdEcoleWallet`),
    KEY `IX_EcoleWalletMouvement_IdEcole` (`IdEcole`),
    KEY `IX_EcolesWalletMouvements_IdPaiement` (`IdPaiement`),
    KEY `IX_EcolesWalletMouvements_IdTransactionMoko` (`IdTransactionMoko`),
    CONSTRAINT `FK_EcolesWalletMouvements_Wallet`
        FOREIGN KEY (`IdEcoleWallet`) REFERENCES `EcolesWallets` (`IdEcoleWallet`) ON DELETE CASCADE,
    CONSTRAINT `FK_EcolesWalletMouvements_Paiement`
        FOREIGN KEY (`IdPaiement`) REFERENCES `Paiements` (`IdPaiement`) ON DELETE SET NULL,
    CONSTRAINT `FK_EcolesWalletMouvements_TransactionMoko`
        FOREIGN KEY (`IdTransactionMoko`) REFERENCES `TransactionsMoko` (`IdTransactionMoko`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260704123906_AddMokoAfrikaPaymentEntities', '6.0.25'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260704123906_AddMokoAfrikaPaymentEntities'
);

-- =============================================================================
-- PARTIE C — Frais.IdAnneeScolaire backfill (migration 20260806120000)
-- =============================================================================

UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
      AND a.DateDebut <= NOW()
      AND a.DateFin >= NOW()
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;

UPDATE Frais f
INNER JOIN Directions d ON d.IdDirection = f.IdDirection
SET f.IdAnneeScolaire = (
    SELECT a.IdAnneeScolaire
    FROM AnneeScolaires a
    WHERE a.IdEcole = d.IdEcole
      AND a.Statut = 1
    ORDER BY a.DateDebut DESC
    LIMIT 1
)
WHERE f.IdAnneeScolaire IS NULL;

-- NOT NULL uniquement si aucun frais orphelin (sinon ignorer cette ligne)
-- ALTER TABLE `Frais` MODIFY COLUMN `IdAnneeScolaire` INT NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260806120000_AddFraisAnneeScolaireAndClasse', '6.0.25'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260806120000_AddFraisAnneeScolaireAndClasse'
);

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================================================
-- PARTIE D — Portée Frais (IdEcole + jonctions) : Scripts/BACKFILL_FRAIS_PORTEE.sql
-- Migration EF : 20260903120000_FraisPorteeDirectionsClasses
-- =============================================================================

-- =============================================================================
-- FIN — Relancer PRODUCTION_DASHBOARD_VERIFY_knb_db.sql pour contrôle
-- =============================================================================
