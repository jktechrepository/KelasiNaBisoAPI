-- ============================================================================
-- SCRIPT SQL : Corriger les Incohérences entre Inscriptions et Élèves
-- ============================================================================
-- Description : Désactive toutes les inscriptions actives dont l'élève 
--               associé est inactif (cascade logicielle)
-- 
-- ⚠️ AVERTISSEMENT IMPORTANT :
--   1. FAIRE UN BACKUP COMPLET DE LA BASE DE DONNÉES AVANT D'EXÉCUTER CE SCRIPT
--   2. Exécuter d'abord la section 1 (VÉRIFICATION) pour voir ce qui sera modifié
--   3. Valider les résultats avant de décommenter la section 2 (CORRECTION)
--   4. Exécuter la section 3 (VÉRIFICATION APRÈS) pour confirmer la correction
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 0 : BACKUP (À FAIRE MANUELLEMENT)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Exécutez cette commande AVANT de continuer :
-- 
-- mysqldump -u [USER] -p [DATABASE_NAME] > backup_avant_correction_inscriptions_$(date +%Y%m%d_%H%M%S).sql
-- 
-- Ou via votre outil de gestion de base de données (phpMyAdmin, MySQL Workbench, etc.)
-- 
-- ═══════════════════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATION - Afficher les inscriptions qui seront désactivées
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ EXÉCUTEZ CETTE REQUÊTE EN PREMIER pour voir ce qui sera modifié
-- 

SELECT 
    i.IdInscription,
    i.IdEleve,
    i.IdEcole,
    i.IdClasse,
    i.Type AS TypeInscription,
    i.StatutInscription,
    i.Statut AS StatutInscriptionAvant,
    i.DateInscription,
    e.NomComplet AS NomEleve,
    e.Matricule AS MatriculeEleve,
    e.Statut AS StatutEleve,
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
    END AS StatutInscriptionLibelleAvant
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
-- ÉTAPE 1B : RÉSUMÉ AVANT CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'AVANT CORRECTION' AS Etape,
    COUNT(DISTINCT i.IdInscription) AS NombreInscriptionsADesactiver,
    COUNT(DISTINCT i.IdEleve) AS NombreElevesInactifsConcernes,
    COUNT(DISTINCT i.IdEcole) AS NombreEcolesConcernées
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : CORRECTION - Désactiver les inscriptions d'élèves inactifs
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ DÉCOMMENTEZ CETTE SECTION UNIQUEMENT APRÈS AVOIR VALIDÉ L'ÉTAPE 1
-- ⚠️ ASSUREZ-VOUS D'AVOIR FAIT UN BACKUP
-- 

-- Décommentez les lignes suivantes pour exécuter la correction :

/*
-- Début de la transaction pour pouvoir annuler si nécessaire
START TRANSACTION;

-- Mise à jour : Désactiver toutes les inscriptions actives d'élèves inactifs
UPDATE Inscriptions i
INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
SET i.Statut = 0
WHERE 
    -- Inscription active
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    -- Élève inactif
    (e.Statut = 0 OR e.Statut IS NULL);

-- Afficher le nombre de lignes affectées
SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Si tout est correct, validez la transaction :
COMMIT;

-- Si vous voulez annuler, utilisez :
-- ROLLBACK;
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : VÉRIFICATION APRÈS CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Exécutez cette requête APRÈS avoir exécuté la correction pour vérifier
-- qu'il n'y a plus d'incohérences
-- 

-- Vérification : Il ne devrait plus y avoir d'inscriptions actives avec élèves inactifs
SELECT 
    'APRÈS CORRECTION' AS Etape,
    COUNT(DISTINCT i.IdInscription) AS NombreInscriptionsActivesAvecElevesInactifs,
    COUNT(DISTINCT i.IdEleve) AS NombreElevesInactifsConcernes,
    COUNT(DISTINCT i.IdEcole) AS NombreEcolesConcernées
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)
    AND 
    (e.Statut = 0 OR e.Statut IS NULL);

-- Résultat attendu : 0 pour toutes les colonnes

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : STATISTIQUES PAR ÉCOLE APRÈS CORRECTION
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
    -- Inscriptions actives avec élèves inactifs (devrait être 0)
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
    -- Afficher uniquement les écoles avec incohérences (devrait être vide)
    DifferenceIncoherence > 0
ORDER BY 
    DifferenceIncoherence DESC,
    ec.Nom;

-- Résultat attendu : Aucune ligne (ou toutes les différences à 0)

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 5 : VÉRIFICATION FINALE - Comparaison globale
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
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- 1. BACKUP : Toujours faire un backup avant d'exécuter des scripts de correction
-- 
-- 2. VALEURS NULL : 
--    - NULL pour Statut est traité comme "actif" pour les inscriptions (valeur par défaut)
--    - NULL pour Statut est traité comme "inactif" pour les élèves (à vérifier selon votre logique)
--    - Adaptez les conditions WHERE si votre logique est différente
-- 
-- 3. TRANSACTION : Le script utilise START TRANSACTION pour pouvoir annuler si nécessaire
--    - Utilisez COMMIT pour valider
--    - Utilisez ROLLBACK pour annuler
-- 
-- 4. VÉRIFICATION : Exécutez toujours les étapes de vérification avant et après
-- 
-- 5. LOGIQUE MÉTIER : 
--    - Ce script désactive les inscriptions d'élèves inactifs
--    - Si vous réactivez un élève plus tard, ses inscriptions resteront inactives
--    - Vous devrez peut-être les réactiver manuellement si nécessaire
-- 
-- ============================================================================
