-- ============================================================
-- SCRIPT DE MIGRATION PRODUCTION
-- Ajout Periode dans Evaluation et Suppression Session de Notes
-- Date : 2024-01-15
-- Description : 
--   1. Ajoute le champ Periode dans la table Evaluations
--   2. Migre les données Session de Notes vers Periode de Evaluations
--   3. Supprime la colonne Session de la table Notes
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
PRINT 'DÉBUT DE LA MIGRATION PERIODE/SESSION';
PRINT 'Date : ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- ============================================================
-- 1. Vérifier l'état actuel
-- ============================================================
PRINT '';
PRINT 'Étape 1/5 : Vérification de l''état actuel...';

DECLARE @TotalNotes INT;
DECLARE @NotesAvecSession INT;
DECLARE @TotalEvaluations INT;

SELECT @TotalNotes = COUNT(*) FROM [dbo].[Notes];
SELECT @NotesAvecSession = COUNT(*) FROM [dbo].[Notes] WHERE [Session] IS NOT NULL AND [Session] != '';
SELECT @TotalEvaluations = COUNT(*) FROM [dbo].[Evaluations];

PRINT '📊 Nombre total de notes : ' + CAST(@TotalNotes AS VARCHAR);
PRINT '📊 Notes avec Session : ' + CAST(@NotesAvecSession AS VARCHAR);
PRINT '📊 Nombre total d''évaluations : ' + CAST(@TotalEvaluations AS VARCHAR);
GO

-- ============================================================
-- 2. Ajouter la colonne Periode dans Evaluations
-- ============================================================
PRINT '';
PRINT 'Étape 2/5 : Ajout de la colonne Periode dans Evaluations...';

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'Periode'
)
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Evaluations]
        ADD [Periode] NVARCHAR(255) NULL;
        
        PRINT '✅ Colonne Periode ajoutée avec succès à la table Evaluations';
    END TRY
    BEGIN CATCH
        PRINT '❌ ERREUR lors de l''ajout de Periode :';
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne Periode existe déjà dans la table Evaluations - Ignoré';
END
GO

-- ============================================================
-- 3. Migrer les données Session de Notes vers Periode de Evaluations
-- ============================================================
PRINT '';
PRINT 'Étape 3/5 : Migration des données Session -> Periode...';

BEGIN TRY
    -- Mettre à jour les évaluations avec la période depuis les notes associées
    -- Prendre la période la plus fréquente pour chaque évaluation
    UPDATE e
    SET e.[Periode] = (
        SELECT TOP 1 n.[Session]
        FROM [dbo].[Notes] n
        WHERE n.[IdEvaluation] = e.[IdEvaluation]
        AND n.[Session] IS NOT NULL
        AND n.[Session] != ''
        GROUP BY n.[Session]
        ORDER BY COUNT(*) DESC
    )
    FROM [dbo].[Evaluations] e
    WHERE e.[Periode] IS NULL
    AND EXISTS (
        SELECT 1 FROM [dbo].[Notes] n
        WHERE n.[IdEvaluation] = e.[IdEvaluation]
        AND n.[Session] IS NOT NULL
        AND n.[Session] != ''
    );
    
    DECLARE @EvaluationsMisesAJour INT = @@ROWCOUNT;
    PRINT '✅ ' + CAST(@EvaluationsMisesAJour AS VARCHAR) + ' évaluation(s) mise(s) à jour avec Periode depuis Notes';
    
    -- Vérifier s'il reste des évaluations sans période
    DECLARE @EvaluationsSansPeriode INT;
    SELECT @EvaluationsSansPeriode = COUNT(*) 
    FROM [dbo].[Evaluations] 
    WHERE [Periode] IS NULL;
    
    IF @EvaluationsSansPeriode > 0
    BEGIN
        PRINT '⚠️ ' + CAST(@EvaluationsSansPeriode AS VARCHAR) + ' évaluation(s) sans période (seront NULL)';
    END
END TRY
BEGIN CATCH
    PRINT '⚠️ Avertissement lors de la migration des données :';
    PRINT ERROR_MESSAGE();
    -- Ne pas rollback, continuer
END CATCH
GO

-- ============================================================
-- 4. Supprimer la colonne Session de Notes
-- ============================================================
PRINT '';
PRINT 'Étape 4/5 : Suppression de la colonne Session de Notes...';

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'Session'
)
BEGIN
    BEGIN TRY
        -- Supprimer la colonne Session
        ALTER TABLE [dbo].[Notes]
        DROP COLUMN [Session];
        
        PRINT '✅ Colonne Session supprimée de la table Notes';
    END TRY
    BEGIN CATCH
        PRINT '❌ ERREUR lors de la suppression de Session :';
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT '⚠️ La colonne Session n''existe pas dans la table Notes - Ignoré';
END
GO

