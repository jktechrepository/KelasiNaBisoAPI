-- ============================================================================
-- SCRIPT SQL : Diagnostic Détaillé des 15 Incohérences Restantes
-- ============================================================================
-- Description : Identifie précisément les 15 inscriptions actives avec 
--               élèves inactifs qui persistent après correction
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- 1. LISTE DÉTAILLÉE DES 15 INSCRIPTIONS PROBLÉMATIQUES
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    i.IdEcole,
    i.IdClasse,
    i.Type AS TypeInscription,
    i.StatutInscription,
    -- Statut Inscription (détaillé)
    i.Statut AS StatutInscriptionRaw,
    CASE 
        WHEN i.Statut IS NULL THEN 'NULL'
        WHEN i.Statut = 0 THEN '0 (FALSE)'
        WHEN i.Statut = 1 THEN '1 (TRUE)'
        ELSE CONCAT('AUTRE: ', i.Statut)
    END AS StatutInscriptionDetail,
    -- Statut Élève (détaillé)
    e.Statut AS StatutEleveRaw,
    CASE 
        WHEN e.Statut IS NULL THEN 'NULL'
        WHEN e.Statut = 0 THEN '0 (FALSE)'
        WHEN e.Statut = 1 THEN '1 (TRUE)'
        ELSE CONCAT('AUTRE: ', e.Statut)
    END AS StatutEleveDetail,
    -- Informations élève
    e.NomComplet AS NomCompletEleve,
    e.Matricule AS MatriculeEleve,
    e.DateCreation AS DateCreationEleve,
    -- Informations école/classe
    ec.Nom AS NomEcole,
    c.NomClasse AS NomClasse,
    -- Date inscription
    i.DateInscription,
    i.DateCreation AS DateCreationInscription,
    -- Condition de détection
    CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) AND (e.Statut = 0 OR e.Statut IS NULL) 
        THEN 'DÉTECTÉ PAR LA CONDITION'
        ELSE 'NON DÉTECTÉ'
    END AS ConditionDetection
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    LEFT JOIN Ecoles ec ON i.IdEcole = ec.IdEcole
    LEFT JOIN Classes c ON i.IdClasse = c.IdClasse
WHERE 
    -- Inscription active
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    -- Élève inactif
    (e.Statut = 0 OR e.Statut IS NULL)
ORDER BY 
    ec.Nom,
    e.NomComplet,
    i.DateInscription DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- 2. ANALYSE PAR TYPE DE STATUT (NULL vs 0 vs 1)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'ANALYSE PAR TYPE DE STATUT' AS TypeAnalyse,
    -- Statut Inscription
    CASE 
        WHEN i.Statut IS NULL THEN 'NULL'
        WHEN i.Statut = 0 THEN '0 (FALSE)'
        WHEN i.Statut = 1 THEN '1 (TRUE)'
        ELSE 'AUTRE'
    END AS StatutInscriptionType,
    -- Statut Élève
    CASE 
        WHEN e.Statut IS NULL THEN 'NULL'
        WHEN e.Statut = 0 THEN '0 (FALSE)'
        WHEN e.Statut = 1 THEN '1 (TRUE)'
        ELSE 'AUTRE'
    END AS StatutEleveType,
    COUNT(*) AS NombreInscriptions
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
GROUP BY 
    StatutInscriptionType, StatutEleveType
ORDER BY 
    NombreInscriptions DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- 3. VÉRIFICATION : Les élèves sont-ils vraiment inactifs ?
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    e.IdEleve,
    e.NomComplet,
    e.Matricule,
    e.Statut AS StatutEleveRaw,
    CASE 
        WHEN e.Statut IS NULL THEN 'NULL'
        WHEN e.Statut = 0 THEN '0 (FALSE - INACTIF)'
        WHEN e.Statut = 1 THEN '1 (TRUE - ACTIF)'
        ELSE CONCAT('AUTRE: ', e.Statut)
    END AS StatutEleveDetail,
    COUNT(i.IdInscription) AS NombreInscriptionsActives,
    GROUP_CONCAT(i.IdInscription ORDER BY i.IdInscription SEPARATOR ', ') AS IdsInscriptions
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
GROUP BY 
    e.IdEleve, e.NomComplet, e.Matricule, e.Statut
