using System.Globalization;
using FastReport;
using FastReport.Barcode;
using FastReport.Export.Html;
using FastReport.Export.PdfSimple;
using FastReport.Web;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using DataBand = FastReport.DataBand;
using PictureObject = FastReport.PictureObject;
using TextObject = FastReport.TextObject;

namespace KelasiNaBiso.Services.Reporting
{
    public interface ICarteReportService
    {
        Task<WebReport> BuildEleveReportAsync(int idEleve, CancellationToken cancellationToken = default);
        Task<WebReport> BuildAgentReportAsync(int idAgent, CancellationToken cancellationToken = default);
        Task<WebReport> BuildElevesBatchReportAsync(int idEcole, IEnumerable<int> idEleves, CancellationToken cancellationToken = default);
        Task<WebReport> BuildAgentsBatchReportAsync(int idEcole, IEnumerable<int> idAgents, CancellationToken cancellationToken = default);
        Task<WebReport> BuildElevesEcoleReportAsync(int idEcole, int? idClasse, CancellationToken cancellationToken = default);
        Task<byte[]> ExportToPdfAsync(Report report, CancellationToken cancellationToken = default);
        Task<string> ExportToHtmlAsync(Report report, CancellationToken cancellationToken = default);
        Task EnsureEcoleAccessAsync(int idEcole, int? userEcoleId, string? userRole, CancellationToken cancellationToken = default);
        Task<WebReport> BuildEleveReportForUserAsync(int idEleve, int? userEcoleId, string? userRole, CancellationToken cancellationToken = default);
        Task<WebReport> BuildAgentReportForUserAsync(int idAgent, int? userEcoleId, string? userRole, CancellationToken cancellationToken = default);
    }

    public class CarteReportService : ICarteReportService
    {
        private readonly IV_EleveRepository _vEleveRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly KelasiNaBisoDbContext _context;
        private readonly IReportImageResolver _imageResolver;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CarteReportService> _logger;

        public CarteReportService(
            IV_EleveRepository vEleveRepository,
            IAgentRepository agentRepository,
            KelasiNaBisoDbContext context,
            IReportImageResolver imageResolver,
            IWebHostEnvironment environment,
            ILogger<CarteReportService> logger)
        {
            _vEleveRepository = vEleveRepository;
            _agentRepository = agentRepository;
            _context = context;
            _imageResolver = imageResolver;
            _environment = environment;
            _logger = logger;
        }

        public Task EnsureEcoleAccessAsync(int idEcole, int? userEcoleId, string? userRole,
            CancellationToken cancellationToken = default)
        {
            if (UserRoles.CanAccessAllSchools(userRole))
                return Task.CompletedTask;

            if (!userEcoleId.HasValue || userEcoleId.Value != idEcole)
                throw new UnauthorizedAccessException("Acces refuse a cette ecole.");

            return Task.CompletedTask;
        }

        public async Task<WebReport> BuildEleveReportForUserAsync(int idEleve, int? userEcoleId, string? userRole,
            CancellationToken cancellationToken = default)
        {
            var eleve = await _vEleveRepository.GetByIdAsync(idEleve);
            if (eleve == null)
                throw new KeyNotFoundException($"Eleve {idEleve} introuvable.");
            if (eleve.IdEcole.HasValue)
                await EnsureEcoleAccessAsync(eleve.IdEcole.Value, userEcoleId, userRole, cancellationToken);
            var dto = await MapEleveRectoAsync(eleve, cancellationToken);
            return CreateEleveRectoWebReport(new List<CarteEleveRectoReportDto> { dto });
        }

        public async Task<WebReport> BuildAgentReportForUserAsync(int idAgent, int? userEcoleId, string? userRole,
            CancellationToken cancellationToken = default)
        {
            var agent = await _agentRepository.GetByIdAsync(idAgent);
            if (agent == null)
                throw new KeyNotFoundException($"Agent {idAgent} introuvable.");
            if (agent.IdEcole.HasValue)
                await EnsureEcoleAccessAsync(agent.IdEcole.Value, userEcoleId, userRole, cancellationToken);
            var dto = await MapAgentRectoAsync(agent, cancellationToken);
            return CreateAgentRectoWebReport(new List<CarteAgentRectoReportDto> { dto });
        }

