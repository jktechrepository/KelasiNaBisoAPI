-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Ajout TitreEvaluation et CoefficientDevoir
-- Date : 2024-01-15
-- Description : 
--   1. Ajoute le champ TitreEvaluation dans la table Evaluations
--   2. Ajoute le champ CoefficientDevoir dans la table DevoirsADomicile
--   3. Met à jour les devoirs existants avec un coefficient par défaut
-- ============================================================
-- ⚠️ IMPORTANT : Exécuter ce script pendant une fenêtre de maintenance
-- ⚠️ IMPORTANT : Faire une sauvegarde de la base de données avant l'exécution
-- ============================================================

USE KelasiNaBiso;
GO

-- Début de la transaction
BEGIN TRANSACTION;
GO

PRINT '========================================';
PRINT 'DÉBUT DE LA MIGRATION';
PRINT 'Date : ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- ============================================================
-- 1. Ajouter TitreEvaluation dans la table Evaluations
-- ============================================================
PRINT '';
PRINT 'Étape 1/3 : Ajout de la colonne TitreEvaluation...';

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'TitreEvaluation'
)
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Evaluations]
        ADD [TitreEvaluation] NVARCHAR(500) NULL;
        
        PRINT '✅ Colonne TitreEvaluation ajoutée avec succès à la table Evaluations';
    END TRY
    BEGIN CATCH
        PRINT '❌ ERREUR lors de l''ajout de TitreEvaluation :';
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne TitreEvaluation existe déjà dans la table Evaluations - Ignoré';
END
GO

-- ============================================================
-- 2. Ajouter CoefficientDevoir dans la table DevoirsADomicile
-- ============================================================
PRINT '';
PRINT 'Étape 2/3 : Ajout de la colonne CoefficientDevoir...';

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'DevoirsADomicile' 
    AND COLUMN_NAME = 'CoefficientDevoir'
)
BEGIN
    BEGIN TRY
        -- Ajouter la colonne avec valeur par défaut
        ALTER TABLE [dbo].[DevoirsADomicile]
        ADD [CoefficientDevoir] INT NOT NULL DEFAULT 1;
        
        PRINT '✅ Colonne CoefficientDevoir ajoutée avec succès à la table DevoirsADomicile';
    END TRY
    BEGIN CATCH
        PRINT '❌ ERREUR lors de l''ajout de CoefficientDevoir :';
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne CoefficientDevoir existe déjà dans la table DevoirsADomicile - Ignoré';
    
    -- Mettre à jour les valeurs NULL ou 0 si la colonne existe déjà
    BEGIN TRY
        UPDATE [dbo].[DevoirsADomicile]
        SET [CoefficientDevoir] = 1
        WHERE [CoefficientDevoir] IS NULL OR [CoefficientDevoir] = 0;
        
        DECLARE @UpdatedRows INT = @@ROWCOUNT;
        IF @UpdatedRows > 0
        BEGIN
            PRINT '✅ ' + CAST(@UpdatedRows AS VARCHAR) + ' ligne(s) mise(s) à jour avec coefficient par défaut';
        END
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Avertissement lors de la mise à jour des coefficients existants :';
        PRINT ERROR_MESSAGE();
        -- Ne pas rollback, continuer
    END CATCH
END
GO

-- ============================================================
-- 3. Mettre à jour les devoirs existants avec un coefficient par défaut
-- (Uniquement si la colonne vient d'être créée)
-- ============================================================
PRINT '';
PRINT 'Étape 3/3 : Vérification des données existantes...';

BEGIN TRY
    -- Compter les devoirs existants
    DECLARE @TotalDevoirs INT;
    SELECT @TotalDevoirs = COUNT(*) FROM [dbo].[DevoirsADomicile];
    
    PRINT '📊 Nombre total de devoirs dans la base : ' + CAST(@TotalDevoirs AS VARCHAR);
    
    -- Vérifier s'il y a des devoirs avec coefficient NULL ou 0
    DECLARE @DevoirsAvecCoefficientInvalide INT;
    SELECT @DevoirsAvecCoefficientInvalide = COUNT(*) 
    FROM [dbo].[DevoirsADomicile]
    WHERE [CoefficientDevoir] IS NULL OR [CoefficientDevoir] = 0;
    
    IF @DevoirsAvecCoefficientInvalide > 0
    BEGIN
        UPDATE [dbo].[DevoirsADomicile]
        SET [CoefficientDevoir] = 1
        WHERE [CoefficientDevoir] IS NULL OR [CoefficientDevoir] = 0;
        
        PRINT '✅ ' + CAST(@DevoirsAvecCoefficientInvalide AS VARCHAR) + ' devoir(s) mis à jour avec coefficient = 1';
    END
    ELSE
    BEGIN
        PRINT '✅ Tous les devoirs ont déjà un coefficient valide';
    END
END TRY
BEGIN CATCH
    PRINT '⚠️ Avertissement lors de la vérification des données :';
    PRINT ERROR_MESSAGE();
    -- Ne pas rollback, continuer
END CATCH
GO

-- ============================================================
-- 4. Vérification finale
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VÉRIFICATION FINALE';
PRINT '========================================';

-- Vérifier TitreEvaluation
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'TitreEvaluation'
)
BEGIN
    PRINT '✅ Colonne TitreEvaluation : PRÉSENTE';
    
    SELECT 
        'Evaluations' AS TableName,
        COLUMN_NAME AS NomColonne,
        DATA_TYPE AS TypeDonnees,
        IS_NULLABLE AS Nullable,
        CHARACTER_MAXIMUM_LENGTH AS LongueurMax
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'TitreEvaluation';
END
ELSE
BEGIN
    PRINT '❌ Colonne TitreEvaluation : ABSENTE';
