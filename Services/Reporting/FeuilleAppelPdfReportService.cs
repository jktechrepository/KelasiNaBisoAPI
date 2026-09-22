using System.Globalization;
using System.Text;
using FastReport;
using FastReport.Export.PdfSimple;
using KelasiNaBiso.Models.DTOs.Reporting;
using DataBand = FastReport.DataBand;
using TextObject = FastReport.TextObject;

namespace KelasiNaBiso.Services.Reporting
{
    public class FeuilleAppelPdfReportService : IFeuilleAppelPdfReportService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FeuilleAppelPdfReportService> _logger;

        public string ContentType => "application/pdf";

        public FeuilleAppelPdfReportService(
            IWebHostEnvironment environment,
            ILogger<FeuilleAppelPdfReportService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public byte[] ExportEleves(FeuilleAppelClasseDto feuille)
        {
            var entete = MapEnteteEleves(feuille);
            var lignes = MapLignesEleves(feuille);
            var report = BuildElevesReport(entete, lignes);
            return ExportToPdf(report);
        }

        public byte[] ExportAgents(FeuilleAppelAgentsDto feuille)
        {
            var entete = MapEnteteAgents(feuille);
            var lignes = MapLignesAgents(feuille);
            var report = BuildAgentsReport(entete, lignes);
            return ExportToPdf(report);
        }

        public string GetFileNameEleves(FeuilleAppelClasseDto feuille)
        {
            var classe = SanitizeFilePart(feuille.NomClasse);
            if (string.IsNullOrWhiteSpace(classe))
                classe = $"Classe{feuille.IdClasse}";

            return $"FeuilleAppel_{classe}_{feuille.Date:yyyyMMdd}.pdf";
        }

        public string GetFileNameAgents(FeuilleAppelAgentsDto feuille)
        {
            var ecole = SanitizeFilePart(feuille.NomEcole);
            if (string.IsNullOrWhiteSpace(ecole))
                ecole = $"Ecole{feuille.IdEcole}";

            var suffix = string.IsNullOrWhiteSpace(feuille.FonctionFiltre)
                ? string.Empty
                : $"_{SanitizeFilePart(feuille.FonctionFiltre)}";

            return $"FeuilleAppel_Agents_{ecole}{suffix}_{feuille.Date:yyyyMMdd}.pdf";
        }

        private Report BuildElevesReport(
            FeuilleAppelEnteteReportDto entete,
            List<FeuilleAppelLigneReportDto> lignes)
        {
            var report = new Report();
            var templatePath = GetTemplatePath("FeuilleAppelEleves.frx");

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (!LooksLikeFeuilleTemplate(report))
                    {
                        _logger.LogWarning("Template {Path} invalide, fallback programmatique.", templatePath);
                        report = new Report();
                        FeuilleAppelElevesReportLayoutBuilder.ApplyLayout(report, entete, lignes);
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
                    FeuilleAppelElevesReportLayoutBuilder.ApplyLayout(report, entete, lignes);
                }
            }
            else
            {
                _logger.LogInformation("Template feuille élèves absent ({Path}), mise en page programmatique.", templatePath);
                FeuilleAppelElevesReportLayoutBuilder.ApplyLayout(report, entete, lignes);
            }

            return report;
        }

        private Report BuildAgentsReport(
            FeuilleAppelEnteteReportDto entete,
            List<FeuilleAppelLigneReportDto> lignes)
        {
            var report = new Report();
            var templatePath = GetTemplatePath("FeuilleAppelAgents.frx");

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (!LooksLikeFeuilleTemplate(report))
                    {
                        _logger.LogWarning("Template {Path} invalide, fallback programmatique.", templatePath);
                        report = new Report();
                        FeuilleAppelAgentsReportLayoutBuilder.ApplyLayout(report, entete, lignes);
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
                    FeuilleAppelAgentsReportLayoutBuilder.ApplyLayout(report, entete, lignes);
                }
            }
            else
            {
                _logger.LogInformation("Template feuille agents absent ({Path}), mise en page programmatique.", templatePath);
                FeuilleAppelAgentsReportLayoutBuilder.ApplyLayout(report, entete, lignes);
            }

