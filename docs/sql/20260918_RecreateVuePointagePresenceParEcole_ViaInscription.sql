-- =============================================================================
-- KelasiNaBiso — Recréation VuePointagePresenceParEcole via Inscriptions
-- Date : 2026-09-18
-- =============================================================================
-- Contexte :
--   Après DropEleveIdClasse, Eleves.IdClasse n'existe plus.
--   Les anciennes définitions (migration AddReportingViews, archives
--   fix-vue-pointage-presence*.sql) joignaient encore Classes via e.IdClasse
--   → erreur MySQL 1356 / vue invalide.
--
-- Correctif :
--   - Classe / Section / Option / Direction via inscription « active »
--     (Statut=1, StatutInscription Confirmé%, MAX(DateInscription)) — même
--     logique que PRODUCTION_RECREATE_V_ELEVE.sql / V_Eleve.
--   - École via ins.IdEcole.
--   - Vacation via p.IdVacation ; exposé v.IdVacation AS IdHoraire (DTO).
--   - Périmètre élève inchangé (INNER JOIN Eleves + Tuteurs) — pas de pointages AGENT.
--
-- Colonnes exposées = VuePointagePresenceParEcoleDTO (pas de breaking change API).
--
-- Prérequis : Presences, Eleves, Tuteurs, Inscriptions, Classes, Options,
--   Sections, Directions, Ecoles, Vacations.
--
-- Apply manuel sur knb_db / dev-knb_db. Idempotent (DROP + CREATE).
-- =============================================================================

USE knb_db;

DROP VIEW IF EXISTS `VuePointagePresenceParEcole`;

CREATE VIEW `VuePointagePresenceParEcole` AS
SELECT DISTINCT
    p.IdPresence,
    p.DateDuJour,
    p.HeureArrivee,
    p.HeureDepart,
    CAST(p.Statut AS CHAR) AS StatutPresence,
    p.Longitute,
    p.Latitude,
    p.DateCreation AS DateCreationPresence,

    e.IdEleve,
    e.ReferenceEleve,
    e.Prenom,
    e.Nom,
    e.Postnom,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormate,
    e.Genre,
    e.DateNaissance,
    YEAR(CURDATE()) - YEAR(e.DateNaissance) AS Age,
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
    e.Statut AS StatutEleve,
    e.DateCreation AS DateCreationEleve,

    c.IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,

    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,

    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,

    d.IdDirection,
    d.NomDirection,
    d.DateCreation AS DateCreationDirection,

    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute AS LongituteEcole,
    ec.Latitude AS LatitudeEcole,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrl,
    ec.Telephone AS TelephoneEcole,
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
    ec.DateCreation AS DateCreationEcole,

    v.IdVacation AS IdHoraire,
    v.NomVacation,
    v.HeureDebut,
    v.HeureFin,
    v.HeureDebutPause,
    v.HeureFinPause,
    v.NombreJoursParSemaine,
    v.DateCreation AS DateCreationVacation,

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

FROM Presences p
INNER JOIN Eleves e ON p.IdEleve = e.IdEleve
INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
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
) ins ON ins.IdEleve = e.IdEleve
LEFT JOIN Classes c ON ins.IdClasse = c.IdClasse
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Sections s ON c.IdSection = s.IdSection
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Vacations v ON p.IdVacation = v.IdVacation
INNER JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole;

-- Vérifications rapides
SHOW COLUMNS FROM `VuePointagePresenceParEcole`
WHERE Field IN ('IdHoraire', 'IdClasse', 'NomEcole', 'StatutPresence', 'NomCompletFormate');

SELECT COUNT(*) AS TotalLignes FROM `VuePointagePresenceParEcole`;

SELECT IdPresence, Matricule, NomCompletFormate, IdClasse, NomClasse,
       NomEcole, IdHoraire, NomVacation, StatutPresence, DateDuJour
FROM `VuePointagePresenceParEcole`
LIMIT 5;
