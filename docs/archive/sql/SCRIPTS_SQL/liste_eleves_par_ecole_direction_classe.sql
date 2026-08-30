-- ============================================================================
-- SCRIPT SQL : Liste des Élèves par École, Direction, Classe
-- ============================================================================
-- Description : Affiche les informations des élèves avec leurs tuteurs,
--               groupé et trié par École, Direction, Classe
-- 
-- Champs affichés :
--   - NomEleve, PostnomEleve, PrenomEleve
--   - AdresseEleve (concaténée)
--   - NomTuteur, TelephoneTuteur
--   - NomEcole, NomDirection, NomClasse
-- 
-- Date : 2025-01-16
-- ============================================================================

-- ═══════════════════════════════════════════════════════════════════════════
-- VERSION 1 : Liste détaillée (un élève par ligne)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Cette version affiche chaque élève sur une ligne séparée,
-- triée par École → Direction → Classe → NomEleve
-- 

SELECT 
    -- Informations École
    ec.IdEcole,
    ec.Nom AS NomEcole,
    
    -- Informations Direction
    d.IdDirection,
    d.NomDirection,
    
    -- Informations Classe
    c.IdClasse,
    c.NomClasse,
    
    -- Informations Élève
    e.IdEleve,
    e.Nom AS NomEleve,
    e.Postnom AS PostnomEleve,
    e.Prenom AS PrenomEleve,
    e.Matricule,
    
    -- Adresse Élève (concaténée)
    CONCAT_WS(', ',
        NULLIF(CONCAT_WS(' ', e.Province, e.Ville), ''),
        NULLIF(e.Commune, ''),
        NULLIF(e.Quartier, ''),
        NULLIF(e.Avenue, ''),
        NULLIF(e.Numero, '')
    ) AS AdresseEleve,
    
    -- Adresse Élève (détaillée - pour référence)
    e.Province AS ProvinceEleve,
    e.Ville AS VilleEleve,
    e.Commune AS CommuneEleve,
    e.Quartier AS QuartierEleve,
    e.Avenue AS AvenueEleve,
    e.Numero AS NumeroEleve,
    
    -- Informations Tuteur
    t.IdTuteur,
    t.NomComplet AS NomTuteur,
    t.Telephone AS TelephoneTuteur,
    t.Email AS EmailTuteur,
    
    -- Statuts
    CASE WHEN e.Statut = 1 THEN 'Actif' ELSE 'Inactif' END AS StatutEleve,
    CASE WHEN t.Statut = 1 THEN 'Actif' ELSE 'Inactif' END AS StatutTuteur

FROM 
    Eleves e
    -- Jointure avec Classe
    LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
    -- Jointure avec Direction
    LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
    -- Jointure avec Ecole
    LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
    -- Jointure avec Tuteur
    LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur

WHERE 
    -- Filtrer uniquement les élèves actifs (optionnel - décommentez si nécessaire)
    -- e.Statut = 1
    
    -- Filtrer uniquement les tuteurs actifs (optionnel - décommentez si nécessaire)
    -- AND (t.Statut = 1 OR t.Statut IS NULL)

ORDER BY 
    -- Tri par École → Direction → Classe → NomEleve
    ec.Nom ASC,
    d.NomDirection ASC,
    c.NomClasse ASC,
    e.Nom ASC,
    e.Postnom ASC,
    e.Prenom ASC;

-- ═══════════════════════════════════════════════════════════════════════════
-- VERSION 2 : Liste avec comptage par groupe (École, Direction, Classe)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Cette version affiche le nombre d'élèves par École/Direction/Classe
-- puis la liste détaillée des élèves
-- 

-- D'abord, le résumé par groupe
SELECT 
    'RÉSUMÉ PAR GROUPE' AS Type,
    ec.IdEcole,
    ec.Nom AS NomEcole,
    d.IdDirection,
    d.NomDirection,
    c.IdClasse,
    c.NomClasse,
    COUNT(e.IdEleve) AS NombreEleves,
    COUNT(CASE WHEN e.Statut = 1 THEN 1 END) AS NombreElevesActifs,
    COUNT(CASE WHEN e.Statut = 0 OR e.Statut IS NULL THEN 1 END) AS NombreElevesInactifs
