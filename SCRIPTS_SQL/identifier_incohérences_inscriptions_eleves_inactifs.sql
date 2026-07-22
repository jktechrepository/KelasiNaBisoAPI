-- ============================================================================
-- SCRIPT SQL : Identifier les Incohérences entre Inscriptions et Élèves
-- ============================================================================
-- Description : Trouve toutes les inscriptions actives (Statut = true) 
--              dont l'élève associé est inactif (Statut = false)
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- 1. INSCRIPTIONS ACTIVES AVEC ÉLÈVES INACTIFS (PROBLÈME PRINCIPAL)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    i.IdEcole,
    i.IdClasse,
    i.Type AS TypeInscription,
    i.StatutInscription,
    i.Statut AS StatutInscriptionBool,
    i.DateInscription,
    i.DateCreation AS DateCreationInscription,
    e.NomComplet AS NomCompletEleve,
    e.Matricule AS MatriculeEleve,
    e.Statut AS StatutEleve,
    e.DateCreation AS DateCreationEleve,
    ec.Nom AS NomEcole,
    c.NomClasse AS NomClasse,
    CASE 
        WHEN e.Statut = 0 OR e.Statut IS NULL THEN 'INACTIF'
        WHEN e.Statut = 1 THEN 'ACTIF'
        ELSE 'INCONNU'
    END AS StatutEleveLibelle,
    CASE 
        WHEN i.Statut = 0 OR i.Statut IS NULL THEN 'INACTIF'
        WHEN i.Statut = 1 THEN 'ACTIF'
        ELSE 'INCONNU'
    END AS StatutInscriptionLibelle
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    LEFT JOIN Ecoles ec ON i.IdEcole = ec.IdEcole
    LEFT JOIN Classes c ON i.IdClasse = c.IdClasse
WHERE 
    -- Inscription active
    (i.Statut = 1 OR i.Statut IS NULL)  -- NULL est considéré comme actif (valeur par défaut)
    AND 
    -- Élève inactif
    (e.Statut = 0 OR e.Statut IS NULL)   -- NULL est considéré comme inactif (à vérifier selon votre logique)
ORDER BY 
    i.DateInscription DESC,
    ec.Nom,
    e.NomComplet;

-- ═══════════════════════════════════════════════════════════════════════════
-- 2. STATISTIQUES PAR ÉCOLE (VUE D'ENSEMBLE)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    ec.IdEcole,
    ec.Nom AS NomEcole,
    COUNT(DISTINCT i.IdInscription) AS NombreInscriptionsActivesAvecElevesInactifs,
    COUNT(DISTINCT i.IdEleve) AS NombreElevesInactifsConcernes,
    GROUP_CONCAT(DISTINCT e.NomComplet ORDER BY e.NomComplet SEPARATOR ', ') AS ListeElevesInactifs
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
    INNER JOIN Ecoles ec ON i.IdEcole = ec.IdEcole
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL)
GROUP BY 
    ec.IdEcole, ec.Nom
ORDER BY 
    NombreInscriptionsActivesAvecElevesInactifs DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- 3. COMPARAISON : NOMBRE D'ÉLÈVES ACTIFS VS NOMBRE D'INSCRIPTIONS ACTIVES
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    ec.IdEcole,
    ec.Nom AS NomEcole,
    -- Élèves actifs
    COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END) AS NombreElevesActifs,
    -- Inscriptions actives (tous élèves confondus)
    COUNT(DISTINCT CASE WHEN (i.Statut = 1 OR i.Statut IS NULL) THEN i.IdInscription END) AS NombreInscriptionsActives,
    -- Inscriptions actives avec élèves actifs uniquement
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) AS NombreInscriptionsActivesAvecElevesActifs,
    -- Inscriptions actives avec élèves inactifs (PROBLÈME)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND (e.Statut = 0 OR e.Statut IS NULL) 
        THEN i.IdInscription 
    END) AS NombreInscriptionsActivesAvecElevesInactifs,
    -- Différence (devrait être 0)
    COUNT(DISTINCT CASE WHEN (i.Statut = 1 OR i.Statut IS NULL) THEN i.IdInscription END) 
    - COUNT(DISTINCT CASE WHEN (i.Statut = 1 OR i.Statut IS NULL) AND e.Statut = 1 THEN i.IdInscription END) 
    AS DifferenceIncoherence
FROM 
    Ecoles ec
    LEFT JOIN Inscriptions i ON ec.IdEcole = i.IdEcole
    LEFT JOIN Eleves e ON i.IdEleve = e.IdEleve
GROUP BY 
    ec.IdEcole, ec.Nom
HAVING 
    -- Afficher uniquement les écoles avec incohérences
    DifferenceIncoherence > 0
ORDER BY 
    DifferenceIncoherence DESC,
    ec.Nom;

-- ═══════════════════════════════════════════════════════════════════════════
-- 4. DÉTAIL PAR ÉLÈVE : Nombre d'inscriptions actives pour élèves inactifs
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    e.IdEleve,
    e.NomComplet,
    e.Matricule,
    e.Statut AS StatutEleve,
    COUNT(i.IdInscription) AS NombreInscriptionsActives,
    GROUP_CONCAT(
        CONCAT('Inscription #', i.IdInscription, ' (', i.Type, ' - ', i.StatutInscription, ')')
        ORDER BY i.DateInscription DESC
        SEPARATOR ' | '
    ) AS DetailsInscriptions
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
    NombreInscriptionsActives DESC,
    e.NomComplet;

-- ═══════════════════════════════════════════════════════════════════════════
-- 5. RÉSUMÉ GLOBAL
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'RÉSUMÉ GLOBAL' AS Type,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesInactifsAvecInscriptionsActives,
    COUNT(DISTINCT i.IdInscription) AS NombreTotalInscriptionsActivesAvecElevesInactifs,
    COUNT(DISTINCT i.IdEcole) AS NombreEcolesConcernées
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- ============================================================================
-- NOTES IMPORTANTES
-- ============================================================================
-- 1. Ce script identifie les incohérences mais NE LES CORRIGE PAS
-- 2. Avant d'exécuter des corrections, faire un BACKUP de la base de données
-- 3. Vérifier la logique métier : 
--    - Un élève inactif peut-il avoir des inscriptions actives ?
--    - Si non, les inscriptions doivent être désactivées automatiquement
-- 4. Les valeurs NULL sont traitées selon votre logique métier :
--    - NULL pour Statut = true (actif) par défaut dans le modèle
--    - À adapter selon votre configuration
-- ============================================================================
