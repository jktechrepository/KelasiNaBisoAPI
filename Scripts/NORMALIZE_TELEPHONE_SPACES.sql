-- =============================================================================
-- Normalisation des numéros de téléphone (suppression espaces et tirets)
-- Format cible : +243XXXXXXXXX
-- Exécuter manuellement après revue des doublons (SELECT preview + doublons).
--
-- MySQL Workbench "safe update mode" : les UPDATE ci-dessous utilisent un JOIN
-- sur la clé primaire (IdUtilisateur / IdTuteur / IdAgent) pour être acceptés
-- sans désactiver SQL_SAFE_UPDATES.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1) Utilisateurs — aperçu
-- -----------------------------------------------------------------------------
SELECT IdUtilisateur, Telephone,
       REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelephoneNormalise
FROM Utilisateurs
WHERE Telephone IS NOT NULL
  AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '');

-- Doublons potentiels après normalisation
SELECT REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelNorm, COUNT(*) AS Nb
FROM Utilisateurs
WHERE Telephone IS NOT NULL AND TRIM(Telephone) <> ''
GROUP BY TelNorm
HAVING COUNT(*) > 1;

-- Mise à jour (JOIN sur IdUtilisateur — compatible safe update mode)
UPDATE Utilisateurs u
INNER JOIN (
    SELECT IdUtilisateur,
           REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelNorm
    FROM Utilisateurs
    WHERE Telephone IS NOT NULL
      AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '')
) src ON u.IdUtilisateur = src.IdUtilisateur
SET u.Telephone = src.TelNorm;

-- -----------------------------------------------------------------------------
-- 2) Tuteurs — Telephone + TelephoneRepresentant
-- -----------------------------------------------------------------------------
SELECT IdTuteur, Telephone, TelephoneRepresentant,
       REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelephoneNormalise,
       REPLACE(REPLACE(TRIM(TelephoneRepresentant), ' ', ''), '-', '') AS TelephoneRepresentantNormalise
FROM Tuteurs
WHERE (Telephone IS NOT NULL
       AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', ''))
   OR (TelephoneRepresentant IS NOT NULL
       AND TelephoneRepresentant <> REPLACE(REPLACE(TRIM(TelephoneRepresentant), ' ', ''), '-', ''));

SELECT REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelNorm, COUNT(*) AS Nb
FROM Tuteurs
WHERE Telephone IS NOT NULL AND TRIM(Telephone) <> ''
GROUP BY TelNorm
HAVING COUNT(*) > 1;

UPDATE Tuteurs t
INNER JOIN (
    SELECT IdTuteur,
           REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '') AS TelNorm
    FROM Tuteurs
    WHERE Telephone IS NOT NULL
      AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '')
) src ON t.IdTuteur = src.IdTuteur
SET t.Telephone = src.TelNorm;

UPDATE Tuteurs t
INNER JOIN (
    SELECT IdTuteur,
           REPLACE(REPLACE(TRIM(TelephoneRepresentant), ' ', ''), '-', '') AS TelNorm
    FROM Tuteurs
    WHERE TelephoneRepresentant IS NOT NULL
      AND TelephoneRepresentant <> REPLACE(REPLACE(TRIM(TelephoneRepresentant), ' ', ''), '-', '')
) src ON t.IdTuteur = src.IdTuteur
SET t.TelephoneRepresentant = src.TelNorm;

-- -----------------------------------------------------------------------------
-- 3) Agents — TelephoneAgent
-- -----------------------------------------------------------------------------
SELECT IdAgent, TelephoneAgent,
       REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '') AS TelephoneAgentNormalise
FROM Agents
WHERE TelephoneAgent IS NOT NULL
  AND TelephoneAgent <> REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '');

SELECT REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '') AS TelNorm, COUNT(*) AS Nb
FROM Agents
WHERE TelephoneAgent IS NOT NULL AND TRIM(TelephoneAgent) <> ''
GROUP BY TelNorm
HAVING COUNT(*) > 1;

UPDATE Agents a
INNER JOIN (
    SELECT IdAgent,
           REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '') AS TelNorm
    FROM Agents
    WHERE TelephoneAgent IS NOT NULL
      AND TelephoneAgent <> REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '')
) src ON a.IdAgent = src.IdAgent
SET a.TelephoneAgent = src.TelNorm;

-- -----------------------------------------------------------------------------
-- 4) Vérification post-update (attendu : 0 pour chaque compteur)
-- -----------------------------------------------------------------------------
SELECT COUNT(*) AS UtilisateursNonNormalises
FROM Utilisateurs
WHERE Telephone IS NOT NULL
  AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '');

SELECT COUNT(*) AS TuteursTelephoneNonNormalise
FROM Tuteurs
WHERE Telephone IS NOT NULL
  AND Telephone <> REPLACE(REPLACE(TRIM(Telephone), ' ', ''), '-', '');

SELECT COUNT(*) AS TuteursTelephoneRepresentantNonNormalise
FROM Tuteurs
WHERE TelephoneRepresentant IS NOT NULL
  AND TelephoneRepresentant <> REPLACE(REPLACE(TRIM(TelephoneRepresentant), ' ', ''), '-', '');

SELECT COUNT(*) AS AgentsTelephoneNonNormalise
FROM Agents
WHERE TelephoneAgent IS NOT NULL
  AND TelephoneAgent <> REPLACE(REPLACE(TRIM(TelephoneAgent), ' ', ''), '-', '');
