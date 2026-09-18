-- =============================================================================
-- Recréation vue VuePaiementsFraisParEcole
-- Aligné post–DropEleveIdClasse : classe via Inscriptions (comme V_Eleve)
-- École via Frais.IdEcole
-- =============================================================================

DROP VIEW IF EXISTS `VuePaiementsFraisParEcole`;

CREATE VIEW `VuePaiementsFraisParEcole` AS
SELECT DISTINCT
    p.IdPaiement,
    p.DatePaiement,
    p.Montant,
    p.Devise,
    p.ModePaiement,
    p.StatutPaiement,
    p.ReferenceTransaction,
    p.JustificatifUrl,
    p.Commentaire AS CommentairePaiement,
    p.DateEnregistrement,
    p.ReferencePaiemenet,
    p.DateCreation AS DateCreationPaiement,
    e.IdEleve,
    e.ReferenceEleve,
    e.Prenom,
    e.Nom,
    e.Postnom,
    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormate,
    e.NomComplet AS NomCompletOriginal,
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
    e.Commentaire AS CommentaireEleve,
    e.Statut AS StatutEleve,
    e.DateCreation AS DateCreationEleve,
    c.IdClasse,
    c.NomClasse,
    c.DateCreation AS DateCreationClasse,
    s.IdSection,
    s.NomSection,
    s.DateCreation AS DateCreationSection,
    d.IdDirection,
    d.NomDirection,
    d.DateCreation AS DateCreationDirection,
    o.IdOption,
    o.NomOption,
    o.DateCreation AS DateCreationOption,
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute,
    ec.Latitude,
    ec.Type AS TypeEcole,
    ec.Logo AS LogoUrl,
    ec.Telephone AS TelephoneEcole,
    ec.EmailContact,
    ec.SiteWeb,
    ec.Description AS DescriptionEcole,
    ec.Province AS ProvinceEcole,
    ec.Ville AS VilleEcole,
    ec.Commune AS CommuneEcole,
    ec.Quartier AS QuartierEcole,
    ec.Avenue AS AvenueEcole,
    ec.Numero AS NumeroEcole,
    ec.DateCreation AS DateCreationEcole,
    f.IdFrais,
    f.LibelleFrais,
    f.Montant AS MontantFrais,
    f.Devise AS DeviseFrais,
    f.DateCreation AS DateCreationFrais,
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
LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
INNER JOIN Frais f ON p.IdFrais = f.IdFrais
INNER JOIN Ecoles ec ON f.IdEcole = ec.IdEcole;
