-- ============================================================
-- Migration : Ajout TitreEvaluation et CoefficientDevoir
-- Date : 2024-01-15
-- Description : 
--   1. Ajoute le champ TitreEvaluation dans la table Evaluations
--   2. Ajoute le champ CoefficientDevoir dans la table DevoirsADomicile
-- ============================================================

USE KelasiNaBiso;

-- ============================================================
-- 1. Ajouter TitreEvaluation dans la table Evaluations
-- ============================================================

-- Vérifier si la colonne existe déjà
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'TitreEvaluation'
)
BEGIN
    ALTER TABLE Evaluations
    ADD TitreEvaluation NVARCHAR(500) NULL;
    
    PRINT '✅ Colonne TitreEvaluation ajoutée à la table Evaluations';
END
ELSE
BEGIN
    PRINT '⚠️ La colonne TitreEvaluation existe déjà dans la table Evaluations';
END
GO

-- ============================================================
-- 2. Ajouter CoefficientDevoir dans la table DevoirsADomicile
-- ============================================================

-- Vérifier si la colonne existe déjà
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'DevoirsADomicile' 
    AND COLUMN_NAME = 'CoefficientDevoir'
)
BEGIN
    ALTER TABLE DevoirsADomicile
    ADD CoefficientDevoir INT NOT NULL DEFAULT 1;
    
    PRINT '✅ Colonne CoefficientDevoir ajoutée à la table DevoirsADomicile';
END
ELSE
BEGIN
    PRINT '⚠️ La colonne CoefficientDevoir existe déjà dans la table DevoirsADomicile';
END
GO

-- ============================================================
-- 3. Mettre à jour les devoirs existants avec un coefficient par défaut
-- ============================================================

UPDATE DevoirsADomicile
SET CoefficientDevoir = 1
WHERE CoefficientDevoir IS NULL OR CoefficientDevoir = 0;

PRINT '✅ Coefficients par défaut mis à jour pour les devoirs existants';
GO

-- ============================================================
-- 4. Vérification finale
-- ============================================================

SELECT 
    'Evaluations' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Evaluations' 
AND COLUMN_NAME = 'TitreEvaluation'

UNION ALL

SELECT 
    'DevoirsADomicile' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DevoirsADomicile' 
AND COLUMN_NAME = 'CoefficientDevoir';

PRINT '✅ Migration terminée avec succès';
GO

