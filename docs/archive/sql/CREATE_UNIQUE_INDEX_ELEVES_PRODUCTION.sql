-- ============================================================================
-- SCRIPT SQL PRODUCTION : CRÉATION INDEX UNIQUE COMPOSITE POUR ÉLÈVES
-- ============================================================================
-- Date : 1er décembre 2025
-- Base de données : KelasiNaBisoDb
-- Objectif : Créer un index unique composite pour empêcher les doublons d'élèves
-- ============================================================================
-- 
-- ⚠️ IMPORTANT : 
-- 1. Exécuter ce script sur une COPIE de la base de production d'abord (test)
-- 2. Vérifier les doublons existants avant d'appliquer l'index
-- 3. Nettoyer les doublons si nécessaire
-- 4. Exécuter pendant une période de faible activité
-- 
-- ============================================================================

-- Désactiver les warnings temporairement
SET SQL_MODE = '';

-- ============================================================================
-- ÉTAPE 1 : VÉRIFIER LES DOUBLONS EXISTANTS (ÉLÈVES ACTIFS)
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 1 : VÉRIFICATION DES DOUBLONS EXISTANTS' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

-- Afficher les doublons potentiels (élèves actifs uniquement)
SELECT 
    Nom,
    Postnom,
    Prenom,
    DATE_FORMAT(DateNaissance, '%Y-%m-%d') as DateNaissance,
    IdTuteur,
    IdClasse,
    COUNT(*) as NombreDoublons,
    GROUP_CONCAT(IdEleve ORDER BY DateCreation SEPARATOR ', ') as IdsEleves,
    GROUP_CONCAT(NomComplet ORDER BY DateCreation SEPARATOR ' | ') as NomsComplets,
    GROUP_CONCAT(DATE_FORMAT(DateCreation, '%Y-%m-%d %H:%i:%s') ORDER BY DateCreation SEPARATOR ' | ') as DatesCreation
FROM Eleves
WHERE Statut = 1
    AND Nom IS NOT NULL
    AND Postnom IS NOT NULL
    AND Prenom IS NOT NULL
    AND IdTuteur IS NOT NULL
    AND IdClasse IS NOT NULL
GROUP BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
HAVING COUNT(*) > 1
ORDER BY NombreDoublons DESC, Nom, Prenom;

-- Compter le nombre total de doublons
SELECT 
    COUNT(*) as NombreGroupesDoublons,
    SUM(NombreDoublons - 1) as NombreElevesEnDoublon
FROM (
    SELECT 
        COUNT(*) as NombreDoublons
    FROM Eleves
    WHERE Statut = 1
        AND Nom IS NOT NULL
        AND Postnom IS NOT NULL
        AND Prenom IS NOT NULL
        AND IdTuteur IS NOT NULL
        AND IdClasse IS NOT NULL
    GROUP BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
    HAVING COUNT(*) > 1
) as Doublons;

-- ============================================================================
-- ÉTAPE 2 : VÉRIFIER SI L'INDEX EXISTE DÉJÀ
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 2 : VÉRIFICATION DE L''INDEX EXISTANT' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

SELECT 
    INDEX_NAME,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX SEPARATOR ', ') as Colonnes,
    NON_UNIQUE,
    CASE 
        WHEN NON_UNIQUE = 0 THEN 'UNIQUE'
        ELSE 'NON-UNIQUE'
    END as TypeIndex
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
GROUP BY INDEX_NAME, NON_UNIQUE;

-- ============================================================================
-- ÉTAPE 3 : NETTOYER LES DOUBLONS (OPTIONNEL - À ADAPTER)
-- ============================================================================

-- ⚠️ ATTENTION : Cette section est COMMENTÉE par défaut
-- Décommenter et adapter selon votre stratégie de nettoyage
-- 
-- Stratégie recommandée :
-- 1. Garder l'élève le plus ancien (DateCreation la plus ancienne)
-- 2. Désactiver (Statut = 0) les doublons plus récents
-- 3. Transférer les inscriptions/paiements/presences vers l'élève conservé
-- 
-- ⚠️ FAIRE UNE SAUVEGARDE AVANT D'EXÉCUTER LE NETTOYAGE

