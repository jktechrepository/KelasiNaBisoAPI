-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Création de la table PaiementCrashed
-- Date : 2024-12-04
-- Description : 
--   Crée la table PaiementCrashed pour stocker les paiements échoués lors du bulk insert Excel
--   Permet de les corriger et de les réinjecter ultérieurement
-- ============================================================
-- ⚠️ IMPORTANT : Exécuter ce script pendant une fenêtre de maintenance
-- ⚠️ IMPORTANT : Faire une sauvegarde de la base de données avant l'exécution
-- ============================================================

USE `dev-knb_db`;

-- ============================================================
-- 1. Vérifier si la table existe déjà
-- ============================================================

SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'PaiementsCrashed';

-- ============================================================
-- 2. Créer la table PaiementsCrashed
-- ============================================================

CREATE TABLE IF NOT EXISTS `PaiementsCrashed` (
    `IdPaiementCrashed` INT NOT NULL AUTO_INCREMENT,
    
    -- Données du paiement
    `DatePaiement` DATETIME NULL,
    `Montant` DECIMAL(18, 2) NULL,
    `Devise` VARCHAR(10) NULL DEFAULT 'USD',
    `ModePaiement` VARCHAR(50) NULL,
    `Statut` TINYINT(1) NULL DEFAULT 1,
    `StatutPaiement` VARCHAR(50) NULL DEFAULT 'Confirmé',
    `ReferenceTransaction` VARCHAR(200) NULL,
    `JustificatifUrl` VARCHAR(1000) NULL,
    `Commentaire` VARCHAR(1000) NULL,
    
    -- IDs (peuvent être null si la recherche a échoué)
    `IdEleve` INT NULL,
    `IdFrais` INT NULL,
    `IdUtilisateur` INT NULL,
    
    -- Données brutes du fichier Excel
    `NomCompletEleve` VARCHAR(200) NULL,
    `LibelleFrais` VARCHAR(200) NULL,
    
    -- Informations sur l'erreur
    `ErreursJson` TEXT NOT NULL DEFAULT '[]',
    `NumeroLigne` INT NOT NULL,
    
    -- Métadonnées
    `IdEcole` INT NOT NULL,
    `NomFichierOriginal` VARCHAR(500) NULL,
    `DateEchec` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `DateCorrection` DATETIME NULL,
    `DateReinjection` DATETIME NULL,
    `IdPaiementCree` INT NULL,
    `EstResolu` TINYINT(1) NOT NULL DEFAULT 0,
    
    -- Attributs Techniques
    `DateCreation` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `DateModification` DATETIME NULL,
    
    PRIMARY KEY (`IdPaiementCrashed`),
    INDEX `IX_PaiementsCrashed_IdEcole` (`IdEcole`),
    INDEX `IX_PaiementsCrashed_IdEleve` (`IdEleve`),
    INDEX `IX_PaiementsCrashed_IdFrais` (`IdFrais`),
    INDEX `IX_PaiementsCrashed_EstResolu` (`EstResolu`),
    INDEX `IX_PaiementsCrashed_DateEchec` (`DateEchec`),
    INDEX `IX_PaiementsCrashed_IdPaiementCree` (`IdPaiementCree`),
    
    CONSTRAINT `FK_PaiementsCrashed_Ecoles_IdEcole` 
        FOREIGN KEY (`IdEcole`) REFERENCES `Ecoles` (`IdEcole`) 
        ON DELETE NO ACTION,
    
    CONSTRAINT `FK_PaiementsCrashed_Eleves_IdEleve` 
        FOREIGN KEY (`IdEleve`) REFERENCES `Eleves` (`IdEleve`) 
        ON DELETE NO ACTION,
    
    CONSTRAINT `FK_PaiementsCrashed_Frais_IdFrais` 
        FOREIGN KEY (`IdFrais`) REFERENCES `Frais` (`IdFrais`) 
        ON DELETE NO ACTION,
    
    CONSTRAINT `FK_PaiementsCrashed_Utilisateurs_IdUtilisateur` 
        FOREIGN KEY (`IdUtilisateur`) REFERENCES `Utilisateurs` (`IdUtilisateur`) 
        ON DELETE NO ACTION,
    
    CONSTRAINT `FK_PaiementsCrashed_Paiements_IdPaiementCree` 
        FOREIGN KEY (`IdPaiementCree`) REFERENCES `Paiements` (`IdPaiement`) 
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================
-- 3. Vérification finale
-- ============================================================

SELECT 
    'Table créée avec succès' AS Status,
    COUNT(*) AS NombreColonnes
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'PaiementsCrashed';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'PaiementsCrashed'
ORDER BY ORDINAL_POSITION;

SELECT '✅ Migration terminée avec succès' AS Status;