ORDER BY 
    NombreInscriptionsActives DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- 4. VÉRIFICATION : Y a-t-il des inscriptions avec Statut = 0 mais considérées comme actives ?
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'VÉRIFICATION STATUT INSCRIPTION' AS TypeVerification,
    COUNT(*) AS NombreInscriptionsAvecStatut0,
    'Ces inscriptions ont Statut = 0 mais sont peut-être considérées comme actives' AS Note
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    i.Statut = 0  -- Explicitement inactif
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
    AND
    -- Vérifier si elles apparaissent dans les comptages comme actives
    EXISTS (
        SELECT 1 
        FROM Inscriptions i2 
        WHERE i2.IdInscription = i.IdInscription 
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
    );

-- ═══════════════════════════════════════════════════════════════════════════
-- 5. TEST DE LA CONDITION DE DÉTECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'TEST CONDITION' AS TypeTest,
    COUNT(*) AS TotalInscriptionsTestees,
    SUM(CASE WHEN (i.Statut = 1 OR i.Statut IS NULL) THEN 1 ELSE 0 END) AS InscriptionsActivesSelonCondition,
    SUM(CASE WHEN (e.Statut = 0 OR e.Statut IS NULL) THEN 1 ELSE 0 END) AS ElevesInactifsSelonCondition,
    SUM(CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND (e.Statut = 0 OR e.Statut IS NULL) 
        THEN 1 
        ELSE 0 
    END) AS IncoherencesDetectees
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve;

-- ═══════════════════════════════════════════════════════════════════════════
-- 6. VÉRIFICATION : Les inscriptions ont-elles été modifiées récemment ?
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    e.NomComplet,
    i.Statut AS StatutInscription,
    e.Statut AS StatutEleve,
    i.DateInscription,
    i.DateCreation AS DateCreationInscription,
    e.DateCreation AS DateCreationEleve,
    DATEDIFF(NOW(), i.DateCreation) AS JoursDepuisCreationInscription,
    DATEDIFF(NOW(), e.DateCreation) AS JoursDepuisCreationEleve
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
ORDER BY 
    i.DateCreation DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- 7. SCRIPT DE CORRECTION CIBLÉ POUR LES 15 INSCRIPTIONS
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ Ce script désactive UNIQUEMENT les 15 inscriptions identifiées
-- ⚠️ Faire un backup avant d'exécuter
-- 

-- D'abord, voir ce qui sera modifié :
SELECT 
    'INSCRIPTIONS QUI SERONT DÉSACTIVÉES' AS Action,
    i.IdInscription,
    i.IdEleve,
    e.NomComplet,
    i.Statut AS StatutAvant,
    0 AS StatutApres
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- Décommentez pour exécuter la correction :
/*
START TRANSACTION;

UPDATE Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
SET i.Statut = 0
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Vérification immédiate
SELECT 
    'VÉRIFICATION APRÈS CORRECTION' AS Etape,
    COUNT(*) AS NombreIncoherencesRestantes
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- Si le résultat est 0, validez :
COMMIT;

-- Sinon, annulez :
-- ROLLBACK;
*/

-- ============================================================================
-- NOTES
-- ============================================================================
-- 
-- Ce script permet de :
-- 1. Identifier précisément les 15 inscriptions problématiques
-- 2. Comprendre pourquoi elles ne sont pas détectées/corrigées
-- 3. Vérifier les valeurs exactes de Statut (NULL, 0, 1)
-- 4. Corriger uniquement ces 15 inscriptions
-- 
-- ============================================================================
