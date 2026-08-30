-- Audit cohérence modèle Inscription
-- (post DropEleveIdClasse / DropTuteurIdEcole — classe/école uniquement via Inscriptions)

-- Élèves actifs sans inscription active confirmée
SELECT e.IdEleve, e.NomComplet
FROM Eleves e
WHERE e.Statut = 1
  AND NOT EXISTS (
    SELECT 1 FROM Inscriptions i
    WHERE i.IdEleve = e.IdEleve
      AND i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
  );

-- Élèves actifs dont la classe/école ne peut pas être résolue via inscription active
SELECT e.IdEleve, e.NomComplet
FROM Eleves e
WHERE e.Statut = 1
  AND NOT EXISTS (
    SELECT 1 FROM Inscriptions i
    WHERE i.IdEleve = e.IdEleve
      AND i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
      AND i.IdClasse IS NOT NULL
      AND i.IdEcole IS NOT NULL
  );

-- Tuteurs actifs sans aucun enfant inscrit (confirmé) dans une école
SELECT t.IdTuteur, t.NomComplet
FROM Tuteurs t
WHERE t.Statut = 1
  AND NOT EXISTS (
    SELECT 1
    FROM Eleves e
    INNER JOIN Inscriptions i ON i.IdEleve = e.IdEleve
    WHERE e.IdTuteur = t.IdTuteur
      AND e.Statut = 1
      AND i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
  );

-- Doublons d'inscriptions actives même élève / même année
SELECT i.IdEleve, i.IdAnneeScolaire, COUNT(*) AS NbActives
FROM Inscriptions i
WHERE i.Statut = 1
GROUP BY i.IdEleve, i.IdAnneeScolaire
HAVING COUNT(*) > 1;
