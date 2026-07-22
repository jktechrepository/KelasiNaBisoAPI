-- ============================================================================
-- SCRIPT SQL : Corriger les Élèves avec Plusieurs Inscriptions Actives
-- ============================================================================
-- Description : Désactive les inscriptions actives supplémentaires pour 
--               garantir qu'un élève n'a qu'une seule inscription active
--               (garde la plus récente, désactive les autres)
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
-- mysqldump -u [USER] -p [DATABASE_NAME] > backup_avant_correction_inscriptions_multiples_$(date +%Y%m%d_%H%M%S).sql
-- 
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1 : VÉRIFICATION - Identifier les élèves avec plusieurs inscriptions actives
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ EXÉCUTEZ CETTE REQUÊTE EN PREMIER pour voir ce qui sera modifié
-- 

SELECT 
    e.IdEleve,
    e.NomComplet,
    e.Matricule,
    COUNT(i.IdInscription) AS NombreInscriptionsActives,
    GROUP_CONCAT(
        CONCAT('Inscription #', i.IdInscription, ' (', i.Type, ' - ', i.StatutInscription, ' - ', i.DateInscription, ')')
        ORDER BY i.DateInscription DESC
        SEPARATOR ' | '
    ) AS DetailsInscriptionsActives
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    e.Statut = 1  -- Élève actif
    AND (i.Statut = 1 OR i.Statut IS NULL)  -- Inscription active
GROUP BY 
    e.IdEleve, e.NomComplet, e.Matricule
HAVING 
    COUNT(i.IdInscription) > 1  -- Uniquement les élèves avec plusieurs inscriptions actives
ORDER BY 
    NombreInscriptionsActives DESC,
    e.NomComplet;

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1B : RÉSUMÉ AVANT CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'AVANT CORRECTION' AS Etape,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesAvecPlusieursInscriptionsActives,
    SUM(nb_inscriptions - 1) AS NombreInscriptionsADesactiver
FROM (
    SELECT 
        e.IdEleve,
        COUNT(i.IdInscription) AS nb_inscriptions
    FROM 
        Eleves e
        INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
    WHERE 
        e.Statut = 1
        AND (i.Statut = 1 OR i.Statut IS NULL)
    GROUP BY 
        e.IdEleve
    HAVING 
        COUNT(i.IdInscription) > 1
) AS stats;

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 1C : DÉTAIL - Inscriptions qui seront désactivées (garder la plus récente)
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    i.IdInscription,
    i.IdEleve,
    e.NomComplet AS NomEleve,
    e.Matricule,
    i.Type AS TypeInscription,
    i.StatutInscription,
    i.DateInscription,
    i.Statut AS StatutAvant,
    0 AS StatutApres,
    CASE 
        WHEN i.IdInscription = (
            SELECT i2.IdInscription
            FROM Inscriptions i2
            WHERE i2.IdEleve = i.IdEleve
            AND (i2.Statut = 1 OR i2.Statut IS NULL)
            ORDER BY i2.DateInscription DESC, i2.IdInscription DESC
            LIMIT 1
        ) THEN 'GARDER (plus récente)'
        ELSE 'DÉSACTIVER'
    END AS Action
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL)
    AND EXISTS (
        SELECT 1
        FROM Inscriptions i2
        WHERE i2.IdEleve = i.IdEleve
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
        GROUP BY i2.IdEleve
        HAVING COUNT(i2.IdInscription) > 1
    )