-- ============================================================
-- 5. Vérification finale
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VÉRIFICATION FINALE';
PRINT '========================================';

-- Vérifier Periode dans Evaluations
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'Periode'
)
BEGIN
    PRINT '✅ Colonne Periode : PRÉSENTE dans Evaluations';
    
    SELECT 
        'Evaluations' AS TableName,
        COLUMN_NAME AS NomColonne,
        DATA_TYPE AS TypeDonnees,
        IS_NULLABLE AS Nullable,
        CHARACTER_MAXIMUM_LENGTH AS LongueurMax
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'Periode';
END
ELSE
BEGIN
    PRINT '❌ Colonne Periode : ABSENTE dans Evaluations';
END
GO

-- Vérifier que Session n'existe plus dans Notes
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'Session'
)
BEGIN
    PRINT '✅ Colonne Session : SUPPRIMÉE de Notes';
END
ELSE
BEGIN
    PRINT '❌ Colonne Session : TOUJOURS PRÉSENTE dans Notes';
END
GO

-- Statistiques sur les évaluations avec période
PRINT '';
PRINT '📊 Statistiques des évaluations après migration :';
SELECT 
    COUNT(*) AS TotalEvaluations,
    SUM(CASE WHEN Periode IS NOT NULL AND Periode != '' THEN 1 ELSE 0 END) AS EvaluationsAvecPeriode,
    SUM(CASE WHEN Periode IS NULL OR Periode = '' THEN 1 ELSE 0 END) AS EvaluationsSansPeriode,
    COUNT(DISTINCT Periode) AS PeriodesUniques
FROM [dbo].[Evaluations];
GO

-- Afficher les périodes uniques
PRINT '';
PRINT '📋 Périodes uniques trouvées :';
SELECT DISTINCT 
    Periode,
    COUNT(*) AS NombreEvaluations
FROM [dbo].[Evaluations]
WHERE Periode IS NOT NULL AND Periode != ''
GROUP BY Periode
ORDER BY COUNT(*) DESC;
GO

-- ============================================================
-- 6. Validation et Commit
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'VALIDATION DE LA MIGRATION';
PRINT '========================================';

DECLARE @PeriodeExists BIT = 0;
DECLARE @SessionRemoved BIT = 0;

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Evaluations' 
    AND COLUMN_NAME = 'Periode'
)
    SET @PeriodeExists = 1;

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'Notes' 
    AND COLUMN_NAME = 'Session'
)
    SET @SessionRemoved = 1;

IF @PeriodeExists = 1 AND @SessionRemoved = 1
BEGIN
    PRINT '✅ MIGRATION RÉUSSIE';
    PRINT '   - Periode ajoutée dans Evaluations';
    PRINT '   - Session supprimée de Notes';
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
    PRINT '❌ MIGRATION ÉCHOUÉE';
    PRINT '   Periode dans Evaluations : ' + CASE WHEN @PeriodeExists = 1 THEN 'OK' ELSE 'MANQUANT' END;
    PRINT '   Session supprimée de Notes : ' + CASE WHEN @SessionRemoved = 1 THEN 'OK' ELSE 'NON SUPPRIMÉ' END;
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
3. ✅ Vérifier que vous avez les permissions nécessaires (ALTER TABLE, UPDATE, DROP COLUMN)
4. ✅ Exécuter pendant une fenêtre de maintenance
5. ✅ Vérifier les logs après l'exécution

📋 APRÈS L'EXÉCUTION :

1. ✅ Vérifier que la colonne Periode est présente dans Evaluations
2. ✅ Vérifier que la colonne Session est supprimée de Notes
3. ✅ Vérifier que les périodes ont été migrées correctement
4. ✅ Tester la création d'une évaluation avec Periode
5. ✅ Tester la création d'une note (sans Session)
6. ✅ Vérifier les logs de l'application

🔄 EN CAS DE PROBLÈME :

Si vous devez annuler les changements :
- Le script utilise une transaction, donc un ROLLBACK annulera tout
- Si la transaction a déjà été commitée, vous devrez créer un script de rollback manuel

📞 SUPPORT :

En cas de problème, contacter l'équipe de développement avec :
- Les messages d'erreur complets
- Le résultat de la vérification finale
- Les statistiques des évaluations
- Les logs de l'application
*/

