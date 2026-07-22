-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Migration Notes : De Cours vers Evaluation
-- Date : 2024-01-15
-- Description : 
--   1. Ajoute la colonne IdEvaluation dans la table Notes
--   2. Crée des évaluations pour les notes existantes (si nécessaire)
--   3. Migre les données existantes
--   4. Rend IdEvaluation obligatoire
--   5. Supprime la colonne IdCours (optionnel, peut être gardée pour compatibilité)
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
PRINT 'DÉBUT DE LA MIGRATION NOTES -> EVALUATION';
PRINT 'Date : ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- ============================================================
-- 1. Vérifier l'état actuel
-- ============================================================
PRINT '';
PRINT 'Étape 1/6 : Vérification de l''état actuel...';

DECLARE @TotalNotes INT;
DECLARE @NotesAvecCours INT;
SELECT @TotalNotes = COUNT(*) FROM [dbo].[Notes];
SELECT @NotesAvecCours = COUNT(*) FROM [dbo].[Notes] WHERE [IdCours] IS NOT NULL AND [IdCours] > 0;

PRINT '📊 Nombre total de notes : ' + CAST(@TotalNotes AS VARCHAR);
PRINT '📊 Notes avec IdCours : ' + CAST(@NotesAvecCours AS VARCHAR);
GO

-- ============================================================
-- 2. Ajouter la colonne IdEvaluation (nullable temporairement)
-- ============================================================
PRINT '';
PRINT 'Étape 2/6 : Ajout de la colonne IdEvaluation...';

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'IdEvaluation'
)
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Notes]
        ADD [IdEvaluation] INT NULL;
        
        PRINT '✅ Colonne IdEvaluation ajoutée (nullable temporairement)';
    END TRY
    BEGIN CATCH
        PRINT '❌ ERREUR lors de l''ajout de IdEvaluation :';
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne IdEvaluation existe déjà - Ignoré';
END
GO

-- ============================================================
-- 3. Créer des évaluations pour les notes existantes
-- ============================================================
PRINT '';
PRINT 'Étape 3/6 : Création des évaluations pour les notes existantes...';

BEGIN TRY
    -- Créer une évaluation générique pour chaque combinaison unique (IdCours, IdClasse)
    -- qui n'a pas encore d'évaluation correspondante
    INSERT INTO [dbo].[Evaluations] (
        [TypeEvaluation],
        [TitreEvaluation],
        [Coefficient],
        [IdCours],
        [IdClasse],
        [Statut],
        [DateCreation]
    )
    SELECT DISTINCT
        'Note Generique' AS TypeEvaluation,
        'Évaluation générée pour migration' AS TitreEvaluation,
        1.0 AS Coefficient,
        n.[IdCours],
        -- Récupérer IdClasse depuis Cours
        (SELECT TOP 1 c.[IdClasse] FROM [dbo].[Cours] c WHERE c.[IdCours] = n.[IdCours]) AS IdClasse,
        1 AS Statut,
        GETDATE() AS DateCreation
    FROM [dbo].[Notes] n
    WHERE n.[IdCours] IS NOT NULL 
    AND n.[IdCours] > 0
    AND EXISTS (SELECT 1 FROM [dbo].[Cours] c WHERE c.[IdCours] = n.[IdCours] AND c.[IdClasse] IS NOT NULL)
    AND NOT EXISTS (
        SELECT 1 FROM [dbo].[Evaluations] e
        WHERE e.[IdCours] = n.[IdCours]
        AND e.[IdClasse] = (SELECT TOP 1 c.[IdClasse] FROM [dbo].[Cours] c WHERE c.[IdCours] = n.[IdCours])
    );
    
    DECLARE @EvaluationsCreees INT = @@ROWCOUNT;
    PRINT '✅ ' + CAST(@EvaluationsCreees AS VARCHAR) + ' évaluation(s) générique(s) créée(s)';
END TRY
BEGIN CATCH
    PRINT '⚠️ Avertissement lors de la création des évaluations :';
    PRINT ERROR_MESSAGE();
    -- Ne pas rollback, continuer
END CATCH
GO

-- ============================================================
-- 4. Mettre à jour les notes avec IdEvaluation
-- ============================================================
PRINT '';
PRINT 'Étape 4/6 : Mise à jour des notes avec IdEvaluation...';

