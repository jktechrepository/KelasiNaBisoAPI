-- =============================================================================
-- KelasiNaBiso — SQL équivalent GET /api/Eleve/ecole/{idEcole}
--
-- Endpoint : GET /api/Eleve/ecole/{idEcole}?page=&pageSize=&idAnneeScolaire=
-- Code     : EleveController.GetElevesByEcole
--            → EleveService.GetByEcoleAsync
--            → InscriptionActiveResolver.FilterElevesInEcole
--
-- Filtres API (tous obligatoires pour apparaître dans la liste) :
--   • Eleves.Statut = 1
--   • Inscriptions.Statut = 1
--   • Inscriptions.IdEcole = @idEcole
--   • Inscriptions.IdAnneeScolaire = @idAnneeScolaire (résolu ou passé en query)
--   • Inscriptions.StatutInscription = 'Confirmé' | 'Confirme' | LIKE 'Confirm%'
--
-- Usage phpMyAdmin : modifier les 4 variables ci-dessous, puis exécuter section
-- par section ou le fichier entier.
-- =============================================================================

USE knb_db;

SET @idEcole = 28;
SET @idAnneeScolaire = 19;
SET @page = 1;
SET @pageSize = 15;
SET @offset = (@page - 1) * @pageSize;

-- =============================================================================
-- 0. Contexte année scolaire
-- =============================================================================
SELECT IdAnneeScolaire, LibelleAnneeScolaire, DateDebut, DateFin, Statut
FROM AnneeScolaires
WHERE IdEcole = @idEcole AND IdAnneeScolaire = @idAnneeScolaire;

-- Année que l'API choisirait si idAnneeScolaire est OMIT en query
SET @now = NOW();
SELECT 'En session (API default)' AS Cas, a.*
FROM AnneeScolaires a
WHERE a.IdEcole = @idEcole AND a.Statut = 1
  AND a.DateDebut <= @now AND a.DateFin >= @now
UNION ALL
SELECT 'Prochaine vacances (API default)', a.*
FROM AnneeScolaires a
WHERE a.IdEcole = @idEcole AND a.Statut = 1
  AND a.DateDebut > @now
  AND EXISTS (
      SELECT 1 FROM AnneeScolaires prev
      WHERE prev.IdEcole = @idEcole AND prev.Statut = 1 AND prev.DateFin < @now
  )
ORDER BY DateDebut;

