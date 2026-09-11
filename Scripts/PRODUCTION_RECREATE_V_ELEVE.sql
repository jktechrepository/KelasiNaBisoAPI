-- ==========================================================
-- PRODUCTION — Recréer V_Eleve (+ EleveParEcole)
-- Base : knb_db (ou toute base où Eleves.IdClasse a été supprimé)
--
-- Symptôme :
--   View 'knb_db.v_eleve' references invalid table(s) or column(s)...
--   Souvent après DROP de Eleves.IdClasse alors que la vue pointe encore
--   vers e.IdClasse (ancien schéma).
--
-- Source de vérité :
--   Migrations/20260728140000_DropEleveIdClasse.cs
--
-- Prérequis : exécuter sur la BONNE base (USE knb_db;).
-- Idempotent : DROP VIEW IF EXISTS puis CREATE VIEW.
-- ==========================================================

-- 0) Contexte
SELECT DATABASE() AS base_courante;

-- 1) Diagnostic rapide
SELECT
  CASE
    WHEN COUNT(*) = 0 THEN 'OK — Eleves.IdClasse absent (attendu)'
    ELSE 'ATTENTION — Eleves.IdClasse existe encore'
  END AS statut_idclasse_eleves
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Eleves'
  AND COLUMN_NAME = 'IdClasse';

SHOW FULL TABLES WHERE Table_type = 'VIEW';

-- ==========================================================
-- 2) Recréer EleveParEcole (classe via Inscription)
-- ==========================================================
DROP VIEW IF EXISTS `EleveParEcole`;

CREATE VIEW `EleveParEcole` AS
SELECT
    e.IdEleve,
    e.ReferenceEleve,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletEleve,
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
    e.Commentaire,
    e.Statut,
    ins.IdClasse AS IdClasse,
    c.NomClasse,
    d.IdDirection,
    d.NomDirection,
    o.IdOption,
    o.NomOption,
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
    ins.IdEcole AS IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan AS SloganEcole,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrlEcole,
    ec.Telephone AS TelephoneEcole,
    ec.EmailContact AS EmailContactEcole,
    ec.SiteWeb AS SiteWebEcole,
    ec.ProvinceEducationnel AS ProvinceEducationnel,
    ec.NomCompletResponsable AS NomCompletResponsable,
    ec.Description AS DescriptionEcole,
    ec.DateCreation AS DateCreationEcole,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole
FROM Eleves e
LEFT JOIN (
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
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole
WHERE e.SerialNumber IS NULL AND e.STATUT IS TRUE;

-- ==========================================================
-- 3) Recréer V_Eleve (classe via Inscription — plus de e.IdClasse)
-- ==========================================================
DROP VIEW IF EXISTS `V_Eleve`;
DROP VIEW IF EXISTS `v_eleve`;

CREATE VIEW `V_Eleve` AS
SELECT
    e.IdEleve,
    e.ReferenceEleve,
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
    e.Province,
    e.Ville,
    e.Commune,
    e.Quartier,
    e.Avenue,
    e.Numero,
    ins.IdClasse AS IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,
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
    ins.IdEcole AS IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan AS SloganEcole,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrlEcole,
    ec.Telephone AS TelephoneEcole,
    ec.EmailContact AS EmailContactEcole,
    ec.SiteWeb AS SiteWebEcole,
    ec.ProvinceEducationnel AS ProvinceEducationnel,
    ec.NomCompletResponsable AS NomCompletResponsable,
    ec.Description AS DescriptionEcole,
    ec.DateCreation AS DateCreationEcole,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole
FROM Eleves e
LEFT JOIN (
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
LEFT JOIN Sections s ON c.IdSection = s.IdSection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON ins.IdEcole = ec.IdEcole;

-- ==========================================================
-- 4) Vérifications
-- ==========================================================
SELECT COUNT(*) AS nb_lignes_v_eleve FROM V_Eleve LIMIT 1;
SELECT IdEleve, NomComplet, IdClasse, NomClasse, IdEcole, NomEcole
FROM V_Eleve
ORDER BY IdEleve
LIMIT 5;
