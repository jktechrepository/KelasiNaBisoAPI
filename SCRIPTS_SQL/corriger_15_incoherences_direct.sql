-- ============================================================================
-- SCRIPT SQL : Correction Directe des 15 Incohérences
-- ============================================================================
-- Description : Désactive les 15 inscriptions actives (Statut = 1) 
--               dont les élèves sont inactifs (Statut = 0)
-- 
-- ⚠️ AVERTISSEMENT : Faire un BACKUP avant d'exécuter
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATION - Voir les 15 inscriptions qui seront désactivées
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    e.NomComplet AS NomEleve,
    e.Matricule,
    i.Statut AS StatutInscriptionAvant,
    e.Statut AS StatutEleve,
    ec.Nom AS NomEcole,
    c.NomClasse AS NomClasse,
    i.DateInscription
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    LEFT JOIN Ecoles ec ON i.IdEcole = ec.IdEcole
    LEFT JOIN Classes c ON i.IdClasse = c.IdClasse
WHERE 
    i.Statut = 1  -- Inscription active (explicitement 1)
    AND 
    e.Statut = 0  -- Élève inactif (explicitement 0)
ORDER BY 
    ec.Nom,
    e.NomComplet;

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : RÉSUMÉ AVANT CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'AVANT CORRECTION' AS Etape,
    COUNT(*) AS NombreInscriptionsADesactiver
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    i.Statut = 1
    AND 
    e.Statut = 0;

-- Résultat attendu : 15

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : CORRECTION - Désactiver les 15 inscriptions
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ DÉCOMMENTEZ CETTE SECTION APRÈS AVOIR VALIDÉ L'ÉTAPE 1
-- ⚠️ ASSUREZ-VOUS D'AVOIR FAIT UN BACKUP
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Correction : Désactiver les inscriptions actives (Statut = 1) d'élèves inactifs (Statut = 0)
UPDATE Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
SET i.Statut = 0
WHERE 
    i.Statut = 1  -- Inscription active
    AND 
    e.Statut = 0; -- Élève inactif

-- Afficher le nombre de lignes affectées
SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Vérification immédiate
SELECT 
    'VÉRIFICATION IMMÉDIATE' AS Etape,
    COUNT(*) AS IncoherencesRestantes
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    i.Statut = 1
    AND 
    e.Statut = 0;

-- Si le résultat est 0, validez :
COMMIT;

-- Sinon, annulez :
-- ROLLBACK;
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : VÉRIFICATION APRÈS CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'APRÈS CORRECTION' AS Etape,
    COUNT(*) AS NombreInscriptionsActivesAvecElevesInactifs
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    i.Statut = 1
    AND 
    e.Statut = 0;

-- Résultat attendu : 0

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 5 : VÉRIFICATION FINALE (comme dans le script principal)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'VÉRIFICATION FINALE' AS Type,
    COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END) AS TotalElevesActifs,
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesActifs,
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND (e.Statut = 0 OR e.Statut IS NULL) 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesInactifs,
    CASE 
        WHEN COUNT(DISTINCT CASE 
            WHEN (i.Statut = 1 OR i.Statut IS NULL) 
            AND (e.Statut = 0 OR e.Statut IS NULL) 
            THEN i.IdInscription 
        END) = 0 
        THEN '✅ CORRECTION RÉUSSIE' 
        ELSE '❌ IL RESTE DES INCOHÉRENCES' 
    END AS StatutCorrection
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve;

-- Résultat attendu : 
-- - TotalInscriptionsActivesAvecElevesInactifs = 0
-- - StatutCorrection = '✅ CORRECTION RÉUSSIE'

-- ============================================================================
-- NOTES
-- ============================================================================
-- 
-- Ce script corrige uniquement les inscriptions avec :
-- - Statut = 1 (explicitement actif, pas NULL)
-- - Élève avec Statut = 0 (explicitement inactif, pas NULL)
-- 
-- Si vous avez aussi des cas avec NULL, utilisez le script principal :
-- corriger_incohérences_inscriptions_eleves_inactifs.sql
-- 
-- ============================================================================
