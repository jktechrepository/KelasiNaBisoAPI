-- =============================================
-- Script de correction des vues pour LogoUrl
-- Corrige les incohérences entre les modèles et les vues
-- =============================================

-- 1. Recréer la vue V_Eleve avec LogoUrlEcole
DROP VIEW IF EXISTS V_Eleve;
GO

CREATE VIEW V_Eleve AS
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
    ec.Logo AS LogoUrlEcole,
    ec.Téléphone AS TelephoneEcole,
    ec.EmailContact AS EmailContactEcole,
    ec.SiteWeb AS SiteWebEcole,
    ec.ProvinceEducationnel AS ProvinceEducationnel,
    ec.NomCompletResponsable AS NomCompletResponsable,
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
GO

-- 2. Recréer la vue EleveParEcole avec LogoUrl
DROP VIEW IF EXISTS EleveParEcole;
GO

CREATE VIEW EleveParEcole AS
SELECT 
    -- Élève
    e.IdEleve,
    e.ReferenceEleve,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletEleve,
    e.Genre,
    e.DateNaissance,
    DATEDIFF(YEAR, e.DateNaissance, GETDATE()) AS Age,
    e.LieuNaissance,
    e.PhotoUrl,
    e.Nationalite,
    e.Matricule,
    e.Province AS ProvinceEleve,
    e.Ville AS VilleEleve,
    e.Commune AS CommuneEleve,
    e.Quartier AS QuartierEleve,
    e.Avenue AS AvenueEleve,
    e.Numero AS NumeroEleve,
    e.Commentaire,
    e.Statut,

    -- Classe
    c.IdClasse,
    c.NomClasse,

    -- Direction
    d.IdDirection,
    d.NomDirection,

    -- Option
    o.IdOption,
    o.NomOption,

    -- Tuteur
    t.IdTuteur,
    t.NomComplet AS NomCompletTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    t.Statut AS StatutTuteur,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,

    -- École
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute,
    ec.Latitude,
    ec.Type,
    ec.Logo AS LogoUrl,
    ec.Téléphone AS TelephoneEcole,
    ec.EmailContact,
    ec.SiteWeb,
    ec.ProvinceEducationnel,
    ec.NomCompletResponsable,
    ec.Description,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole

FROM Eleves e
LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole;
GO

-- 3. Recréer la vue VuePaiementsFraisParEcole avec LogoUrl
DROP VIEW IF EXISTS VuePaiementsFraisParEcole;
GO

CREATE VIEW VuePaiementsFraisParEcole AS
SELECT DISTINCT
    -- Paiement
    p.IdPaiement,
    p.DatePaiement,
    p.Montant,
    p.Devise,
    p.ModePaiement,
    p.Statut AS StatutPaiement,
    p.RéférenceTransaction,
    p.JustificatifUrl,
    p.Commentaire AS CommentairePaiement,
    p.DateEnregistrement,
    p.ReferencePaiemenet,
    p.DateCreation AS DateCreationPaiement,

    -- Élève
    e.IdEleve,
    e.ReferenceEleve,
    e.Prenom,
    e.Nom,
    e.Postnom,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormaté,
    e.NomComplet AS NomCompletOriginal,
    e.Genre,
    e.DateNaissance,
    DATEDIFF(YEAR, e.DateNaissance, GETDATE()) AS Age,
    e.LieuNaissance,
    e.PhotoUrl,
    e.Nationalite,
    e.Matricule,
    e.Province AS ProvinceEleve,
    e.Ville AS VilleEleve,
    e.Commune AS CommuneEleve,
    e.Quartier AS QuartierEleve,
    e.Avenue AS AvenueEleve,
    e.Numero AS NumeroEleve,
    e.Commentaire AS CommentaireEleve,
    e.Statut AS StatutEleve,
    e.DateCreation AS DateCreationEleve,

    -- Classe
    c.IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,

    -- Section
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,

    -- Direction
    d.IdDirection,
    d.NomDirection,
    d.DateCreation AS DateCreationDirection,

    -- Option
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,

    -- École
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute,
    ec.Latitude,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrl,
    ec.Téléphone AS TelephoneEcole,
    ec.EmailContact,
    ec.SiteWeb,
    ec.ProvinceEducationnel,
    ec.NomCompletResponsable,
    ec.Description AS DescriptionEcole,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole,
    ec.DateCréation AS DateCreationEcole,

    -- Frais
    f.IdFrais,
    f.LibelleFrais,
    f.Montant AS MontantFrais,
    f.Devise AS DeviseFrais,
    f.DateCreation AS DateCreationFrais,

    -- Tuteur
    t.IdTuteur,
    t.NomComplet AS NomTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    t.Statut AS StatutTuteur,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    t.DateCreation AS DateCreationTuteur

FROM Paiements p
INNER JOIN Eleves e ON p.IdEleve = e.IdEleve
INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
INNER JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Sections s ON c.IdSection = s.IdSection
INNER JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
INNER JOIN Frais f ON p.IdFrais = f.IdFrais
INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole;
GO

PRINT 'Toutes les vues ont été corrigées avec succès!';