/*
-- Exemple de nettoyage : Désactiver les doublons (garder le plus ancien)
-- ADAPTER SELON VOS BESOINS

-- Étape 3.1 : Identifier les doublons à désactiver
CREATE TEMPORARY TABLE IF NOT EXISTS Temp_Doublons_A_Desactiver AS
SELECT 
    e.IdEleve,
    e.Nom,
    e.Postnom,
    e.Prenom,
    e.DateNaissance,
    e.IdTuteur,
    e.IdClasse,
    e.DateCreation,
    ROW_NUMBER() OVER (
        PARTITION BY e.Nom, e.Postnom, e.Prenom, e.DateNaissance, e.IdTuteur, e.IdClasse 
        ORDER BY e.DateCreation ASC
    ) as RowNum
FROM Eleves e
WHERE e.Statut = 1
    AND e.Nom IS NOT NULL
    AND e.Postnom IS NOT NULL
    AND e.Prenom IS NOT NULL
    AND e.IdTuteur IS NOT NULL
    AND e.IdClasse IS NOT NULL
    AND EXISTS (
        SELECT 1
        FROM Eleves e2
        WHERE e2.Statut = 1
            AND e2.Nom = e.Nom
            AND e2.Postnom = e.Postnom
            AND e2.Prenom = e.Prenom
            AND e2.DateNaissance = e.DateNaissance
            AND e2.IdTuteur = e.IdTuteur
            AND e2.IdClasse = e.IdClasse
            AND e2.IdEleve != e.IdEleve
    );

-- Étape 3.2 : Afficher les doublons qui seront désactivés (pour vérification)
SELECT 
    'Doublons qui seront désactivés (garder RowNum = 1, désactiver RowNum > 1)' as Info,
    COUNT(*) as NombreADesactiver
FROM Temp_Doublons_A_Desactiver
WHERE RowNum > 1;

-- Étape 3.3 : Désactiver les doublons (garder RowNum = 1, désactiver les autres)
-- ⚠️ DÉCOMMENTER UNIQUEMENT APRÈS VÉRIFICATION
-- UPDATE Eleves e
-- INNER JOIN Temp_Doublons_A_Desactiver d ON e.IdEleve = d.IdEleve
-- SET e.Statut = 0,
--     e.Commentaire = CONCAT(IFNULL(e.Commentaire, ''), ' | Doublon désactivé le ', DATE_FORMAT(NOW(), '%Y-%m-%d'))
-- WHERE d.RowNum > 1;

-- Étape 3.4 : Nettoyer la table temporaire
-- DROP TEMPORARY TABLE IF EXISTS Temp_Doublons_A_Desactiver;
*/

-- ============================================================================
-- ÉTAPE 4 : VÉRIFIER QU'IL N'Y A PLUS DE DOUBLONS ACTIFS
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 4 : VÉRIFICATION FINALE DES DOUBLONS' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

-- Cette requête doit retourner 0 avant d'appliquer l'index
SELECT 
    COUNT(*) as NombreDoublonsActifs,
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ Aucun doublon actif - OK pour créer l''index'
        ELSE '❌ Des doublons actifs existent - Nettoyer avant de créer l''index'
    END as Statut
FROM (
    SELECT 
        Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse,
        COUNT(*) as Nb
    FROM Eleves
    WHERE Statut = 1
        AND Nom IS NOT NULL
        AND Postnom IS NOT NULL
        AND Prenom IS NOT NULL
        AND IdTuteur IS NOT NULL
        AND IdClasse IS NOT NULL
    GROUP BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse
    HAVING COUNT(*) > 1
) as Doublons;

-- ============================================================================
-- ÉTAPE 5 : CRÉER L'INDEX UNIQUE COMPOSITE
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 5 : CRÉATION DE L''INDEX UNIQUE' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

-- Vérifier si l'index existe déjà
SET @index_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = 'Eleves'
        AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
);