        public async Task<WebReport> BuildEleveReportAsync(int idEleve, CancellationToken cancellationToken = default)
        {
            var eleve = await _vEleveRepository.GetByIdAsync(idEleve);
            if (eleve == null)
                throw new KeyNotFoundException($"Eleve {idEleve} introuvable.");

            var dto = await MapEleveRectoAsync(eleve, cancellationToken);
            return CreateEleveRectoWebReport(new List<CarteEleveRectoReportDto> { dto });
        }

        public async Task<WebReport> BuildAgentReportAsync(int idAgent, CancellationToken cancellationToken = default)
        {
            var agent = await _agentRepository.GetByIdAsync(idAgent);
            if (agent == null)
                throw new KeyNotFoundException($"Agent {idAgent} introuvable.");

            var dto = await MapAgentRectoAsync(agent, cancellationToken);
            return CreateAgentRectoWebReport(new List<CarteAgentRectoReportDto> { dto });
        }

        public async Task<WebReport> BuildElevesBatchReportAsync(int idEcole, IEnumerable<int> idEleves,
            CancellationToken cancellationToken = default)
        {
            var ids = idEleves.Distinct().ToList();
            var cartes = new List<CarteEleveRectoReportDto>();

            foreach (var id in ids)
            {
                var eleve = await _vEleveRepository.GetByIdAsync(id);
                if (eleve == null || eleve.IdEcole != idEcole)
                    continue;
                cartes.Add(await MapEleveRectoAsync(eleve, cancellationToken));
            }

            if (cartes.Count == 0)
                throw new KeyNotFoundException("Aucun eleve valide pour cette ecole.");

            return CreateEleveRectoWebReport(cartes);
        }

        public async Task<WebReport> BuildAgentsBatchReportAsync(int idEcole, IEnumerable<int> idAgents,
            CancellationToken cancellationToken = default)
        {
            var ids = idAgents.Distinct().ToList();
            var cartes = new List<CarteAgentRectoReportDto>();

            foreach (var id in ids)
            {
                var agent = await _agentRepository.GetByIdAsync(id);
                if (agent == null || agent.IdEcole != idEcole)
                    continue;
                cartes.Add(await MapAgentRectoAsync(agent, cancellationToken));
            }

            if (cartes.Count == 0)
                throw new KeyNotFoundException("Aucun agent valide pour cette ecole.");

            return CreateAgentRectoWebReport(cartes);
        }

        public async Task<WebReport> BuildElevesEcoleReportAsync(int idEcole, int? idClasse,
            CancellationToken cancellationToken = default)
        {
            var eleves = (await _vEleveRepository.GetByEcoleAsync(idEcole)).ToList();
            if (idClasse.HasValue)
                eleves = eleves.Where(e => e.IdClasse == idClasse.Value).ToList();

            if (eleves.Count == 0)
                throw new KeyNotFoundException("Aucun eleve trouve pour cette ecole.");

            var cartes = new List<CarteEleveRectoReportDto>();
            foreach (var eleve in eleves)
                cartes.Add(await MapEleveRectoAsync(eleve, cancellationToken));

            return CreateEleveRectoWebReport(cartes);
        }

