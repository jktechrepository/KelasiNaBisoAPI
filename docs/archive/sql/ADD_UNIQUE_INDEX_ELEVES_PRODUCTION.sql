-- ============================================================================
-- SCRIPT SQL : INDEX UNIQUE COMPOSITE POUR PRÉVENIR LES DOUBLONS D'ÉLÈVES
-- ============================================================================
-- Date : 1er décembre 2025
-- Objectif : Créer un index unique composite pour empêcher les doublons
-- ============================================================================
-- 
-- ⚠️ IMPORTANT : 
-- 1. Exécuter d'abord le script de nettoyage des doublons (si nécessaire)
-- 2. Vérifier qu'il n'y a pas de doublons actifs avant d'appliquer l'index
-- 3. Tester sur un environnement de staging avant la production
-- 
-- ============================================================================

-- ============================================================================
-- ÉTAPE 1 : VÉRIFIER LES DOUBLONS EXISTANTS
-- ============================================================================

-- Afficher les doublons potentiels (élèves actifs uniquement)
SELECT 
    Nom,
    Postnom,
    Prenom,
    DateNaissance,
    IdTuteur,
    IdClasse,
    COUNT(*) as NombreDoublons,
    GROUP_CONCAT(IdEleve ORDER BY DateCreation) as IdsEleves,
    GROUP_CONCAT(NomComplet ORDER BY DateCreation SEPARATOR ' | ') as NomsComplets,
    GROUP_CONCAT(DateCreation ORDER BY DateCreation SEPARATOR ' | ') as DatesCreation
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

-- ============================================================================
-- ÉTAPE 2 : NETTOYER LES DOUBLONS (OPTIONNEL - À ADAPTER SELON VOS BESOINS)
-- ============================================================================

-- ⚠️ ATTENTION : Ce script est un EXEMPLE. Adapter selon votre stratégie de nettoyage.
-- 
-- Stratégie recommandée :
-- 1. Garder l'élève le plus ancien (DateCreation la plus ancienne)
-- 2. Désactiver (Statut = 0) ou supprimer les doublons plus récents
-- 3. Transférer les inscriptions/paiements/presences vers l'élève conservé
-- 
-- Exemple de nettoyage (À ADAPTER) :
/*
-- Identifier les doublons à désactiver (garder le plus ancien)
WITH Doublons AS (
    SELECT 
        IdEleve,
        Nom,
        Postnom,
        Prenom,
        DateNaissance,
        IdTuteur,
        IdClasse,
        DateCreation,
        ROW_NUMBER() OVER (
            PARTITION BY Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse 
            ORDER BY DateCreation ASC
        ) as RowNum
    FROM Eleves
    WHERE Statut = 1
        AND Nom IS NOT NULL
        AND Postnom IS NOT NULL
        AND Prenom IS NOT NULL
        AND IdTuteur IS NOT NULL
        AND IdClasse IS NOT NULL
)
-- Désactiver les doublons (garder RowNum = 1, désactiver les autres)
UPDATE Eleves e
INNER JOIN Doublons d ON e.IdEleve = d.IdEleve
SET e.Statut = 0
WHERE d.RowNum > 1;
*/

-- ============================================================================
-- ÉTAPE 3 : VÉRIFIER QU'IL N'Y A PLUS DE DOUBLONS ACTIFS
-- ============================================================================

-- Cette requête doit retourner 0 lignes avant d'appliquer l'index
SELECT 
    COUNT(*) as NombreDoublonsActifs
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
-- ÉTAPE 4 : CRÉER L'INDEX UNIQUE COMPOSITE
-- ============================================================================

-- Vérifier si l'index existe déjà
SELECT 
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe';

-- Créer l'index unique composite
-- Note : MariaDB/MySQL ne supporte pas directement les index filtrés avec WHERE
-- On va créer un index unique standard, mais la vérification C# filtre déjà par Statut = 1

CREATE UNIQUE INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe
ON Eleves(Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse);

-- ============================================================================
-- ÉTAPE 5 : VÉRIFIER QUE L'INDEX A ÉTÉ CRÉÉ
-- ============================================================================

SELECT 
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX,
    NON_UNIQUE
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Eleves'
    AND INDEX_NAME = 'IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe'
ORDER BY SEQ_IN_INDEX;

-- ============================================================================
-- ÉTAPE 6 : TESTER L'INDEX (OPTIONNEL)
-- ============================================================================

-- Tenter d'insérer un doublon (devrait échouer)
-- ⚠️ NE PAS EXÉCUTER EN PRODUCTION - C'est juste un test
/*
INSERT INTO Eleves (
    Nom, Postnom, Prenom, DateNaissance, IdTuteur, IdClasse, Statut, DateCreation
) VALUES (
    'TEST', 'TEST', 'TEST', '2020-01-01', 1, 1, 1, NOW()
);

-- Si l'insertion réussit, supprimer le test
DELETE FROM Eleves WHERE Nom = 'TEST' AND Postnom = 'TEST' AND Prenom = 'TEST';
*/

-- ============================================================================
-- ROLLBACK (Si nécessaire)
-- ============================================================================

-- Pour supprimer l'index si nécessaire :
-- DROP INDEX IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe ON Eleves;

-- ============================================================================
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- 1. **NULL dans l'index** : 
--    - MariaDB/MySQL traite NULL comme une valeur distincte
--    - Plusieurs élèves avec Nom=NULL peuvent coexister
--    - C'est pourquoi on vérifie NULL dans le code C#
-- 
-- 2. **Performance** :
--    - L'index améliore les performances de recherche
--    - L'insertion peut être légèrement plus lente (acceptable)
-- 
-- 3. **Doublons existants** :
--    - Si des doublons existent, l'index ne peut pas être créé
--    - Nettoyer d'abord les doublons
-- 
-- 4. **Statut** :
--    - L'index ne filtre pas par Statut (limitation MariaDB)
--    - Le code C# vérifie Statut = 1 avant création
--    - Les élèves inactifs peuvent avoir des "doublons" (normal)
-- 
-- ============================================================================

