-- =============================================================================
-- KelasiNaBiso — Migration manuelle MOKO Afrika (PayIn / PayOut / Wallet)
-- Base : MySQL / MariaDB (utf8mb4)
-- Migration EF source : 20260704123906_AddMokoAfrikaPaymentEntities
--
-- IMPORTANT :
--   • Ce script couvre UNIQUEMENT l'intégration paiement MOKO.
--   • Il n'inclut PAS les autres changements de la même migration EF
--     (Notes/Evaluations/Eleves/DevoirsADomicile) — appliquez-les séparément
--     si votre base ne les a pas encore.
--   • Exécuter sur la base cible avec un compte ayant DDL.
--   • Script idempotent : vérifie l'existence des tables/colonnes avant création.
--
-- BASE DE DONNEES — choisir celle utilisée par l'API :
--   • local (appsettings.Development.json)          → dev-knb_db  ← défaut ci-dessous
--   • serveur prod (appsettings.json)               → knb_db
-- =============================================================================

-- ▼▼▼ MODIFIER ICI si nécessaire ▼▼▼
-- Le tiret dans dev-knb_db impose des backticks autour du nom de base.
USE `dev-knb_db`;
-- USE knb_db;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- -----------------------------------------------------------------------------
-- 1. Colonnes MOKO sur Paiements (si absentes)
-- -----------------------------------------------------------------------------
SET @db = DATABASE();

SELECT COUNT(*) INTO @col_exists FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Paiements' AND COLUMN_NAME = 'MontantNet';
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE `Paiements` ADD COLUMN `MontantNet` DECIMAL(18,2) NULL AFTER `Montant`',
    'SELECT ''MontantNet déjà présent'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT COUNT(*) INTO @col_exists FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Paiements' AND COLUMN_NAME = 'MontantCollecte';
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE `Paiements` ADD COLUMN `MontantCollecte` DECIMAL(18,2) NULL AFTER `MontantNet`',
    'SELECT ''MontantCollecte déjà présent'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT COUNT(*) INTO @col_exists FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Paiements' AND COLUMN_NAME = 'OperateurMobileMoney';
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE `Paiements` ADD COLUMN `OperateurMobileMoney` VARCHAR(20) NULL AFTER `ModePaiement`',
    'SELECT ''OperateurMobileMoney déjà présent'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 2. EcolesInfoPaiementMobile — config paiement par école (site MOKO)
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- 3. EcolesBeneficiairesMomo — numéros PayOut par opérateur
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- 4. EcolesWallets — wallet virtuel par école
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- 5. TransactionsMoko — audit gateway PayIn / PayOut / Check
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- 6. FilePayoutsMoko — file d'attente reversements PayOut
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- 7. EcolesWalletMouvements — journal wallet
-- -----------------------------------------------------------------------------
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

SET FOREIGN_KEY_CHECKS = 1;

-- -----------------------------------------------------------------------------
-- 8. (Optionnel) Enregistrer la migration EF si vous utilisez dotnet ef
-- -----------------------------------------------------------------------------
-- INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
-- SELECT '20260704123906_AddMokoAfrikaPaymentEntities', '6.0.25'
-- WHERE NOT EXISTS (
--     SELECT 1 FROM `__EFMigrationsHistory`
--     WHERE `MigrationId` = '20260704123906_AddMokoAfrikaPaymentEntities'
-- );

-- -----------------------------------------------------------------------------
-- 9. Vérification post-migration
-- -----------------------------------------------------------------------------
SELECT 'EcolesInfoPaiementMobile' AS TableName, COUNT(*) AS Nb FROM EcolesInfoPaiementMobile
UNION ALL SELECT 'EcolesBeneficiairesMomo', COUNT(*) FROM EcolesBeneficiairesMomo
UNION ALL SELECT 'EcolesWallets', COUNT(*) FROM EcolesWallets
UNION ALL SELECT 'TransactionsMoko', COUNT(*) FROM TransactionsMoko
UNION ALL SELECT 'FilePayoutsMoko', COUNT(*) FROM FilePayoutsMoko
UNION ALL SELECT 'EcolesWalletMouvements', COUNT(*) FROM EcolesWalletMouvements;

-- =============================================================================
-- FIN — Migration MOKO Afrika KelasiNaBiso
-- =============================================================================