BEGIN TRY
    -- Mettre à jour les notes pour pointer vers les évaluations correspondantes
    UPDATE n
    SET n.[IdEvaluation] = e.[IdEvaluation]
    FROM [dbo].[Notes] n
    INNER JOIN [dbo].[Evaluations] e ON e.[IdCours] = n.[IdCours]
    WHERE n.[IdEvaluation] IS NULL
    AND n.[IdCours] IS NOT NULL
    AND n.[IdCours] > 0
    -- Matcher sur IdCours uniquement (Notes n'a pas IdClasse)
    AND e.[IdCours] = n.[IdCours];
    
    -- Si Notes n'a pas IdClasse, utiliser une approche plus simple
    -- Prendre la première évaluation pour chaque cours
    UPDATE n
    SET n.[IdEvaluation] = (
        SELECT TOP 1 e.[IdEvaluation]
        FROM [dbo].[Evaluations] e
        WHERE e.[IdCours] = n.[IdCours]
        ORDER BY e.[IdEvaluation]
    )
    FROM [dbo].[Notes] n
    WHERE n.[IdEvaluation] IS NULL
    AND n.[IdCours] IS NOT NULL
    AND n.[IdCours] > 0;
    
    DECLARE @NotesMisesAJour INT = @@ROWCOUNT;
    PRINT '✅ ' + CAST(@NotesMisesAJour AS VARCHAR) + ' note(s) mise(s) à jour avec IdEvaluation';
    
    -- Vérifier s'il reste des notes sans IdEvaluation
    DECLARE @NotesSansEvaluation INT;
    SELECT @NotesSansEvaluation = COUNT(*) 
    FROM [dbo].[Notes] 
    WHERE [IdEvaluation] IS NULL;
    
    IF @NotesSansEvaluation > 0
    BEGIN
        PRINT '⚠️ ATTENTION : ' + CAST(@NotesSansEvaluation AS VARCHAR) + ' note(s) sans évaluation associée';
        PRINT '   Ces notes devront être migrées manuellement ou supprimées';
    END
END TRY
BEGIN CATCH
    PRINT '❌ ERREUR lors de la mise à jour des notes :';
    PRINT ERROR_MESSAGE();
    ROLLBACK TRANSACTION;
    RETURN;
END CATCH
GO

-- ============================================================
-- 5. Rendre IdEvaluation obligatoire (si toutes les notes ont une évaluation)
-- ============================================================
PRINT '';
PRINT 'Étape 5/6 : Rendre IdEvaluation obligatoire...';

BEGIN TRY
    DECLARE @NotesSansEval INT;
    SELECT @NotesSansEval = COUNT(*) FROM [dbo].[Notes] WHERE [IdEvaluation] IS NULL;
    
    IF @NotesSansEval = 0
    BEGIN
        -- Supprimer les contraintes de clé étrangère existantes si nécessaire
        -- (à adapter selon votre schéma)
        
        -- Rendre la colonne NOT NULL
        ALTER TABLE [dbo].[Notes]
        ALTER COLUMN [IdEvaluation] INT NOT NULL;
        
        PRINT '✅ Colonne IdEvaluation rendue obligatoire (NOT NULL)';
    END
    ELSE
    BEGIN
        PRINT '⚠️ ' + CAST(@NotesSansEval AS VARCHAR) + ' note(s) sans évaluation - IdEvaluation reste nullable';
        PRINT '   Veuillez corriger ces notes avant de rendre IdEvaluation obligatoire';
    END
END TRY
BEGIN CATCH
    PRINT '⚠️ Avertissement lors de la modification de IdEvaluation :';
    PRINT ERROR_MESSAGE();
    -- Ne pas rollback, continuer
END CATCH
GO

-- ============================================================
-- 6. Supprimer la colonne IdCours (OPTIONNEL - pour compatibilité, peut être gardée)
-- ============================================================
PRINT '';
PRINT 'Étape 6/6 : Suppression de la colonne IdCours (OPTIONNEL)...';
PRINT '⚠️ Cette étape est OPTIONNELLE - La colonne peut être gardée pour compatibilité';
PRINT '⚠️ Pour supprimer IdCours, décommenter le code ci-dessous';

/*
-- ⚠️ DÉCOMMENTER POUR SUPPRIMER IdCours
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'IdCours'
)
BEGIN
    BEGIN TRY
        -- Supprimer la contrainte de clé étrangère si elle existe
        DECLARE @FKName NVARCHAR(128);
        SELECT @FKName = name
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID('dbo.Notes')
        AND referenced_object_id = OBJECT_ID('dbo.Cours');
        
        IF @FKName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [dbo].[Notes] DROP CONSTRAINT [' + @FKName + ']');
            PRINT '✅ Contrainte de clé étrangère supprimée';
        END
        
        -- Supprimer la colonne
        ALTER TABLE [dbo].[Notes]
        DROP COLUMN [IdCours];
        
        PRINT '✅ Colonne IdCours supprimée';
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Avertissement lors de la suppression de IdCours :';
        PRINT ERROR_MESSAGE();
        -- Ne pas rollback, continuer
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne IdCours n''existe pas ou a déjà été supprimée';
END
*/
GO

-- ============================================================
-- 7. Vérification finale
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VÉRIFICATION FINALE';
PRINT '========================================';

-- Vérifier IdEvaluation
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'IdEvaluation'
)
BEGIN
    PRINT '✅ Colonne IdEvaluation : PRÉSENTE';
    
    SELECT 
        'Notes' AS TableName,
        COLUMN_NAME AS NomColonne,
        DATA_TYPE AS TypeDonnees,
        IS_NULLABLE AS Nullable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'IdEvaluation';