-- =============================================================================
-- 1. REQUÊTE PRINCIPALE — équivalent exact API (page courante)
-- Note : LIMIT/OFFSET via PREPARE (MySQL n'accepte pas LIMIT @var directement)
-- =============================================================================
SET @sql_main = CONCAT(
    'SELECT ',
    'e.IdEleve, e.ReferenceEleve, e.Matricule, e.Nom, e.Postnom, e.Prenom, ',
    'e.NomComplet, e.Genre, e.DateNaissance, e.PhotoUrl, e.Statut, e.IdTuteur, ',
    't.NomComplet AS NomCompletTuteur, t.Telephone AS TelephoneTuteur, ',
    'i.IdInscription, i.IdClasse, c.NomClasse, i.IdAnneeScolaire, ',
    'a.LibelleAnneeScolaire, i.IdEcole ',
    'FROM Eleves e ',
    'INNER JOIN Inscriptions i ON i.IdEleve = e.IdEleve ',
    'LEFT JOIN Tuteurs t ON t.IdTuteur = e.IdTuteur ',
    'LEFT JOIN Classes c ON c.IdClasse = i.IdClasse ',
    'LEFT JOIN AnneeScolaires a ON a.IdAnneeScolaire = i.IdAnneeScolaire ',
    'WHERE e.Statut = 1 AND i.Statut = 1 ',
    'AND i.IdEcole = ', @idEcole, ' ',
    'AND i.IdAnneeScolaire = ', @idAnneeScolaire, ' ',
    'AND i.StatutInscription IS NOT NULL ',
    'AND (i.StatutInscription = ''Confirmé'' OR i.StatutInscription = ''Confirme'' ',
    'OR i.StatutInscription LIKE ''Confirm%'') ',
    'ORDER BY e.NomComplet ',
    'LIMIT ', @pageSize, ' OFFSET ', @offset
);
PREPARE stmt_main FROM @sql_main;
EXECUTE stmt_main;
DEALLOCATE PREPARE stmt_main;

-- =============================================================================
-- 2. totalCount — doit correspondre à pagination.totalCount dans la réponse JSON
-- =============================================================================
SELECT COUNT(DISTINCT e.IdEleve) AS totalCount
FROM Eleves e
INNER JOIN Inscriptions i ON i.IdEleve = e.IdEleve
WHERE
    e.Statut = 1
    AND i.Statut = 1
    AND i.IdEcole = @idEcole
    AND i.IdAnneeScolaire = @idAnneeScolaire
    AND i.StatutInscription IS NOT NULL
    AND (
        i.StatutInscription = 'Confirmé'
        OR i.StatutInscription = 'Confirme'
        OR i.StatutInscription LIKE 'Confirm%'
    );

-- =============================================================================
-- 3. DIAGNOSTIC A — toutes inscriptions école/année (sans filtre confirmé)
-- =============================================================================
SELECT
    i.IdInscription,
    i.IdEleve,
    i.StatutInscription,
    i.Statut AS InscStatut,
    e.Statut AS EleveStatut,
    e.NomComplet
FROM Inscriptions i
JOIN Eleves e ON e.IdEleve = i.IdEleve
WHERE i.IdEcole = @idEcole AND i.IdAnneeScolaire = @idAnneeScolaire
ORDER BY e.NomComplet;

-- =============================================================================
-- 4. DIAGNOSTIC A — inscriptions EXCLUES par l'API (cause liste vide)
-- =============================================================================
SELECT
    i.IdInscription,
    i.IdEleve,
    e.NomComplet,
    i.StatutInscription,
    i.Statut AS InscStatut,
    e.Statut AS EleveStatut,
    CASE
        WHEN e.Statut IS NULL OR e.Statut = 0 THEN 'Eleve inactif'
        WHEN i.Statut IS NULL OR i.Statut = 0 THEN 'Inscription inactive'
        WHEN i.StatutInscription IS NULL THEN 'StatutInscription NULL'
        WHEN i.StatutInscription NOT IN ('Confirmé', 'Confirme')
             AND i.StatutInscription NOT LIKE 'Confirm%' THEN CONCAT('Statut non confirmé: ', i.StatutInscription)
        ELSE 'OK'
    END AS RaisonExclusion
FROM Inscriptions i
JOIN Eleves e ON e.IdEleve = i.IdEleve
WHERE i.IdEcole = @idEcole AND i.IdAnneeScolaire = @idAnneeScolaire
  AND NOT (
    e.Statut = 1 AND i.Statut = 1
    AND i.StatutInscription IS NOT NULL
    AND (i.StatutInscription IN ('Confirmé', 'Confirme') OR i.StatutInscription LIKE 'Confirm%')
  );

-- =============================================================================
-- 5. DIAGNOSTIC D — distribution StatutInscription par année (école)
-- =============================================================================
SELECT
    i.IdAnneeScolaire,
    a.LibelleAnneeScolaire,
    i.StatutInscription,
    COUNT(*) AS Nb
FROM Inscriptions i
LEFT JOIN AnneeScolaires a ON a.IdAnneeScolaire = i.IdAnneeScolaire
WHERE i.IdEcole = @idEcole
GROUP BY i.IdAnneeScolaire, a.LibelleAnneeScolaire, i.StatutInscription
ORDER BY i.IdAnneeScolaire, Nb DESC;

-- =============================================================================
-- 6. Résumé rapide
-- =============================================================================
SELECT
    (SELECT COUNT(*) FROM Inscriptions i
     WHERE i.IdEcole = @idEcole AND i.IdAnneeScolaire = @idAnneeScolaire) AS InscriptionsTotal,
    (SELECT COUNT(DISTINCT e.IdEleve) FROM Eleves e
     INNER JOIN Inscriptions i ON i.IdEleve = e.IdEleve
     WHERE e.Statut = 1 AND i.Statut = 1
       AND i.IdEcole = @idEcole AND i.IdAnneeScolaire = @idAnneeScolaire
       AND i.StatutInscription IS NOT NULL
       AND (i.StatutInscription IN ('Confirmé','Confirme') OR i.StatutInscription LIKE 'Confirm%')
    ) AS ElevesApiEquivalent;
