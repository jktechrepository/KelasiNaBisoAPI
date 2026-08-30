-- ========================================
-- SCRIPT SQL: RENOMMER ENSEIGNANT → AGENT
-- Date: 17 Octobre 2025
-- ========================================

-- Étape 1: Supprimer la vue qui dépend de la table Enseignants
DROP VIEW IF EXISTS Vue_RepertoireEnseignantsParParent;

-- Étape 2: Renommer la table Enseignants en Agents
RENAME TABLE Enseignants TO Agents;

-- Étape 3: Renommer la colonne IdEnseignant en IdAgent dans la table Agents
ALTER TABLE Agents CHANGE COLUMN IdEnseignant IdAgent INT NOT NULL AUTO_INCREMENT;

-- Étape 4: Renommer la colonne TelephoneEnseignant en TelephoneAgent
ALTER TABLE Agents CHANGE COLUMN TelephoneEnseignant TelephoneAgent VARCHAR(255);

-- Étape 5: Renommer la colonne EmailEnseignant en EmailAgent
ALTER TABLE Agents CHANGE COLUMN EmailEnseignant EmailAgent VARCHAR(255);

-- Étape 6: Renommer la colonne IdEnseignant en IdAgent dans AffectationsCours
ALTER TABLE AffectationsCours CHANGE COLUMN IdEnseignant IdAgent INT NOT NULL;

-- Étape 7: Recréer la vue SQL avec le nouveau nom
CREATE VIEW Vue_RepertoireAgentsParParent AS
SELECT 
    e.IdAgent,
    CONCAT(e.Nom, ' ', e.Postnom, ' ', e.Prenom) AS NomCompletAgent,
    e.Genre AS GenreAgent,
    e.Numero AS TelephoneAgent,
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
INNER JOIN Classes cl ON el.IdClasse = cl.IdClasse
INNER JOIN Cours c ON cl.IdClasse = c.IdClasse
INNER JOIN AffectationsCours ac ON c.IdCours = ac.IdCours
INNER JOIN Agents e ON ac.IdAgent = e.IdAgent
INNER JOIN Directions d ON cl.IdDirection = d.IdDirection
INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
LEFT JOIN AnneeScolaires an ON an.IdEcole = ec.IdEcole
WHERE el.Statut = 1 AND tut.Statut = 1 AND ac.Statut = 1;

-- ========================================
-- FIN DU SCRIPT
-- ========================================