END
ELSE
BEGIN
    PRINT '❌ Colonne IdEvaluation : ABSENTE';
END
GO

-- Statistiques sur les notes
PRINT '';
PRINT '📊 Statistiques des notes après migration :';
SELECT 
    COUNT(*) AS TotalNotes,
    SUM(CASE WHEN IdEvaluation IS NOT NULL THEN 1 ELSE 0 END) AS NotesAvecEvaluation,
    SUM(CASE WHEN IdEvaluation IS NULL THEN 1 ELSE 0 END) AS NotesSansEvaluation,
    COUNT(DISTINCT IdEvaluation) AS NombreEvaluationsUniques
FROM [dbo].[Notes];
GO

-- ============================================================
-- 8. Validation et Commit
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VALIDATION DE LA MIGRATION';
PRINT '========================================';

DECLARE @IdEvaluationExists BIT = 0;
DECLARE @NotesSansEvalFinal INT = 0;

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'IdEvaluation'
)
    SET @IdEvaluationExists = 1;

SELECT @NotesSansEvalFinal = COUNT(*) 
FROM [dbo].[Notes] 
WHERE [IdEvaluation] IS NULL;

IF @IdEvaluationExists = 1
BEGIN
    IF @NotesSansEvalFinal = 0
    BEGIN
        PRINT '✅ MIGRATION RÉUSSIE - Toutes les notes ont une évaluation associée';
    END
    ELSE
    BEGIN
        PRINT '⚠️ MIGRATION PARTIELLE - ' + CAST(@NotesSansEvalFinal AS VARCHAR) + ' note(s) sans évaluation';
        PRINT '   Veuillez corriger ces notes manuellement';
    END
    
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
    PRINT '❌ MIGRATION ÉCHOUÉE - La colonne IdEvaluation est absente';
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
3. ✅ Vérifier que vous avez les permissions nécessaires (ALTER TABLE, INSERT, UPDATE)
4. ✅ Exécuter pendant une fenêtre de maintenance
5. ✅ Vérifier les logs après l'exécution

📋 APRÈS L'EXÉCUTION :

1. ✅ Vérifier que la colonne IdEvaluation est présente
2. ✅ Vérifier que toutes les notes ont une évaluation associée
3. ✅ Tester la création d'une note avec IdEvaluation
4. ✅ Vérifier que les endpoints fonctionnent correctement
5. ✅ Vérifier les logs de l'application

🔄 EN CAS DE PROBLÈME :

Si vous devez annuler les changements :
- Le script utilise une transaction, donc un ROLLBACK annulera tout
- Si la transaction a déjà été commitée, vous devrez créer un script de rollback manuel

📞 SUPPORT :

En cas de problème, contacter l'équipe de développement avec :
- Les messages d'erreur complets
- Le résultat de la vérification finale
- Les statistiques des notes
- Les logs de l'application
*/