        public async Task<byte[]> ExportToPdfAsync(Report report, CancellationToken cancellationToken = default)
        {
            EnsureDuplexCardReady(report);
            report.Prepare();

            var preparedCount = report.PreparedPages?.Count ?? 0;
            _logger.LogInformation(
                "Export PDF carte : {PageCount} page(s) préparée(s) (template {TemplatePages} page(s))",
                preparedCount,
                report.Pages.Count);
            if (preparedCount < 2)
            {
                _logger.LogWarning(
                    "PDF carte avec seulement {PageCount} page(s) — attendu recto+verso (2 pages par personne). " +
                    "Vérifiez RectoAgent.frx / RectoEleve.frx (2 ReportPage + Data1/Data2) et redémarrez l'API.",
                    preparedCount);
            }

            await using var stream = new MemoryStream();
            var export = new PDFSimpleExport();
            report.Export(export, stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Force toutes les pages (recto + verso) visibles et reliées aux sources de données.
        /// Data2 utilise <c>CarteVerso</c> (copie de <c>Carte</c>) pour ne pas être à EOF après Data1.
        /// </summary>
        private static void EnsureDuplexCardReady(Report report)
        {
            var carteDs = report.GetDataSource("Carte");
            var versoDs = report.GetDataSource("CarteVerso") ?? carteDs;

            foreach (Base pageBase in report.Pages)
            {
                if (pageBase is not ReportPage page)
                    continue;

                page.Visible = true;

                foreach (Base bandBase in page.Bands)
                {
                    if (bandBase is not DataBand dataBand)
                        continue;

                    dataBand.Visible = true;
                    dataBand.PrintIfDatasourceEmpty = true;

                    // Data2 = verso : source dédiée (évite curseur déjà consommé par Data1)
                    if (string.Equals(dataBand.Name, "Data2", StringComparison.OrdinalIgnoreCase))
                    {
                        dataBand.DataSource = versoDs;
                        // Déjà sur une 2ᵉ ReportPage — StartNewPage ferait sauter le verso
                        dataBand.StartNewPage = false;
                    }
                    else if (dataBand.DataSource == null && carteDs != null)
                    {
                        dataBand.DataSource = carteDs;
                    }
                }
            }
        }

        /// <summary>Enregistre la même liste sous Carte (recto) et CarteVerso (verso).</summary>
        private static void RegisterCarteDuplexData<T>(Report report, List<T> data)
        {
            report.RegisterData(data, "Carte");
            report.RegisterData(data, "CarteVerso");

            var carteDs = report.GetDataSource("Carte");
            if (carteDs != null)
                carteDs.Enabled = true;

            var versoDs = report.GetDataSource("CarteVerso");
            if (versoDs != null)
                versoDs.Enabled = true;
        }

        private static void BindDuplexDataBands(Report report)
        {
            var carteDs = report.GetDataSource("Carte");
            var versoDs = report.GetDataSource("CarteVerso") ?? carteDs;

            if (report.FindObject("Data1") is DataBand dataBandRecto)
            {
                dataBandRecto.DataSource = carteDs;
                dataBandRecto.Visible = true;
                dataBandRecto.StartNewPage = false;
            }

            if (report.FindObject("Data2") is DataBand dataBandVerso)
            {
                dataBandVerso.DataSource = versoDs;
                dataBandVerso.Visible = true;
                dataBandVerso.PrintIfDatasourceEmpty = true;
                dataBandVerso.StartNewPage = false;
            }
        }

        public async Task<string> ExportToHtmlAsync(Report report, CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureDuplexCardReady(report);
                report.Prepare();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Echec Prepare() du rapport carte avant export HTML.");
                throw new InvalidOperationException(
                    "Impossible de preparer le rapport carte. Verifiez le template FastReport (.frx) et l'image de fond.",
                    ex);
            }

            await using var stream = new MemoryStream();
            var export = new HTMLExport
            {
                SinglePage = true,
                Navigator = false,
                EmbedPictures = true
            };
            report.Export(export, stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(html))
            {
                throw new InvalidOperationException(
                    "L'export HTML de la carte est vide. Verifiez RectoAgent.frx / RectoEleve.frx " +
                    "(image de fond, datasource Carte) puis redemarrez l'API apres deploiement.");
            }

            return html;
        }

        private async Task<CarteEleveRectoReportDto> MapEleveRectoAsync(V_Eleve eleve, CancellationToken cancellationToken)
        {
            var annee = await GetAnneeScolaireLabelAsync(eleve.IdEcole, cancellationToken);
            return new CarteEleveRectoReportDto
            {
                Ecole = ToCardUpper(eleve.NomEcole),
                NomComplet = ToCardUpper(eleve.NomComplet ?? $"{eleve.Nom} {eleve.Postnom} {eleve.Prenom}"),
                Matricule = ToCardUpper(eleve.Matricule ?? eleve.IdEleve.ToString()),
                Classe = ToCardUpper(eleve.NomClasse),
                Adresse = ToCardUpper(FormatAdresseEleve(eleve)),
                AnneeScolaire = ToCardUpper(annee),
                Photo = await _imageResolver.ResolveAsync(eleve.PhotoUrl, cancellationToken),
                Logo = await _imageResolver.ResolveAsync(eleve.LogoUrlEcole, cancellationToken)
            };
        }

        private static string FormatAdresseEleve(V_Eleve eleve)
        {
            var parts = new[]
            {
                eleve.Avenue,
                eleve.Numero,
                eleve.Quartier,
                eleve.Commune,
                eleve.Ville,
                eleve.Province
            };

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private async Task<CarteEleveReportDto> MapEleveAsync(V_Eleve eleve, CancellationToken cancellationToken)
        {
            var annee = await GetAnneeScolaireLabelAsync(eleve.IdEcole, cancellationToken);
            return new CarteEleveReportDto
            {
                IdEleve = eleve.IdEleve,
                IdEcole = eleve.IdEcole,
                NomComplet = ToCardUpper(eleve.NomComplet ?? $"{eleve.Nom} {eleve.Postnom} {eleve.Prenom}".Trim()),
                Matricule = ToCardUpper(eleve.Matricule ?? eleve.IdEleve.ToString()),
                Genre = ToCardUpper(eleve.Genre ?? string.Empty),
                DateNaissance = eleve.DateNaissance,
                NomClasse = ToCardUpper(eleve.NomClasse ?? string.Empty),
                NomEcole = ToCardUpper(eleve.NomEcole ?? string.Empty),
                SloganEcole = ToCardUpper(eleve.SloganEcole ?? string.Empty),
                AnneeScolaire = ToCardUpper(annee),
                PhotoBytes = await _imageResolver.ResolveAsync(eleve.PhotoUrl, cancellationToken),
                LogoBytes = await _imageResolver.ResolveAsync(eleve.LogoUrlEcole, cancellationToken)
            };
        }

        private async Task<CarteAgentRectoReportDto> MapAgentRectoAsync(Agent agent, CancellationToken cancellationToken)
        {
            var ecole = agent.IdEcole.HasValue
                ? await _context.Ecoles.AsNoTracking().FirstOrDefaultAsync(e => e.IdEcole == agent.IdEcole.Value, cancellationToken)
                : null;

            var nomComplet = $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim();

            return new CarteAgentRectoReportDto
            {
                Noms = ToCardUpper(nomComplet),
                Matricule = ToCardUpper(agent.Matricule ?? agent.IdAgent.ToString()),
                Fonction = ToCardUpper(agent.Fonction ?? agent.RoleAgent ?? string.Empty),
                Adresse = ToCardUpper(FormatAdresse(agent)),
                Ecole = ToCardUpper(ecole?.Nom ?? string.Empty),
                Photo = await _imageResolver.ResolveAsync(agent.PhotoUrl, cancellationToken),
                Logo = await _imageResolver.ResolveAsync(ecole?.Logo, cancellationToken)
            };
        }

        private static string FormatAdresse(Agent agent)
        {
            var parts = new[]
            {
                agent.Avenue,
                agent.Numero,
                agent.Quartier,
                agent.Commune,
                agent.Ville,
                agent.Province
            };

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        /// <summary>Met en majuscules les textes de la carte (accents FR).</summary>
        private static string ToCardUpper(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpper(CultureInfo.GetCultureInfo("fr-FR"));

        private async Task<CarteAgentReportDto> MapAgentAsync(Agent agent, CancellationToken cancellationToken)
        {
            var ecole = agent.IdEcole.HasValue
                ? await _context.Ecoles.AsNoTracking().FirstOrDefaultAsync(e => e.IdEcole == agent.IdEcole.Value, cancellationToken)
                : null;

            var annee = await GetAnneeScolaireLabelAsync(agent.IdEcole, cancellationToken);
            var nomComplet = $"{agent.Nom} {agent.Postnom} {agent.Prenom}".Trim();

            return new CarteAgentReportDto
            {
                IdAgent = agent.IdAgent,
                IdEcole = agent.IdEcole,
                NomComplet = ToCardUpper(nomComplet),
                Matricule = ToCardUpper(agent.Matricule ?? agent.IdAgent.ToString()),
                Fonction = ToCardUpper(agent.Fonction ?? string.Empty),
                RoleAgent = ToCardUpper(agent.RoleAgent ?? string.Empty),
                NomEcole = ToCardUpper(ecole?.Nom ?? string.Empty),
                SloganEcole = ToCardUpper(ecole?.Slogan ?? string.Empty),
                AnneeScolaire = ToCardUpper(annee),
                PhotoBytes = await _imageResolver.ResolveAsync(agent.PhotoUrl, cancellationToken),
                LogoBytes = await _imageResolver.ResolveAsync(ecole?.Logo, cancellationToken)
            };
        }

        private async Task<string> GetAnneeScolaireLabelAsync(int? idEcole, CancellationToken cancellationToken)
        {
            var query = _context.AnneeScolaires.AsNoTracking().Where(a => a.Statut == true);
            if (idEcole.HasValue)
                query = query.Where(a => a.IdEcole == idEcole || a.IdEcole == null);

            var annee = await query
                .OrderByDescending(a => a.DateDebut)
                .FirstOrDefaultAsync(cancellationToken);

            return annee?.LibelleAnneeScolaire ?? DateTime.UtcNow.Year.ToString();
        }

        private WebReport CreateAgentRectoWebReport(List<CarteAgentRectoReportDto> data)
        {
            var report = new Report();
            var templatePath = GetAgentRectoTemplatePath();

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (!LooksLikeAgentRectoTemplate(report))
                    {
                        _logger.LogWarning(
                            "Template {Template} charge mais structure inattendue (Picture1/Noms absents). Fallback.",
                            templatePath);
                        return CreateAgentFallbackWebReport(data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Template {Template} invalide, mise en page programmatique.", templatePath);
                    return CreateAgentFallbackWebReport(data);
                }
            }
            else
            {
                _logger.LogWarning("Template agent introuvable: {Template}", templatePath);
                return CreateAgentFallbackWebReport(data);
            }

            RegisterCarteDuplexData(report, data);
            ApplyRectoAgentBindings(report);
            EnsureRectoAgentBackground(report);
            EnsureDuplexCardReady(report);

            return new WebReport
            {
                Report = report,
                Width = "100%"
            };
        }

        private WebReport CreateEleveRectoWebReport(List<CarteEleveRectoReportDto> data)
        {
            var report = new Report();
            var templatePath = GetEleveRectoTemplatePath();

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (!LooksLikeEleveRectoTemplate(report))
                    {
                        _logger.LogWarning(
                            "Template {Template} invalide pour recto-verso " +
                            "(besoin pages>=2, Data1/Data2, nomcomplet, photo). Fallback 1 page. " +
                            "Taille fichier={Size} octets.",
                            templatePath,
                            new FileInfo(templatePath).Length);
                        return CreateEleveFallbackWebReport(data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Template {Template} invalide, mise en page programmatique.", templatePath);
                    return CreateEleveFallbackWebReport(data);
                }
            }
            else
            {
                _logger.LogWarning("Template eleve introuvable: {Template}", templatePath);
                return CreateEleveFallbackWebReport(data);
            }

            RegisterCarteDuplexData(report, data);
            ApplyRectoEleveBindings(report);
            EnsureDuplexCardReady(report);

            _logger.LogInformation(
                "Carte eleve FastReport: template={Template}, pages={Pages}, eleves={Count}",
                templatePath,
                report.Pages.Count,
                data.Count);

            return new WebReport
            {
                Report = report,
                Width = "100%"
            };
        }

        private WebReport CreateEleveFallbackWebReport(List<CarteEleveRectoReportDto> data)
        {
            var report = new Report();
            ApplyFallbackLayout(report, CarteTemplateType.Eleve);
            var legacy = data.Select(d => new CarteEleveReportDto
            {
                NomComplet = d.NomComplet,
                Matricule = d.Matricule,
                NomClasse = d.Classe,
                NomEcole = d.Ecole,
                AnneeScolaire = d.AnneeScolaire,
                PhotoBytes = d.Photo,
                LogoBytes = d.Logo
            }).ToList();

            RegisterCarteDuplexData(report, legacy);
            if (report.FindObject("CarteBand") is DataBand rectoBand)
                rectoBand.DataSource = report.GetDataSource("Carte");
            BindDuplexDataBands(report);
            EnsureDuplexCardReady(report);

            return new WebReport
            {
                Report = report,
                Width = "100%"
            };
        }

        private static void ApplyRectoEleveBindings(Report report)
        {
            BindDuplexDataBands(report);

            // Noms d'objets = ceux du designer RectoEleve.frx (minuscules)
            SetTextBinding(report, "ecole", "[Carte.Ecole]");
            SetTextBinding(report, "nomcomplet", "[Carte.NomComplet]");
            SetTextBinding(report, "matricule", "MATRICULE : [Carte.Matricule]");
            SetTextBinding(report, "classe", "CLASSE : [Carte.Classe]");
            SetTextBinding(report, "adresse", "ADRESSE : [Carte.Adresse]");
            SetTextBinding(report, "anneescolaire", "[Carte.AnneeScolaire]");

            if (report.FindObject("photo") is PictureObject photo)
                photo.DataColumn = "Carte.Photo";

            if (report.FindObject("logo") is PictureObject logo)
                logo.DataColumn = "Carte.Logo";

            // Verso — logo école (même dictionnaire que le recto ; pas "Logo" nu → CS0103 au Prepare)
            if (report.FindObject("logo1") is PictureObject logoVerso)
                logoVerso.DataColumn = "Carte.Logo";
        }

        private WebReport CreateAgentFallbackWebReport(List<CarteAgentRectoReportDto> data)
        {
            var report = new Report();
            ApplyFallbackLayout(report, CarteTemplateType.Agent);
            var legacy = MapRectoToLegacyAgentDtos(data);
            RegisterCarteDuplexData(report, legacy);
            if (report.FindObject("CarteBand") is DataBand rectoBand)
                rectoBand.DataSource = report.GetDataSource("Carte");
            BindDuplexDataBands(report);
            EnsureDuplexCardReady(report);

            return new WebReport
            {
                Report = report,
                Width = "100%"
            };
        }

        private static List<CarteAgentReportDto> MapRectoToLegacyAgentDtos(List<CarteAgentRectoReportDto> data) =>
            data.Select(d => new CarteAgentReportDto
            {
                NomComplet = d.Noms,
                Matricule = d.Matricule,
                Fonction = d.Fonction,
                NomEcole = d.Ecole,
                PhotoBytes = d.Photo,
                LogoBytes = d.Logo
            }).ToList();

        private static void ApplyRectoAgentBindings(Report report)
        {
            BindDuplexDataBands(report);

            SetTextBinding(report, "Noms", "[Carte.Noms]");
            BindFonctionLabelAndValue(report);
            SetTextBinding(report, "Matricule", "MATRICULE : [Carte.Matricule]");
            SetTextBinding(report, "Adresse", "ADRESSE : [Carte.Adresse]");
            SetTextBinding(report, "Ecole", "[Carte.Ecole]");

            if (report.FindObject("Photo") is PictureObject photo)
                photo.DataColumn = "Carte.Photo";

            // Logo école — recto (header)
            if (report.FindObject("Logo") is PictureObject logo)
                logo.DataColumn = "Carte.Logo";

            // Logo école — verso (Carte.Logo ; "Logo" seul provoque CompilerException CS0103)
            if (report.FindObject("logo1") is PictureObject logoVerso)
                logoVerso.DataColumn = "Carte.Logo";

            // Code-barres matricule : taille fixe dans la carte (évite AutoSize qui fait déborder)
            if (report.FindObject("Barcode") is BarcodeObject barcode)
            {
                const float cardWidth = 325.08f;
                const float rightMargin = 8f;
                const float barcodeWidth = 105f;
                const float barcodeHeight = 22f;

                barcode.Barcode = new Barcode128();
                barcode.Text = "[Carte.Matricule]";
                // Matricule déjà affiché en texte → pas de ShowText (évite débordement)
                barcode.ShowText = false;
                barcode.AutoSize = false;
                barcode.Width = barcodeWidth;
                barcode.Height = barcodeHeight;
                barcode.Left = cardWidth - barcodeWidth - rightMargin;
                barcode.Top = 172f;
                barcode.CanGrow = false;
                barcode.CanShrink = false;
            }

            // Polices designer (ex. Arial Rounded) souvent absentes sur le serveur → substitution Arial
            foreach (Base obj in GetAllReportObjects(report))
            {
                if (obj is TextObject text && text.Font != null)
                {
                    var family = text.Font.FontFamily?.Name ?? text.Font.Name;
                    if (!string.IsNullOrEmpty(family) &&
                        family.Contains("Rounded", StringComparison.OrdinalIgnoreCase))
                    {
                        text.Font = new System.Drawing.Font(
                            "Arial",
                            text.Font.Size,
                            text.Font.Style);
                    }
                }
            }
        }

        private void EnsureRectoAgentBackground(Report report)
        {
            if (report.FindObject("Picture1") is not PictureObject background)
                return;

            if (background.Image != null)
                return;

            var bgPath = Path.Combine(
                _environment.ContentRootPath, "Reports", "Templates", "recto-agent-bg.png");
            if (!File.Exists(bgPath))
            {
                _logger.LogWarning("Fond carte agent introuvable: {Path}", bgPath);
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(bgPath);
                using var temp = Image.FromStream(new MemoryStream(bytes));
                background.Image = new Bitmap(temp);
                _logger.LogInformation("Fond Picture1 recharge depuis {Path}", bgPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de charger le fond {Path}", bgPath);
            }
        }

        private static bool LooksLikeEleveRectoTemplate(Report report) =>
            report.Pages.Count >= 2
            && report.FindObject("Data1") is DataBand
            && report.FindObject("Data2") is DataBand
            && report.FindObject("nomcomplet") is TextObject
            && report.FindObject("photo") is PictureObject
            && report.FindObject("logo1") is PictureObject;

        private static bool LooksLikeAgentRectoTemplate(Report report) =>
            report.Pages.Count >= 2
            && report.FindObject("Data1") is DataBand
            && report.FindObject("Data2") is DataBand
            && report.FindObject("Noms") is TextObject
            && report.FindObject("Photo") is PictureObject;

        private static bool LooksLikeDemoEleveTemplate(Report report)
        {
            if (LooksLikeEleveRectoTemplate(report))
                return false;

            // CarteEleve.frx etait encore le demo NorthWind Employees
            if (report.FindObject("Data1") is DataBand band
                && band.DataSource != null
                && string.Equals(band.DataSource.Name, "Employees", StringComparison.OrdinalIgnoreCase))
                return true;

            var hasCarteFields =
                report.FindObject("NomComplet") is TextObject
                || report.FindObject("Noms") is TextObject
                || report.FindObject("nomcomplet") is TextObject;
            return !hasCarteFields;
        }

        private static IEnumerable<Base> GetAllReportObjects(Report report)
        {
            foreach (Base pageBase in report.Pages)
            {
                if (pageBase is not ReportPage page)
                    continue;
                foreach (Base bandBase in page.Bands)
                {
                    yield return bandBase;
                    if (bandBase is BandBase band)
                    {
                        foreach (Base child in band.Objects)
                            yield return child;
                    }
                }
            }
        }

        private static void SetTextBinding(Report report, string objectName, string expression)
        {
            if (report.FindObject(objectName) is TextObject text)
                text.Text = expression;
        }

        /// <summary>
        /// Label « FONCTION : » en taille fixe ; la valeur seule s'adapte (AutoShrink) sur une ligne.
        /// </summary>
        private static void BindFonctionLabelAndValue(Report report)
        {
            if (report.FindObject("Fonction") is not TextObject valeur)
                return;

            if (valeur.Parent is not BandBase band)
                return;

            const string labelText = "FONCTION :";
            const float labelWidth = 72f;
            const float gap = 2f;

            var left = valeur.Left;
            var top = valeur.Top;
            var height = valeur.Height;
            var totalWidth = valeur.Width;
            var baseFont = valeur.Font;

            var label = report.FindObject("FonctionLabel") as TextObject;
            if (label == null)
            {
                label = new TextObject { Name = "FonctionLabel" };
                band.Objects.Add(label);
            }

            label.Left = left;
            label.Top = top;
            label.Width = labelWidth;
            label.Height = height;
            label.Text = labelText;
            label.WordWrap = false;
            label.CanGrow = false;
            label.CanShrink = false;
            label.AutoShrink = AutoShrinkMode.None;
            label.VertAlign = VertAlign.Center;
            label.HorzAlign = HorzAlign.Left;
            if (baseFont != null)
                label.Font = (System.Drawing.Font)baseFont.Clone();

            valeur.Left = left + labelWidth + gap;
            valeur.Top = top;
            valeur.Width = Math.Max(24f, totalWidth - labelWidth - gap);
            valeur.Height = height;
            valeur.Text = "[Carte.Fonction]";
            valeur.WordWrap = false;
            valeur.CanGrow = false;
            valeur.CanShrink = false;
            valeur.AutoShrink = AutoShrinkMode.FontSize;
            valeur.AutoShrinkMinSize = 4.5f;
            valeur.VertAlign = VertAlign.Center;
            valeur.HorzAlign = HorzAlign.Left;
        }

        private WebReport CreateWebReport<T>(List<T> data, CarteTemplateType templateType)
        {
            var report = new Report();
            var templatePath = GetTemplatePath(templateType);
            var useFallback = true;

            if (File.Exists(templatePath))
            {
                try
                {
                    report.Load(templatePath);
                    if (templateType == CarteTemplateType.Eleve && LooksLikeDemoEleveTemplate(report))
                    {
                        _logger.LogWarning(
                            "Template eleve {Template} est un modele demo (NorthWind) ou sans champs carte. Fallback layout.",
                            templatePath);
                    }
                    else
                    {
                        useFallback = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Template {Template} invalide, mise en page programmatique.", templatePath);
                }
            }

            if (useFallback)
            {
                report = new Report();
                ApplyFallbackLayout(report, templateType);
            }

            report.RegisterData(data, "Carte");
            var ds = report.GetDataSource("Carte");
            if (ds != null)
                ds.Enabled = true;

            if (!useFallback && templateType == CarteTemplateType.Eleve)
                ApplyEleveTemplateBindings(report);

            return new WebReport
            {
                Report = report,
                Width = "100%"
            };
        }

        private static void ApplyEleveTemplateBindings(Report report)
        {
            if (report.FindObject("Data1") is DataBand dataBand)
                dataBand.DataSource = report.GetDataSource("Carte");

            SetTextBinding(report, "NomComplet", "[Carte.NomComplet]");
            SetTextBinding(report, "Matricule", "[Carte.Matricule]");
            SetTextBinding(report, "NomClasse", "[Carte.NomClasse]");
            SetTextBinding(report, "NomEcole", "[Carte.NomEcole]");
            SetTextBinding(report, "SloganEcole", "[Carte.SloganEcole]");
            SetTextBinding(report, "AnneeScolaire", "[Carte.AnneeScolaire]");

            if (report.FindObject("Photo") is PictureObject photo)
                photo.DataColumn = "Carte.PhotoBytes";
            if (report.FindObject("Logo") is PictureObject logo)
                logo.DataColumn = "Carte.LogoBytes";
            if (report.FindObject("PhotoBytes") is PictureObject photoBytes)
                photoBytes.DataColumn = "Carte.PhotoBytes";
            if (report.FindObject("LogoBytes") is PictureObject logoBytes)
                logoBytes.DataColumn = "Carte.LogoBytes";
        }

        private static void ApplyFallbackLayout(Report report, CarteTemplateType templateType)
        {
            if (templateType == CarteTemplateType.Agent)
                CarteReportLayoutBuilder.ApplyAgentLayout(report);
            else
                CarteReportLayoutBuilder.ApplyEleveLayout(report);
        }

        private string GetAgentRectoTemplatePath() =>
            Path.Combine(_environment.ContentRootPath, "Reports", "Templates", "RectoAgent.frx");

        private string GetEleveRectoTemplatePath() =>
            Path.Combine(_environment.ContentRootPath, "Reports", "Templates", "RectoEleve.frx");

        private string GetTemplatePath(CarteTemplateType templateType)
        {
            var fileName = templateType == CarteTemplateType.Agent
                ? "CarteAgent.frx"
                : "RectoEleve.frx";
            return Path.Combine(_environment.ContentRootPath, "Reports", "Templates", fileName);
        }

        private enum CarteTemplateType
        {
            Eleve,
            Agent
        }
    }
}