END
GO

-- Vérifier CoefficientDevoir
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'DevoirsADomicile' 
    AND COLUMN_NAME = 'CoefficientDevoir'
)
BEGIN
    PRINT '✅ Colonne CoefficientDevoir : PRÉSENTE';
    
    SELECT 
        'DevoirsADomicile' AS TableName,
        COLUMN_NAME AS NomColonne,
        DATA_TYPE AS TypeDonnees,
        IS_NULLABLE AS Nullable,
        COLUMN_DEFAULT AS ValeurParDefaut
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'DevoirsADomicile' 
    AND COLUMN_NAME = 'CoefficientDevoir';
END
ELSE
BEGIN
    PRINT '❌ Colonne CoefficientDevoir : ABSENTE';
END
GO

-- Statistiques sur les devoirs
PRINT '';
PRINT '📊 Statistiques des devoirs :';
SELECT 
    COUNT(*) AS TotalDevoirs,
    SUM(CASE WHEN CoefficientDevoir = 1 THEN 1 ELSE 0 END) AS Coefficient1,
    SUM(CASE WHEN CoefficientDevoir > 1 THEN 1 ELSE 0 END) AS CoefficientSuperieur1,
    SUM(CASE WHEN CoefficientDevoir IS NULL OR CoefficientDevoir = 0 THEN 1 ELSE 0 END) AS CoefficientInvalide
FROM [dbo].[DevoirsADomicile];
GO

-- ============================================================
-- 5. Validation et Commit
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VALIDATION DE LA MIGRATION';
PRINT '========================================';

-- Vérifier que les deux colonnes existent
DECLARE @TitreEvaluationExists BIT = 0;
DECLARE @CoefficientDevoirExists BIT = 0;

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'TitreEvaluation'
)
    SET @TitreEvaluationExists = 1;

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'DevoirsADomicile' 
    AND COLUMN_NAME = 'CoefficientDevoir'
)
    SET @CoefficientDevoirExists = 1;

IF @TitreEvaluationExists = 1 AND @CoefficientDevoirExists = 1
BEGIN
    PRINT '✅ MIGRATION RÉUSSIE - Toutes les colonnes sont présentes';
    PRINT '';
    PRINT '⚠️ IMPORTANT : Vérifier manuellement que tout fonctionne correctement avant de continuer';
    PRINT '⚠️ Si tout est OK, exécutez : COMMIT TRANSACTION;';
    PRINT '⚠️ Si problème, exécutez : ROLLBACK TRANSACTION;';
    PRINT '';
    PRINT '========================================';
    PRINT 'FIN DE LA MIGRATION';
    PRINT 'Date : ' + CONVERT(VARCHAR, GETDATE(), 120);
    PRINT '========================================';
    
    -- ⚠️ DÉCOMMENTER LA LIGNE SUIVANTE POUR COMMITER AUTOMATIQUEMENT
    -- COMMIT TRANSACTION;
    -- PRINT '✅ Transaction commitée avec succès';
END
ELSE
BEGIN
    PRINT '❌ MIGRATION ÉCHOUÉE - Certaines colonnes sont absentes';
    PRINT '   TitreEvaluation : ' + CASE WHEN @TitreEvaluationExists = 1 THEN 'OK' ELSE 'MANQUANT' END;
    PRINT '   CoefficientDevoir : ' + CASE WHEN @CoefficientDevoirExists = 1 THEN 'OK' ELSE 'MANQUANT' END;
    PRINT '';
    PRINT '❌ ROLLBACK DE LA TRANSACTION';
    ROLLBACK TRANSACTION;
    PRINT '✅ Transaction annulée';
END
GO

-- ============================================================
-- NOTES IMPORTANTES
-- ============================================================
/*
⚠️ AVANT D'EXÉCUTER CE SCRIPT :

1. ✅ Faire une SAUVEGARDE COMPLÈTE de la base de données
2. ✅ Tester le script sur une base de données de test
3. ✅ Vérifier que vous avez les permissions nécessaires (ALTER TABLE)
4. ✅ Exécuter pendant une fenêtre de maintenance
5. ✅ Vérifier les logs après l'exécution

📋 APRÈS L'EXÉCUTION :

1. ✅ Vérifier que les colonnes sont présentes
2. ✅ Vérifier que les données sont correctes
3. ✅ Tester la création d'un devoir avec IdCours
4. ✅ Vérifier qu'une évaluation est créée automatiquement
5. ✅ Vérifier les logs de l'application

🔄 EN CAS DE PROBLÈME :

Si vous devez annuler les changements :
- Le script utilise une transaction, donc un ROLLBACK annulera tout
- Si la transaction a déjà été commitée, vous devrez créer un script de rollback manuel

📞 SUPPORT :

En cas de problème, contacter l'équipe de développement avec :
- Les messages d'erreur complets
- Le résultat de la vérification finale
- Les logs de l'application
*/