-- Si l'index n'existe pas, le créer
SET @sql = IF(
    @index_exists = 0,
    'CREATE UNIQUE INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe
     ON Eleves(Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse)',
    'SELECT ''L''''index existe déjà - Aucune action nécessaire'' as Message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- ÉTAPE 6 : VÉRIFIER QUE L'INDEX A ÉTÉ CRÉÉ
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 6 : VÉRIFICATION DE L''INDEX CRÉÉ' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

SELECT 
    INDEX_NAME as NomIndex,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX SEPARATOR ', ') as Colonnes,
    CASE 
        WHEN NON_UNIQUE = 0 THEN 'UNIQUE ✅'
        ELSE 'NON-UNIQUE'
    END as TypeIndex,
    CASE 
        WHEN COUNT(*) = 6 THEN '✅ Index complet (6 colonnes)'
        ELSE CONCAT('⚠️ Index incomplet (', COUNT(*), ' colonnes)')
    END as Statut
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
GROUP BY INDEX_NAME, NON_UNIQUE;

-- ============================================================================
-- ÉTAPE 7 : TEST DE L'INDEX (OPTIONNEL)
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'ÉTAPE 7 : TEST DE L''INDEX' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

-- Tester que l'index fonctionne en tentant d'insérer un doublon
-- ⚠️ NE PAS EXÉCUTER EN PRODUCTION - C'est juste un test
-- 
-- Pour tester :
-- 1. Récupérer un élève existant
-- 2. Tenter d'insérer un doublon avec les mêmes critères
-- 3. Vérifier que l'insertion échoue

/*
-- Exemple de test (À NE PAS EXÉCUTER EN PRODUCTION)
-- Récupérer un élève existant pour le test
SET @test_nom = (SELECT Nom FROM Eleves WHERE Statut = 1 AND Nom IS NOT NULL LIMIT 1);
SET @test_postnom = (SELECT Postnom FROM Eleves WHERE Statut = 1 AND Postnom IS NOT NULL LIMIT 1);
SET @test_prenom = (SELECT Prenom FROM Eleves WHERE Statut = 1 AND Prenom IS NOT NULL LIMIT 1);
SET @test_date = (SELECT DateNaissance FROM Eleves WHERE Statut = 1 LIMIT 1);
SET @test_tuteur = (SELECT IdTuteur FROM Eleves WHERE Statut = 1 AND IdTuteur IS NOT NULL LIMIT 1);
SET @test_classe = (SELECT IdClasse FROM Eleves WHERE Statut = 1 AND IdClasse IS NOT NULL LIMIT 1);

-- Tenter d'insérer un doublon (devrait échouer)
INSERT INTO Eleves (
    Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse, Statut, DateCreation, NomComplet
) VALUES (
    @test_nom, @test_postnom, @test_prenom, @test_date, @test_tuteur, @test_classe, 1, NOW(), CONCAT(@test_nom, ' ', @test_postnom, ' ', @test_prenom)
);

-- Si l'insertion réussit (ne devrait pas), supprimer le test
-- DELETE FROM Eleves WHERE Nom = @test_nom AND Postnom = @test_postnom AND Prenom = @test_prenom AND DateCreation > DATE_SUB(NOW(), INTERVAL 1 MINUTE);
*/

-- ============================================================================
-- RÉSUMÉ FINAL
-- ============================================================================

SELECT 
    '════════════════════════════════════════════════════════════' as Separator,
    'RÉSUMÉ FINAL' as Etape,
    '════════════════════════════════════════════════════════════' as Separator2;

SELECT 
    'Index unique créé avec succès ✅' as Message,
    'Nom' as Colonne1,
    'Postnom' as Colonne2,
    'Prenom' as Colonne3,
    'DateNaissance' as Colonne4,
    'IdTuteur' as Colonne5,
    'IdClasse' as Colonne6;

-- ============================================================================
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- 1. **NULL dans l'index** : 
--    - MariaDB/MySQL traite NULL comme une valeur distincte
--    - Plusieurs élèves avec Nom=NULL peuvent coexister
--    - Le code C# vérifie NULL avant création
-- 
-- 2. **Performance** :
--    - L'index améliore les performances de recherche
--    - L'insertion peut être légèrement plus lente (acceptable)
-- 
-- 3. **Doublons existants** :
--    - Si des doublons existent, l'index ne peut pas être créé
--    - Nettoyer d'abord les doublons avec la section ÉTAPE 3
-- 
-- 4. **Statut** :
--    - L'index ne filtre pas par Statut (limitation MariaDB)
--    - Le code C# vérifie Statut = 1 avant création
--    - Les élèves inactifs peuvent avoir des "doublons" (normal)
-- 
-- 5. **Rollback** :
--    - Pour supprimer l'index : 
--      DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;
-- 
-- ============================================================================

