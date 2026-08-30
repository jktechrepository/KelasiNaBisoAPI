-- Script pour mettre à jour la vue V_Utilisateur avec le champ Genre
-- Ce script doit être exécuté après l'ajout du champ Genre à la table Utilisateurs

-- Supprimer la vue existante si elle existe
IF EXISTS (SELECT * FROM sys.views WHERE name = 'V_Utilisateur')
BEGIN
    DROP VIEW V_Utilisateur
END

-- Créer la vue mise à jour avec le champ Genre
CREATE VIEW V_Utilisateur AS
SELECT 
    u.IdUtilisateur,
    u.ReferenceUtilisateur,
    u.NomUtilisateur,
    u.PostNomUtilisateur,
    u.PrenomUtilisateur,
    u.Email,
    u.Téléphone,
    u.PhotoUrl,
    u.LieuNaissance,
    u.DateNaissance,
    u.Genre,
    u.Statut,
    u.DateCreation,
    u.IsConnecte,
    
    -- Champs Adresse (hérités par Utilisateur)
    u.Province,
    u.Ville,
    u.Commune,
    u.Quartier,
    u.Avenue,
    u.Numero,
    
    -- Champs Role
    r.IdRole,
    r.NomRole,
    r.DateCreation AS DateCreationRole,
    
    -- Champs Ecole
    e.IdEcole,
    e.Nom AS NomEcole,
    e.Slogan AS SloganEcole,
    e.Type AS TypeEcole,
    e.LogoUrl AS LogoUrlEcole,
    e.Téléphone AS TéléphoneEcole,
    e.EmailContact AS EmailContactEcole,
    e.SiteWeb AS SiteWebEcole, 
    e.Description AS DescriptionEcole,
    e.DateCréation AS DateCréationEcole,
    
    -- Champs Adresse Ecole
    e.Province AS ProvinceEcole,
    e.Ville AS VilleEcole,
    e.Commune AS CommuneEcole,
    e.Quartier AS QuartierEcole,
    e.Avenue AS AvenueEcole,
    e.Numero AS NumeroEcole
    
FROM Utilisateurs u
LEFT JOIN Roles r ON u.IdRole = r.IdRole
LEFT JOIN Ecoles e ON u.IdEcole = e.IdEcole;
