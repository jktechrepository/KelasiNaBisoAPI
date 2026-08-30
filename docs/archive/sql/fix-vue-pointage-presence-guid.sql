-- Script pour corriger la vue VuePointagePresenceParEcole
-- Problème : ReferenceEleve est de type GUID dans la base mais string dans le DTO
-- Solution : Convertir le GUID en string dans la vue

-- Supprimer la vue existante
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VuePointagePresenceParEcole')
    DROP VIEW VuePointagePresenceParEcole;

-- Recréer la vue avec la conversion GUID vers string
CREATE VIEW VuePointagePresenceParEcole AS
SELECT DISTINCT
    -- Présence
    p.IdPresence,
    p.DateDuJour,
    p.HeureArrivee,
    p.HeureDepart,
    p.Statut AS StatutPresence,
    p.Longitute,
    p.Latitude,
    p.DateCreation AS DateCreationPresence,

    -- Élève
    e.IdEleve,
    CAST(e.ReferenceEleve AS NVARCHAR(36)) AS ReferenceEleve, -- Conversion GUID vers string
    e.Prenom,
    e.Nom,
    e.Postnom,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormaté,
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
    CASE WHEN e.Statut = 1 THEN 'True' ELSE 'False' END AS StatutEleve, -- Conversion bool vers string
    e.DateCreation AS DateCreationEleve,

    -- Classe
    c.IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,

    -- Option
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,

    -- Section
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,

    -- Direction
    d.IdDirection,
    d.NomDirection,
    d.DateCreation AS DateCreationDirection,

    -- École
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute AS LongituteEcole,
    ec.Latitude AS LatitudeEcole,
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

    -- Horaire / Vacation
    v.IdHoraire,
    v.NomVacation,
    v.HeureDebut,
    v.HeureFin,
    v.HeureDebutPause,
    v.HeureFinPause,
    v.NombreJoursParSemaine,
    v.DateCreation AS DateCreationVacation,

    -- Tuteur
    t.IdTuteur,
    t.NomComplet AS NomTuteur,
    t.Genre AS GenreTuteur,
    t.Email AS EmailTuteur,
    t.Telephone AS TelephoneTuteur,
    t.NomCompletRepresentant,
    t.TelephoneRepresentant,
    CASE WHEN t.Statut = 1 THEN 'True' ELSE 'False' END AS StatutTuteur, -- Conversion bool vers string
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    t.DateCreation AS DateCreationTuteur

FROM Presences p
INNER JOIN Eleves e ON p.IdEleve = e.IdEleve
INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
INNER JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Sections s ON c.IdSection = s.IdSection
INNER JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Vacations v ON p.IdHoraire = v.IdHoraire
INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole;

PRINT 'Vue VuePointagePresenceParEcole recréée avec succès avec les conversions de type correctes.';
