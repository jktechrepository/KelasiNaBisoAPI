-- ============================================================================
-- SCRIPT SQL : Analyser l'Incohérence entre Élèves Actifs et Inscriptions
-- ============================================================================
-- Description : Analyse pourquoi TotalInscriptionsActivesAvecElevesActifs 
--               peut être différent de TotalElevesActifs
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- 1. COMPARAISON DIRECTE : Élèves Actifs vs Inscriptions Actives
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'COMPARAISON DIRECTE' AS TypeAnalyse,
    -- Nombre d'élèves actifs (comptage direct)
    (SELECT COUNT(DISTINCT e.IdEleve) 
     FROM Eleves e 
     WHERE e.Statut = 1) AS TotalElevesActifsDirect,
    -- Nombre d'inscriptions actives avec élèves actifs (via JOIN)
    (SELECT COUNT(DISTINCT i.IdInscription)
     FROM Inscriptions i
     INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
     WHERE (i.Statut = 1 OR i.Statut IS NULL)
     AND e.Statut = 1) AS TotalInscriptionsActivesAvecElevesActifs,
    -- Nombre d'élèves actifs ayant au moins une inscription active
    (SELECT COUNT(DISTINCT e.IdEleve)
     FROM Eleves e
     INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
     WHERE e.Statut = 1
     AND (i.Statut = 1 OR i.Statut IS NULL)) AS TotalElevesActifsAvecInscriptionsActives,
    -- Différence
    (SELECT COUNT(DISTINCT i.IdInscription)
     FROM Inscriptions i
     INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
     WHERE (i.Statut = 1 OR i.Statut IS NULL)
     AND e.Statut = 1)
    -
    (SELECT COUNT(DISTINCT e.IdEleve) 
     FROM Eleves e 
     WHERE e.Statut = 1) AS Difference;

-- ═══════════════════════════════════════════════════════════════════════════
-- 2. ANALYSE : Élèves actifs SANS inscription active
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'ÉLÈVES ACTIFS SANS INSCRIPTION ACTIVE' AS TypeAnalyse,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesActifsSansInscriptionActive
FROM 
    Eleves e
WHERE 
    e.Statut = 1
    AND NOT EXISTS (
        SELECT 1
        FROM Inscriptions i
        WHERE i.IdEleve = e.IdEleve
        AND (i.Statut = 1 OR i.Statut IS NULL)
    );

-- ═══════════════════════════════════════════════════════════════════════════
-- 3. ANALYSE : Nombre d'inscriptions actives par élève actif
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'RÉPARTITION INSCRIPTIONS PAR ÉLÈVE' AS TypeAnalyse,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesActifsAvecInscriptions,
    COUNT(i.IdInscription) AS NombreTotalInscriptionsActives,
    ROUND(COUNT(i.IdInscription) / COUNT(DISTINCT e.IdEleve), 2) AS MoyenneInscriptionsParEleve,
    MIN(nb_inscriptions) AS MinInscriptionsParEleve,
    MAX(nb_inscriptions) AS MaxInscriptionsParEleve
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
    INNER JOIN (
        SELECT 
            e2.IdEleve,
            COUNT(i2.IdInscription) AS nb_inscriptions
        FROM Eleves e2
        INNER JOIN Inscriptions i2 ON e2.IdEleve = i2.IdEleve
        WHERE e2.Statut = 1
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
        GROUP BY e2.IdEleve
    ) AS stats ON e.IdEleve = stats.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL);

-- ═══════════════════════════════════════════════════════════════════════════
-- 4. DÉTAIL : Élèves actifs avec plusieurs inscriptions actives
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    e.IdEleve,
    e.NomComplet,
    e.Matricule,
    COUNT(i.IdInscription) AS NombreInscriptionsActives,
    GROUP_CONCAT(
        CONCAT('Inscription #', i.IdInscription, ' (', i.Type, ' - ', i.StatutInscription, ' - ', i.DateInscription, ')')
        ORDER BY i.DateInscription DESC
        SEPARATOR ' | '
    ) AS DetailsInscriptions
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL)
GROUP BY 
    e.IdEleve, e.NomComplet, e.Matricule
