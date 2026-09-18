using FastReport;
using FastReport.Export.PdfSimple;
using KelasiNaBiso.Models.DTOs.Bulletin;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using System.Globalization;
using System.Text;
using DataBand = FastReport.DataBand;
using TextObject = FastReport.TextObject;

namespace KelasiNaBiso.Services.Reporting
{
    public class BulletinReportService : IBulletinReportService
    {
        private readonly IBulletinService _bulletinService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BulletinReportService> _logger;

        public BulletinReportService(
            IBulletinService bulletinService,
            IWebHostEnvironment environment,
            ILogger<BulletinReportService> logger)
        {
            _bulletinService = bulletinService;
            _environment = environment;
            _logger = logger;
        }

        public async Task<byte[]> GetElevePdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default)
        {
            var bulletin = await _bulletinService.GetBulletinEleveAsync(
                idEleve, idAnneeScolaire, periode, idPeriode, cancellationToken);
            if (bulletin == null)
                throw new KeyNotFoundException($"Bulletin introuvable pour l'élève {idEleve}.");

            var entete = MapEntete(bulletin);
            var lignes = MapLignes(bulletin);
            var report = BuildReport(entete, lignes);
            return await ExportToPdfAsync(report, cancellationToken);
        }

        private Report BuildReport(BulletinEnteteReportDto entete, List<BulletinLigneReportDto> lignes)
        {
            var report = new Report();
            var templatePath = GetTemplatePath();

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (!LooksLikeBulletinTemplate(report))
                    {
                        _logger.LogWarning("Template {Path} invalide, fallback programmatique.", templatePath);
                        report = new Report();
                        BulletinReportLayoutBuilder.ApplyLayout(report, entete, lignes);
                        return report;
                    }

                    RegisterLignesData(report, lignes);
                    ApplyHeaderToTemplate(report, entete);
                    BindLignesBand(report);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Échec chargement {Path}, fallback programmatique.", templatePath);
                    report = new Report();
                    BulletinReportLayoutBuilder.ApplyLayout(report, entete, lignes);
                }
            }
            else
            {
                _logger.LogInformation("Template bulletin absent ({Path}), mise en page programmatique.", templatePath);
                BulletinReportLayoutBuilder.ApplyLayout(report, entete, lignes);
            }

            return report;
        }

        private static void RegisterLignesData(Report report, List<BulletinLigneReportDto> lignes)
        {
            report.RegisterData(new List<BulletinLigneReportDto>(lignes), "BulletinLignes");
            var ds = report.GetDataSource("BulletinLignes");
            if (ds != null)
                ds.Enabled = true;
        }

        private static void BindLignesBand(Report report)
        {
            var ds = report.GetDataSource("BulletinLignes");
            if (report.FindObject("LignesBand") is DataBand band)
            {
                band.DataSource = ds;
                band.Visible = true;
            }
            else if (report.FindObject("Data1") is DataBand data1)
            {
                data1.DataSource = ds;
                data1.Visible = true;
            }
        }

        private static void ApplyHeaderToTemplate(Report report, BulletinEnteteReportDto entete)
        {
            SetText(report, "txtEcole", entete.NomEcole);
            SetText(report, "txtEleve", entete.NomEleve);
            SetText(report, "txtClasse", entete.NomClasse);
            SetText(report, "txtAnnee", entete.LibelleAnnee);
            SetText(report, "txtPeriode", entete.Periode);
            SetText(report, "txtMoyenne", entete.MoyenneGeneraleTexte);
            SetText(report, "txtRang", entete.RangTexte);
            SetText(report, "txtEffectif", entete.EffectifTexte);
        }

        private static void SetText(Report report, string name, string value)
        {
            if (report.FindObject(name) is TextObject text)
                text.Text = value;
        }

        private static bool LooksLikeBulletinTemplate(Report report)
        {
            return report.FindObject("txtEcole") != null
                && (report.FindObject("LignesBand") != null || report.FindObject("Data1") != null);
        }

        private string GetTemplatePath() =>
            Path.Combine(_environment.ContentRootPath, "Reports", "Templates", "BulletinEleve.frx");

        private static BulletinEnteteReportDto MapEntete(BulletinEleveDto b)
        {
            return new BulletinEnteteReportDto
            {
                NomEcole = b.NomEcole,
                NomEleve = b.NomCompletEleve,
                NomClasse = b.NomClasse,
                LibelleAnnee = b.LibelleAnneeScolaire,
                Periode = b.Periode,
                MoyenneGeneraleTexte = FormatNote(b.MoyenneGenerale),
                RangTexte = b.Rang.HasValue ? $"{b.Rang.Value} / {b.EffectifClasse}" : "-",
                EffectifTexte = b.EffectifClasse > 0 ? b.EffectifClasse.ToString(CultureInfo.InvariantCulture) : "-",
                DecisionTexte = string.IsNullOrWhiteSpace(b.Decision) ? "-" : b.Decision,
                AppreciationTexte = string.IsNullOrWhiteSpace(b.AppreciationGenerale) ? "-" : b.AppreciationGenerale
            };
        }

        private static List<BulletinLigneReportDto> MapLignes(BulletinEleveDto b)
        {
            return b.Lignes.Select(l =>
            {
                var details = new StringBuilder();
                foreach (var note in l.Notes)
                {
                    if (details.Length > 0)
                        details.Append(" | ");
                    var titre = string.IsNullOrWhiteSpace(note.TitreEvaluation)
                        ? note.TypeEvaluation
                        : note.TitreEvaluation;
                    details.Append($"{titre}: {FormatNote(note.NoteObtenue)} (x{FormatCoeff(note.CoefficientEvaluation)})");
                }

                return new BulletinLigneReportDto
                {
                    NomCours = l.NomCours,
                    MoyenneCoursTexte = FormatNote(l.MoyenneCours),
                    CoefficientTexte = FormatCoeff(l.PonderationCours),
                    DetailNotes = details.Length > 0 ? details.ToString() : "-"
                };
            }).ToList();
        }

        private static string FormatNote(double? value) =>
            value.HasValue ? value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "-";

        private static string FormatCoeff(double value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture);

        private static async Task<byte[]> ExportToPdfAsync(Report report, CancellationToken cancellationToken)
        {
            report.Prepare();
            await using var stream = new MemoryStream();
            var export = new PDFSimpleExport();
            report.Export(export, stream);
            return stream.ToArray();
        }
    }
}