            return report;
        }

        private static void RegisterLignesData(Report report, List<FeuilleAppelLigneReportDto> lignes)
        {
            report.RegisterData(new List<FeuilleAppelLigneReportDto>(lignes), "FeuilleAppelLignes");
            var ds = report.GetDataSource("FeuilleAppelLignes");
            if (ds != null)
                ds.Enabled = true;
        }

        private static void BindLignesBand(Report report)
        {
            var ds = report.GetDataSource("FeuilleAppelLignes");
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

        private static void ApplyHeaderToTemplate(Report report, FeuilleAppelEnteteReportDto entete)
        {
            SetText(report, "txtEcole", entete.NomEcole);
            SetText(report, "txtTitre", entete.Titre);
            SetText(report, "txtContexte", entete.LigneContexte);
            SetText(report, "txtDate", entete.DateTexte);
            SetText(report, "txtEffectif", entete.EffectifTexte);
            SetText(report, "txtPresents", entete.NbPresentsTexte);
            SetText(report, "txtAbsents", entete.NbAbsentsTexte);
            SetText(report, "txtRetards", entete.NbRetardsTexte);
        }

        private static void SetText(Report report, string name, string value)
        {
            if (report.FindObject(name) is TextObject text)
                text.Text = value;
        }

        private static bool LooksLikeFeuilleTemplate(Report report) =>
            report.FindObject("txtEcole") != null
            && (report.FindObject("LignesBand") != null || report.FindObject("Data1") != null);

        private string GetTemplatePath(string fileName) =>
            Path.Combine(_environment.ContentRootPath, "Reports", "Templates", fileName);

        public static FeuilleAppelEnteteReportDto MapEnteteEleves(FeuilleAppelClasseDto feuille)
        {
            var ecole = string.IsNullOrWhiteSpace(feuille.NomEcole)
                ? $"École #{feuille.IdEcole}"
                : feuille.NomEcole!;

            return new FeuilleAppelEnteteReportDto
            {
                Titre = "Feuille d'appel",
                NomEcole = ecole,
                LigneContexte = $"Classe : {feuille.NomClasse}  |  Année scolaire (Id) : {feuille.IdAnneeScolaire}",
                DateTexte = feuille.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EffectifTexte = feuille.Effectif.ToString(CultureInfo.InvariantCulture),
                NbPresentsTexte = feuille.NbPresents.ToString(CultureInfo.InvariantCulture),
                NbAbsentsTexte = feuille.NbAbsents.ToString(CultureInfo.InvariantCulture),
                NbRetardsTexte = feuille.NbRetards.ToString(CultureInfo.InvariantCulture)
            };
        }

        public static FeuilleAppelEnteteReportDto MapEnteteAgents(FeuilleAppelAgentsDto feuille)
        {
            var ecole = string.IsNullOrWhiteSpace(feuille.NomEcole)
                ? $"École #{feuille.IdEcole}"
                : feuille.NomEcole!;
            var fonction = string.IsNullOrWhiteSpace(feuille.FonctionFiltre)
                ? "(toutes)"
                : feuille.FonctionFiltre!;

            return new FeuilleAppelEnteteReportDto
            {
                Titre = "Feuille d'appel agents",
                NomEcole = ecole,
                LigneContexte = $"Fonction (filtre) : {fonction}",
                DateTexte = feuille.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EffectifTexte = feuille.Effectif.ToString(CultureInfo.InvariantCulture),
                NbPresentsTexte = feuille.NbPresents.ToString(CultureInfo.InvariantCulture),
                NbAbsentsTexte = feuille.NbAbsents.ToString(CultureInfo.InvariantCulture),
                NbRetardsTexte = feuille.NbRetards.ToString(CultureInfo.InvariantCulture)
            };
        }

        public static List<FeuilleAppelLigneReportDto> MapLignesEleves(FeuilleAppelClasseDto feuille)
        {
            var index = 1;
            return feuille.Lignes.Select(l => new FeuilleAppelLigneReportDto
            {
                Index = (index++).ToString(CultureInfo.InvariantCulture),
                Matricule = l.Matricule ?? string.Empty,
                NomComplet = l.NomComplet ?? string.Empty,
                Genre = l.Genre ?? string.Empty,
                Fonction = string.Empty,
                StatutJour = l.StatutJour ?? string.Empty,
                HeureArrivee = FormatTime(l.HeureArrivee),
                HeureDepart = FormatTime(l.HeureDepart),
                Observation = l.Observation ?? string.Empty
            }).ToList();
        }

        public static List<FeuilleAppelLigneReportDto> MapLignesAgents(FeuilleAppelAgentsDto feuille)
        {
            var index = 1;
            return feuille.Lignes.Select(l => new FeuilleAppelLigneReportDto
            {
                Index = (index++).ToString(CultureInfo.InvariantCulture),
                Matricule = l.Matricule ?? string.Empty,
                NomComplet = l.NomComplet ?? string.Empty,
                Genre = l.Genre ?? string.Empty,
                Fonction = l.Fonction ?? string.Empty,
                StatutJour = l.StatutJour ?? string.Empty,
                HeureArrivee = FormatTime(l.HeureArrivee),
                HeureDepart = FormatTime(l.HeureDepart),
                Observation = l.Observation ?? string.Empty
            }).ToList();
        }

        private static string FormatTime(TimeSpan? time) =>
            time.HasValue ? time.Value.ToString(@"hh\:mm") : string.Empty;

        private static byte[] ExportToPdf(Report report)
        {
            report.Prepare();
            using var stream = new MemoryStream();
            var export = new PDFSimpleExport();
            report.Export(export, stream);
            return stream.ToArray();
        }

        private static string SanitizeFilePart(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(value.Length);
            foreach (var c in value.Trim())
            {
                if (invalid.Contains(c) || c == ' ')
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
