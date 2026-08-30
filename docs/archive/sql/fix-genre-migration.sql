-- Script pour résoudre le problème de migration du champ Genre
-- Ce script doit être exécuté avant d'appliquer la migration

-- 1. Supprimer la vue V_Utilisateur existante si elle existe
IF EXISTS (SELECT * FROM sys.views WHERE name = 'V_Utilisateur')
BEGIN
    DROP VIEW V_Utilisateur
    PRINT 'Vue V_Utilisateur supprimée avec succès'
END

-- 2. Vérifier si la colonne Genre existe dans la table Utilisateurs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Utilisateurs') AND name = 'Genre')
BEGIN
    -- Ajouter la colonne Genre si elle n'existe pas
    ALTER TABLE Utilisateurs ADD Genre NVARCHAR(MAX) NULL
    PRINT 'Colonne Genre ajoutée à la table Utilisateurs'
END
ELSE
BEGIN
    PRINT 'Colonne Genre existe déjà dans la table Utilisateurs'
END

-- 3. Recréer la vue V_Utilisateur avec le champ Genre
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
    e.CapaciteEleve AS CapaciteEleveEcole,
    e.NombreSalles AS NombreSallesEcole,
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

PRINT 'Vue V_Utilisateur recréée avec succès avec le champ Genre'
