-- =============================================================================
-- KelasiNaBiso — Recréation Vue_RepertoireAgentsParParent via Inscriptions
-- Date : 2026-09-18
-- =============================================================================
-- Contexte :
--   Après DropEleveIdClasse, Eleves.IdClasse n'existe plus.
--   Les anciennes définitions (migration AddReportingViews, archive
--   rename-enseignant-to-agent.sql) joignaient encore Classes via el.IdClasse
--   → erreur MySQL 1356 / vue invalide.
--
-- Correctif :
--   - Classe / école via inscription « active »
--     (Statut=1, StatutInscription Confirmé%, MAX(DateInscription)) — même
--     logique que PRODUCTION_RECREATE_V_ELEVE.sql / V_Eleve.
--   - Cours sur ins.IdClasse ; AffectationsCours filtrées sur
--     ac.IdAnneeScolaire = ins.IdAnneeScolaire et ac.Statut = 1.
--   - Année via an.IdAnneeScolaire = ac.IdAnneeScolaire (plus de JOIN an.IdEcole).
--   - Téléphone agent : Agents.TelephoneAgent (pas Adresse.Numero).
--
-- Colonnes exposées = VueRepertoireAgentsParParentDTO (pas de breaking change API).
--
-- Prérequis : Eleves, Tuteurs, Inscriptions, Classes, Cours, AffectationsCours,
--   Agents, Ecoles, AnneeScolaires.
--
-- Apply manuel sur knb_db / dev-knb_db. Idempotent (DROP + CREATE).
-- =============================================================================

USE knb_db;

DROP VIEW IF EXISTS `Vue_RepertoireAgentsParParent`;

CREATE VIEW `Vue_RepertoireAgentsParParent` AS
SELECT
    e.IdAgent,
    CONCAT(e.Nom, ' ', e.Postnom, ' ', e.Prenom) AS NomCompletAgent,
    e.Genre AS GenreAgent,
    e.TelephoneAgent AS TelephoneAgent,
    e.EmailAgent,
    e.PhotoUrl AS PhotoAgent,
    e.DateCreation AS DateCreationAgent,

    c.IdCours,
    c.NomCours,
    c.Description AS DescriptionCours,
    c.DateCreation AS DateCreationCours,

    cl.IdClasse,
    cl.NomClasse,
    cl.DateCreation AS DateCreationClasse,

    an.IdAnneeScolaire,
    an.LibelleAnneeScolaire,
    an.DateDebut,
    an.DateFin,
    an.DateCreation AS DateCreationAnnee,

    el.IdEleve,
    el.NomComplet AS NomCompletEleve,
    el.Genre AS GenreEleve,
    el.Matricule,
    el.Statut AS StatutEleve,
    el.DateCreation AS DateCreationEleve,

    tut.IdTuteur,
    tut.NomComplet AS NomCompletTuteur,
    tut.Genre AS GenreTuteur,
    tut.Telephone AS TelephoneTuteur,
    tut.Email AS EmailTuteur,
    tut.NomCompletRepresentant,
    tut.TelephoneRepresentant,
    tut.Statut AS StatutTuteur,
    tut.DateCreation AS DateCreationTuteur,

    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Type AS TypeEcole,
    ec.DateCreation AS DateCreationEcole

FROM Eleves el
INNER JOIN Tuteurs tut ON el.IdTuteur = tut.IdTuteur
INNER JOIN (
    SELECT i.*
    FROM Inscriptions i
    INNER JOIN (
        SELECT IdEleve, MAX(DateInscription) AS MaxDate
        FROM Inscriptions
        WHERE Statut = 1
          AND (StatutInscription = 'Confirmé' OR StatutInscription = 'Confirme' OR StatutInscription LIKE 'Confirm%')
        GROUP BY IdEleve
    ) latest ON i.IdEleve = latest.IdEleve AND i.DateInscription = latest.MaxDate
    WHERE i.Statut = 1
      AND (i.StatutInscription = 'Confirmé' OR i.StatutInscription = 'Confirme' OR i.StatutInscription LIKE 'Confirm%')
) ins ON ins.IdEleve = el.IdEleve
INNER JOIN Classes cl ON ins.IdClasse = cl.IdClasse
INNER JOIN Cours c ON cl.IdClasse = c.IdClasse
INNER JOIN AffectationsCours ac
    ON c.IdCours = ac.IdCours
   AND ac.IdAnneeScolaire = ins.IdAnneeScolaire
INNER JOIN Agents e ON ac.IdAgent = e.IdAgent
INNER JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole
LEFT JOIN AnneeScolaires an ON an.IdAnneeScolaire = ac.IdAnneeScolaire
WHERE el.Statut = 1
  AND tut.Statut = 1
  AND ac.Statut = 1
  AND (c.Statut = 1 OR c.Statut IS NULL)
  AND (e.Statut = 1 OR e.Statut IS NULL);

-- Vérifications rapides
SHOW COLUMNS FROM `Vue_RepertoireAgentsParParent`
WHERE Field IN ('TelephoneAgent', 'IdClasse', 'IdAnneeScolaire', 'NomEcole', 'IdAgent');

SELECT COUNT(*) AS TotalLignes FROM `Vue_RepertoireAgentsParParent`;

SELECT IdAgent, NomCompletAgent, TelephoneAgent, NomCours, NomClasse,
       LibelleAnneeScolaire, Matricule, NomCompletTuteur, NomEcole
FROM `Vue_RepertoireAgentsParParent`
LIMIT 5;