FROM 
    Eleves e
    LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
    LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
    LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
WHERE 
    c.IdClasse IS NOT NULL  -- Uniquement les élèves avec une classe assignée
GROUP BY 
    ec.IdEcole, ec.Nom,
    d.IdDirection, d.NomDirection,
    c.IdClasse, c.NomClasse
ORDER BY 
    ec.Nom ASC,
    d.NomDirection ASC,
    c.NomClasse ASC;

-- ═══════════════════════════════════════════════════════════════════════════
-- VERSION 3 : Liste simplifiée (colonnes essentielles uniquement)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Version compacte avec uniquement les colonnes demandées
-- 

SELECT 
    -- Groupement
    ec.Nom AS NomEcole,
    d.NomDirection,
    c.NomClasse,
    
    -- Élève
    e.Nom AS NomEleve,
    e.Postnom AS PostnomEleve,
    e.Prenom AS PrenomEleve,
    
    -- Adresse (format compact)
    CONCAT_WS(', ',
        NULLIF(CONCAT_WS(' ', e.Province, e.Ville), ''),
        NULLIF(e.Quartier, ''),
        NULLIF(e.Avenue, '')
    ) AS AdresseEleve,
    
    -- Tuteur
    t.NomComplet AS NomTuteur,
    t.Telephone AS TelephoneTuteur

FROM 
    Eleves e
    LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
    LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
    LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
    LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur

ORDER BY 
    ec.Nom,
    d.NomDirection,
    c.NomClasse,
    e.Nom,
    e.Postnom,
    e.Prenom;

-- ═══════════════════════════════════════════════════════════════════════════
-- VERSION 4 : Export pour Excel/CSV (format plat, séparé par virgules)
-- ═══════════════════════════════════════════════════════════════════════════
-- 
-- Format optimisé pour export Excel ou CSV
-- 

SELECT 
    ec.Nom AS 'École',
    d.NomDirection AS 'Direction',
    c.NomClasse AS 'Classe',
    e.Nom AS 'Nom Élève',
    e.Postnom AS 'Postnom Élève',
    e.Prenom AS 'Prénom Élève',
    e.Matricule AS 'Matricule',
    CONCAT_WS(', ',
        NULLIF(CONCAT_WS(' ', e.Province, e.Ville), ''),
        NULLIF(e.Commune, ''),
        NULLIF(e.Quartier, ''),
        NULLIF(e.Avenue, ''),
        NULLIF(e.Numero, '')
    ) AS 'Adresse Élève',
    t.NomComplet AS 'Nom Tuteur',
    t.Telephone AS 'Téléphone Tuteur',
    t.Email AS 'Email Tuteur',
    CASE WHEN e.Statut = 1 THEN 'Actif' ELSE 'Inactif' END AS 'Statut Élève'
FROM 
    Eleves e
    LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
    LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
    LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
    LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
ORDER BY 
    ec.Nom,
    d.NomDirection,
    c.NomClasse,
    e.Nom,
    e.Postnom,
    e.Prenom;

-- ============================================================================
-- NOTES IMPORTANTES
-- ============================================================================
-- 
-- 1. **GROUP BY vs ORDER BY** :
--    - Si vous voulez juste TRIER par École/Direction/Classe → Utilisez ORDER BY (VERSION 1, 3, 4)
--    - Si vous voulez COMPTER par groupe → Utilisez GROUP BY (VERSION 2)
-- 
-- 2. **Filtrage** :
--    - Par défaut, tous les élèves sont inclus (actifs et inactifs)
--    - Décommentez les lignes WHERE pour filtrer uniquement les actifs
-- 
-- 3. **Adresse concaténée** :
--    - Utilise CONCAT_WS pour éviter les virgules multiples
--    - NULLIF pour ignorer les champs vides
-- 
-- 4. **Performance** :
--    - Les LEFT JOIN garantissent que tous les élèves sont inclus,
--      même s'ils n'ont pas de classe/direction/école/tuteur assigné
--    - Ajoutez des INDEX sur les clés étrangères si nécessaire
-- 
-- 5. **Export** :
--    - VERSION 4 est optimisée pour export Excel/CSV
--    - Les noms de colonnes sont en français avec accents
-- 
-- ============================================================================
