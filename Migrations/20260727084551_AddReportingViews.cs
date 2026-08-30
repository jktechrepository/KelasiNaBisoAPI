using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelasiNaBiso.Migrations
{
    /// <summary>
    /// Source de vérité des vues de reporting (ex-CreateView* au démarrage).
    /// Idempotent : DROP IF EXISTS + CREATE VIEW.
    /// </summary>
    public partial class AddReportingViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `V_Utilisateur`;");
            migrationBuilder.Sql(@"
CREATE VIEW `V_Utilisateur` AS
SELECT
    u.IdUtilisateur,
    u.ReferenceUtilisateur,
    u.NomUtilisateur,
    u.PostNomUtilisateur,
    u.PrenomUtilisateur,
    a.Matricule As Email,
    u.Telephone,
    u.PhotoUrl,
    u.LieuNaissance,
    u.DateNaissance,
    u.Genre,
    u.Statut,
    u.DateCreation,
    u.IsConnecte,
    u.Province,
    u.Ville,
    u.Commune,
    u.Quartier,
    u.Avenue,
    u.Numero,
    r.IdRole,
    a.Fonction AS NomRole,
    r.DateCreation AS DateCreationRole,
    e.IdEcole,
    e.Nom AS NomEcole,
    e.Slogan AS SloganEcole,
    e.Type AS TypeEcole,
    e.Logo AS LogoUrl,
    e.Telephone AS TelephoneEcole,
    e.EmailContact AS EmailContactEcole,
    e.SiteWeb AS SiteWebEcole,
    e.ProvinceEducationnel,
    e.NomCompletResponsable,
    e.Description AS DescriptionEcole,
    e.DateCreation AS DateCreationEcole,
    e.Province AS ProvinceEcole,
    e.Ville AS VilleEcole,
    e.Commune AS CommuneEcole,
    e.Quartier AS QuartierEcole,
    e.Avenue AS AvenueEcole,
    e.Numero AS NumeroEcole
FROM Utilisateurs u
LEFT JOIN agents a ON u.Email = a.EmailAgent
LEFT JOIN Roles r ON u.IdRole = r.IdRole
LEFT JOIN Ecoles e ON u.IdEcole = e.IdEcole
WHERE u.idRole <> 4 AND a.SerialNumber IS NULL;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `EleveParEcole`;");
            migrationBuilder.Sql(@"
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
    c.IdClasse,
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
    t.Statut AS StatutTuteur,
    t.PhotoTuteurUrl,
    t.PieceIdentiteTuteur,
    ec.IdEcole,
    ec.Nom AS NomEcole,
    ec.Slogan,
    ec.Longitute,
    ec.Latitude,
    ec.Type,
    ec.Logo AS LogoUrl,
    ec.Telephone AS TelephoneEcole,
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
LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
WHERE e.SerialNumber IS NULL AND e.STATUT IS TRUE;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `V_Eleve`;");
            migrationBuilder.Sql(@"
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
    c.IdClasse,
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
    ec.IdEcole,
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
LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Sections s ON c.IdSection = s.IdSection
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
LEFT JOIN Ecoles ec ON t.IdEcole = ec.IdEcole;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `VuePaiementsFraisParEcole`;");
            migrationBuilder.Sql(@"
CREATE VIEW `VuePaiementsFraisParEcole` AS
SELECT DISTINCT
    p.IdPaiement,
    p.DatePaiement,
    p.Montant,
    p.Devise,
    p.ModePaiement,
    p.Statut AS StatutPaiement,
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
INNER JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Sections s ON c.IdSection = s.IdSection
INNER JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Options o ON c.IdOption = o.IdOption
INNER JOIN Frais f ON p.IdFrais = f.IdFrais
INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `VuePointagePresenceParEcole`;");
            migrationBuilder.Sql(@"
CREATE VIEW `VuePointagePresenceParEcole` AS
SELECT DISTINCT
    p.IdPresence,
    p.DateDuJour,
    p.HeureArrivee,
    p.HeureDepart,
    p.Statut AS StatutPresence,
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
    v.IdVacation,
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
INNER JOIN Classes c ON e.IdClasse = c.IdClasse
LEFT JOIN Options o ON c.IdOption = o.IdOption
LEFT JOIN Sections s ON c.IdSection = s.IdSection
INNER JOIN Directions d ON c.IdDirection = d.IdDirection
LEFT JOIN Vacations v ON p.IdVacation = v.IdVacation
INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole;
");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `Vue_RepertoireAgentsParParent`;");
            migrationBuilder.Sql(@"
CREATE VIEW `Vue_RepertoireAgentsParParent` AS
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
WHERE el.Statut = 'True' AND tut.Statut = 'True' AND ac.Statut = 1;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `Vue_RepertoireAgentsParParent`;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `VuePointagePresenceParEcole`;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `VuePaiementsFraisParEcole`;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `V_Eleve`;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `EleveParEcole`;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS `V_Utilisateur`;");
        }
    }
}
