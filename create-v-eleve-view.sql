-- Script pour créer la vue V_Eleve
-- Exécutez ce script dans votre base de données SQL Server

CREATE OR ALTER VIEW V_Eleve AS
SELECT 
    e.IdEleve,
    e.ReferenceEleve,
    
    -- Champs Élève
    e.Matricule,
    e.Nom,
    e.Postnom,
    e.Prenom,
    e.NomComplet,
    e.Genre,
    e.DateNaissance,
    e.LieuNaissance,
    e.PhotoUrl,
    e.Nationalite,
    e.Commentaire,
    e.Statut,
    e.DateCreation,
    
    -- Champs Adresse Élève
    e.Province,
    e.Ville,
    e.Commune,
    e.Quartier,
    e.Avenue,
    e.Numero,
    
    -- Champs Classe
    c.IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,
    
    -- Champs Section
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,
    
    -- Champs Option
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,
    
    -- Champs Tuteur
    t.IdTuteur,
    t.NomComplet AS NomCompletTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    t.Statut AS StatutTuteur,
    t.DateCreation AS DateCreationTuteur,
    
    -- Champs École (via Tuteur)
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan AS SloganEcole,
    ec.Type AS TypeEcole,
    ec.LogoUrl AS LogoUrlEcole,
    ec.Téléphone AS TelephoneEcole,
    ec.EmailContact AS EmailContactEcole,
    ec.SiteWeb AS SiteWebEcole,
    ec.CapaciteEleve AS CapaciteEleveEcole, 
    ec.Description AS DescriptionEcole,
    ec.DateCréation AS DateCreationEcole,
    
    -- Champs Adresse École
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole
    
FROM Eleves e
LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Sections s ON c.IdSection = s.IdSection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON t.IdEcole = ec.IdEcole;