HAVING 
    COUNT(i.IdInscription) > 1  -- Uniquement les élèves avec plusieurs inscriptions
ORDER BY 
    NombreInscriptionsActives DESC,
    e.NomComplet
LIMIT 20;  -- Limiter à 20 pour ne pas surcharger

-- ═══════════════════════════════════════════════════════════════════════════
-- 5. VÉRIFICATION : La requête de vérification finale est-elle correcte ?
-- ═══════════════════════════════════════════════════════════════════════════

-- La requête actuelle compte les DISTINCT inscriptions, pas les DISTINCT élèves
-- C'est normal qu'il y ait plus d'inscriptions que d'élèves si certains élèves
-- ont plusieurs inscriptions actives

SELECT 
    'VÉRIFICATION LOGIQUE' AS TypeAnalyse,
    -- Nombre d'élèves actifs (comptage direct)
    COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END) AS TotalElevesActifs,
    -- Nombre d'inscriptions actives avec élèves actifs (comptage des inscriptions)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesActifs,
    -- Nombre d'élèves actifs ayant au moins une inscription active (comptage des élèves via inscriptions)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN e.IdEleve 
    END) AS TotalElevesActifsAvecInscriptionsActives,
    -- Explication
    CASE 
        WHEN COUNT(DISTINCT CASE 
            WHEN (i.Statut = 1 OR i.Statut IS NULL) 
            AND e.Statut = 1 
            THEN i.IdInscription 
        END) > COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END)
        THEN 'NORMAL : Certains élèves ont plusieurs inscriptions actives'
        WHEN COUNT(DISTINCT CASE 
            WHEN (i.Statut = 1 OR i.Statut IS NULL) 
            AND e.Statut = 1 
            THEN i.IdInscription 
        END) = COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END)
        THEN 'ÉGAL : Chaque élève a exactement une inscription active'
        ELSE 'ANORMAL : Moins d''inscriptions que d''élèves'
    END AS Explication
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve;

-- ═══════════════════════════════════════════════════════════════════════════
-- 6. CONCLUSION : Est-ce vraiment une incohérence ?
-- ═══════════════════════════════════════════════════════════════════════════

-- Si TotalInscriptionsActivesAvecElevesActifs > TotalElevesActifs :
-- C'est NORMAL car un élève peut avoir plusieurs inscriptions actives
-- (par exemple : une inscription par année scolaire, ou réinscription)

-- Si vous voulez comparer avec le nombre d'élèves actifs ayant des inscriptions :
-- Utilisez TotalElevesActifsAvecInscriptionsActives au lieu de TotalElevesActifs

SELECT 
    'CONCLUSION' AS TypeAnalyse,
    'TotalElevesActifs compte tous les élèves actifs (avec ou sans inscription)' AS Note1,
    'TotalInscriptionsActivesAvecElevesActifs compte toutes les inscriptions actives' AS Note2,
    'Un élève peut avoir plusieurs inscriptions actives (normal)' AS Note3,
    'Pour comparer, utilisez TotalElevesActifsAvecInscriptionsActives' AS Note4;

-- ============================================================================
-- NOTES
-- ============================================================================
-- 
-- Il est NORMAL que TotalInscriptionsActivesAvecElevesActifs > TotalElevesActifs
-- car :
-- 1. Un élève peut avoir plusieurs inscriptions actives (plusieurs années scolaires)
-- 2. Un élève peut être réinscrit (nouvelle inscription pour la même année)
-- 3. Un élève peut avoir des inscriptions dans différentes classes/écoles
-- 
-- Si vous voulez vérifier qu'il n'y a pas d'incohérence, comparez :
-- - TotalElevesActifsAvecInscriptionsActives (élèves actifs ayant des inscriptions)
-- - TotalInscriptionsActivesAvecElevesActifs (inscriptions actives)
-- 
-- Ces deux valeurs peuvent être différentes, mais c'est normal.
-- 
-- ============================================================================