ORDER BY 
    e.NomComplet,
    i.DateInscription DESC,
    i.IdInscription DESC;

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2 : CORRECTION - Désactiver les inscriptions supplémentaires
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⚠️ DÉCOMMENTEZ CETTE SECTION UNIQUEMENT APRÈS AVOIR VALIDÉ L'ÉTAPE 1
-- ⚠️ ASSUREZ-VOUS D'AVOIR FAIT UN BACKUP
-- 
-- LOGIQUE : Garde la plus récente inscription active par élève, désactive les autres
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Méthode : Désactiver toutes les inscriptions actives sauf la plus récente par élève
UPDATE Inscriptions i
SET i.Statut = 0
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)  -- Inscription active
    AND EXISTS (
        -- Vérifier que l'élève a plusieurs inscriptions actives
        SELECT 1
        FROM Inscriptions i2
        WHERE i2.IdEleve = i.IdEleve
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
        GROUP BY i2.IdEleve
        HAVING COUNT(i2.IdInscription) > 1
    )
    AND i.IdInscription NOT IN (
        -- Garder la plus récente inscription active par élève
        SELECT IdInscription
        FROM (
            SELECT 
                i3.IdInscription,
                i3.IdEleve,
                ROW_NUMBER() OVER (
                    PARTITION BY i3.IdEleve 
                    ORDER BY i3.DateInscription DESC, i3.IdInscription DESC
                ) AS rn
            FROM Inscriptions i3
            INNER JOIN Eleves e3 ON i3.IdEleve = e3.IdEleve
            WHERE 
                e3.Statut = 1  -- Élève actif
                AND (i3.Statut = 1 OR i3.Statut IS NULL)  -- Inscription active
        ) AS ranked
        WHERE rn = 1
    );

-- Afficher le nombre de lignes affectées
SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Vérification immédiate
SELECT 
    'VÉRIFICATION IMMÉDIATE' AS Etape,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesAvecPlusieursInscriptionsActives
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL)
GROUP BY 
    e.IdEleve
HAVING 
    COUNT(i.IdInscription) > 1;

-- Si le résultat est 0, validez :
COMMIT;

-- Sinon, annulez :
-- ROLLBACK;
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 2B : CORRECTION - Version alternative (sans ROW_NUMBER, compatible MySQL < 8.0)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- ⭐ RECOMMANDÉ : Utilisez cette méthode si vous n'êtes pas sûr de votre version MySQL
-- 

-- Décommentez pour exécuter :

/*
START TRANSACTION;

-- Étape 1 : Créer une table temporaire avec les IDs des inscriptions à GARDER (plus récente par élève)
CREATE TEMPORARY TABLE temp_inscriptions_a_garder AS
SELECT 
    i.IdInscription,
    i.IdEleve
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve
WHERE 
    e.Statut = 1  -- Élève actif
    AND (i.Statut = 1 OR i.Statut IS NULL)  -- Inscription active
    AND i.IdInscription = (
        -- Sous-requête : Trouver l'ID de l'inscription la plus récente pour cet élève
        SELECT i2.IdInscription
        FROM Inscriptions i2
        WHERE i2.IdEleve = i.IdEleve
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
        ORDER BY i2.DateInscription DESC, i2.IdInscription DESC
        LIMIT 1
    );

-- Vérification : Afficher combien d'inscriptions seront gardées
SELECT COUNT(*) AS NombreInscriptionsAGarder FROM temp_inscriptions_a_garder;

-- Étape 2 : Désactiver toutes les inscriptions actives SAUF celles à garder
UPDATE Inscriptions i
SET i.Statut = 0
WHERE 
    (i.Statut = 1 OR i.Statut IS NULL)  -- Inscription active
    AND EXISTS (
        -- Vérifier que l'élève est actif
        SELECT 1
        FROM Eleves e
        WHERE e.IdEleve = i.IdEleve
        AND e.Statut = 1
    )
    AND i.IdInscription NOT IN (
        -- Exclure les inscriptions à garder
        SELECT IdInscription FROM temp_inscriptions_a_garder
    )
    AND EXISTS (
        -- Vérifier que l'élève a plusieurs inscriptions actives (pour éviter de désactiver si l'élève n'a qu'une inscription)
        SELECT 1
        FROM Inscriptions i2
        WHERE i2.IdEleve = i.IdEleve
        AND (i2.Statut = 1 OR i2.Statut IS NULL)
        GROUP BY i2.IdEleve
        HAVING COUNT(i2.IdInscription) > 1
    );

-- Afficher le nombre de lignes affectées
SELECT ROW_COUNT() AS NombreInscriptionsDesactivees;

-- Étape 3 : Supprimer la table temporaire
DROP TEMPORARY TABLE temp_inscriptions_a_garder;

-- Vérification immédiate : Plus aucun élève ne devrait avoir plusieurs inscriptions actives
SELECT 
    'VÉRIFICATION IMMÉDIATE' AS Etape,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesAvecPlusieursInscriptionsActives
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL)
GROUP BY 
    e.IdEleve
HAVING 
    COUNT(i.IdInscription) > 1;

-- Résultat attendu : 0

-- Si le résultat est 0, validez :
COMMIT;

-- Sinon, annulez :
-- ROLLBACK;
*/

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 3 : VÉRIFICATION APRÈS CORRECTION
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'APRÈS CORRECTION' AS Etape,
    COUNT(DISTINCT e.IdEleve) AS NombreElevesAvecPlusieursInscriptionsActives
