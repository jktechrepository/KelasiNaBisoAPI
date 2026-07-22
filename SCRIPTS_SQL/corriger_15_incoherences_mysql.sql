-- ============================================================================
-- SCRIPT SQL : Correction des 15 Incohérences (Syntaxe MySQL Compatible)
-- ============================================================================
-- Description : Désactive les 15 inscriptions actives (Statut = 1) 
--               dont les élèves sont inactifs (Statut = 0)
-- 
-- ⚠️ AVERTISSEMENT : Faire un BACKUP avant d'exécuter
-- 
-- Date : 2025-01-16
-- Version : 2.0 (Syntaxe MySQL corrigée)
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
-- ÉTAPE 3 : CORRECTION - Version avec sous-requête (Compatible MySQL)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ DÉCOMMENTEZ CETTE SECTION APRÈS AVOIR VALIDÉ L'ÉTAPE 1
-- ⚠️ ASSUREZ-VOUS D'AVOIR FAIT UN BACKUP
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Méthode 1 : UPDATE avec sous-requête (plus compatible)
UPDATE Inscriptions
SET Statut = 0
WHERE IdInscription IN (
    SELECT i.IdInscription
    FROM Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    WHERE 
        i.Statut = 1
        AND 
        e.Statut = 0
);

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
-- ÉTAPE 3B : CORRECTION - Version alternative avec table temporaire
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Si la méthode 1 ne fonctionne pas, utilisez cette méthode
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Créer une table temporaire avec les IDs à modifier
CREATE TEMPORARY TABLE temp_inscriptions_a_desactiver AS
SELECT i.IdInscription
FROM Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    i.Statut = 1
    AND 
    e.Statut = 0;

-- Mettre à jour les inscriptions
UPDATE Inscriptions
SET Statut = 0
WHERE IdInscription IN (
    SELECT IdInscription FROM temp_inscriptions_a_desactiver
);

-- Afficher le nombre de lignes affectées
SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Supprimer la table temporaire
DROP TEMPORARY TABLE temp_inscriptions_a_desactiver;

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
-- ÉTAPE 3C : CORRECTION - Version la plus simple (UPDATE direct)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Si les méthodes précédentes ne fonctionnent pas, utilisez cette version
-- qui utilise EXISTS (très compatible)
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Méthode 3 : UPDATE avec EXISTS (très compatible MySQL)
UPDATE Inscriptions i
SET i.Statut = 0
WHERE EXISTS (
    SELECT 1
    FROM Eleves e
    WHERE e.IdEleve = i.IdEleve
    AND e.Statut = 0
)
AND i.Statut = 1;

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
-- ÉTAPE 5 : VÉRIFICATION FINALE
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'VÉRIFICATION FINALE' AS Type,
    -- Nombre total d'élèves actifs (tous élèves actifs, avec ou sans inscription)
    COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END) AS TotalElevesActifs,
    -- Nombre d'inscriptions actives avec élèves actifs (peut être > TotalElevesActifs si certains élèves ont plusieurs inscriptions)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesActifs,
    -- Nombre d'élèves actifs ayant au moins une inscription active (pour comparaison)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN e.IdEleve 
    END) AS TotalElevesActifsAvecInscriptionsActives,
    -- Inscriptions actives avec élèves inactifs (devrait être 0 après correction)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND (e.Statut = 0 OR e.Statut IS NULL) 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesInactifs,
    -- Différence entre inscriptions et élèves (normal si > 0)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) 
    - 
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN e.IdEleve 
    END) AS DifferenceInscriptionsEleves,
    -- Statut de la correction (vérifie uniquement les inscriptions d'élèves inactifs)
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
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- Ce script propose 3 méthodes différentes de correction :
-- 
-- Méthode 1 (ÉTAPE 3) : UPDATE avec sous-requête IN
--   - Compatible avec la plupart des versions MySQL
--   - Peut avoir des problèmes avec certaines versions
-- 
-- Méthode 2 (ÉTAPE 3B) : UPDATE avec table temporaire
--   - Très compatible, fonctionne partout
--   - Plus verbeux mais plus sûr
-- 
-- Méthode 3 (ÉTAPE 3C) : UPDATE avec EXISTS
--   - Très compatible MySQL
--   - Syntaxe simple et claire
--   - RECOMMANDÉ si les autres ne fonctionnent pas
-- 
-- Essayez les méthodes dans l'ordre : 3C → 3 → 3B
-- 
-- ============================================================================