FROM 
    Eleves e
    INNER JOIN Inscriptions i ON e.IdEleve = i.IdEleve
WHERE 
    e.Statut = 1
    AND (i.Statut = 1 OR i.Statut IS NULL)
GROUP BY 
    e.IdEleve
HAVING 
    COUNT(i.IdInscription) > 1;

-- Résultat attendu : 0 (aucun élève ne devrait avoir plusieurs inscriptions actives)

-- ═══════════════════════════════════════════════════════════════════════════
-- ÉTAPE 4 : VÉRIFICATION FINALE - Comparaison globale
-- ═══════════════════════════════════════════════════════════════════════════

SELECT 
    'VÉRIFICATION FINALE' AS Type,
    -- Nombre total d'élèves actifs
    COUNT(DISTINCT CASE WHEN e.Statut = 1 THEN e.IdEleve END) AS TotalElevesActifs,
    -- Nombre d'inscriptions actives avec élèves actifs (devrait être = TotalElevesActifs)
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN i.IdInscription 
    END) AS TotalInscriptionsActivesAvecElevesActifs,
    -- Nombre d'élèves actifs ayant des inscriptions actives
    COUNT(DISTINCT CASE 
        WHEN (i.Statut = 1 OR i.Statut IS NULL) 
        AND e.Statut = 1 
        THEN e.IdEleve 
    END) AS TotalElevesActifsAvecInscriptionsActives,
    -- Différence (devrait être 0 après correction)
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
    -- Statut de la correction
    CASE 
        WHEN COUNT(DISTINCT CASE 
            WHEN (i.Statut = 1 OR i.Statut IS NULL) 
            AND e.Statut = 1 
            THEN i.IdInscription 
        END) 
        = 
        COUNT(DISTINCT CASE 
            WHEN (i.Statut = 1 OR i.Statut IS NULL) 
            AND e.Statut = 1 
            THEN e.IdEleve 
        END)
        THEN '✅ CORRECTION RÉUSSIE : 1 inscription active par élève'
        ELSE '❌ IL RESTE DES ÉLÈVES AVEC PLUSIEURS INSCRIPTIONS ACTIVES'
    END AS StatutCorrection
FROM 
    Inscriptions i
    INNER JOIN Eleves e ON i.IdEleve = e.IdEleve;

-- Résultat attendu : 
-- - TotalInscriptionsActivesAvecElevesActifs = TotalElevesActifsAvecInscriptionsActives
-- - DifferenceInscriptionsEleves = 0
-- - StatutCorrection = '✅ CORRECTION RÉUSSIE : 1 inscription active par élève'

-- ============================================================================
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- LOGIQUE DE CORRECTION :
-- - Pour chaque élève actif avec plusieurs inscriptions actives
-- - Garde la plus récente (basée sur DateInscription, puis IdInscription en cas d'égalité)
-- - Désactive toutes les autres inscriptions actives
-- 
-- MÉTHODES DISPONIBLES :
-- - ÉTAPE 2 : Utilise ROW_NUMBER() (nécessite MySQL 8.0+)
-- - ÉTAPE 2B : Utilise une table temporaire (compatible toutes versions MySQL)
-- 
-- RECOMMANDATION : Utiliser l'ÉTAPE 2B si vous n'êtes pas sûr de votre version MySQL
-- 
-- ============================================================================
