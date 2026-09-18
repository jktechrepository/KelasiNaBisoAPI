using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public interface IDashboardTuteurService
    {
        /// <summary>
        /// Dashboard parent : mono-école si <paramref name="idEcole"/> fourni, sinon agrégat multi-écoles.
        /// </summary>
        Task<DashboardTuteurDto> BuildAsync(
            int idTuteur,
            int? idEcole,
            string? libelleAnneeScolaire,
            CancellationToken cancellationToken = default);
    }

    public class DashboardTuteurService : IDashboardTuteurService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public DashboardTuteurService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<DashboardTuteurDto> BuildAsync(
            int idTuteur,
            int? idEcole,
            string? libelleAnneeScolaire,
            CancellationToken cancellationToken = default)
        {
            var targetDate = DateTime.Now.Date;
            var periode = BuildJourPeriode(targetDate);
            var now = DateTime.Now;

            List<Inscription> inscriptions;
            string? libelleAnnee;
            int? idAnneeMono = null;
            EcoleInfoDto? ecoleMono = null;

            if (idEcole.HasValue && idEcole.Value > 0)
            {
                var resolvedEcole = idEcole.Value;
                int idAnnee;

                if (string.IsNullOrWhiteSpace(libelleAnneeScolaire))
                {
                    idAnnee = await _scope.ResolveIdAnneeScolaireAsync(resolvedEcole, null);
                    libelleAnnee = await _context.AnneeScolaires.AsNoTracking()
                        .Where(a => a.IdAnneeScolaire == idAnnee)
                        .Select(a => a.LibelleAnneeScolaire)
                        .FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    var libelle = libelleAnneeScolaire.Trim().ToLower();
                    var annee = await _context.AnneeScolaires.AsNoTracking()
                        .Where(a => a.IdEcole == resolvedEcole
                            && a.Statut == true
                            && a.LibelleAnneeScolaire.ToLower() == libelle)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (annee == null)
                    {
                        throw new InvalidOperationException(
                            $"Aucune année scolaire active avec le libellé '{libelleAnneeScolaire.Trim()}' pour l'école {resolvedEcole}.");
                    }

                    idAnnee = annee.IdAnneeScolaire;
                    libelleAnnee = annee.LibelleAnneeScolaire;
                }

                var ecole = await _context.Ecoles.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEcole == resolvedEcole, cancellationToken)
                    ?? throw new InvalidOperationException($"École avec l'ID {resolvedEcole} introuvable.");

                ecoleMono = new EcoleInfoDto
                {
                    IdEcole = ecole.IdEcole,
                    NomEcole = ecole.Nom ?? string.Empty,
                    Logo = ecole.Logo
                };
                idAnneeMono = idAnnee;

                inscriptions = await BaseInscriptionsQuery(idTuteur)
                    .Include(i => i.Ecole)
                    .Include(i => i.AnneeScolaire)
                    .Where(i => i.IdEcole == resolvedEcole && i.IdAnneeScolaire == idAnnee)
                    .OrderByDescending(i => i.DateInscription)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                IQueryable<Inscription> query = BaseInscriptionsQuery(idTuteur)
                    .Include(i => i.Ecole)
                    .Include(i => i.AnneeScolaire);

                if (string.IsNullOrWhiteSpace(libelleAnneeScolaire))
                {
                    query = query.Where(i => i.AnneeScolaire != null
                        && i.AnneeScolaire.Statut == true
                        && i.AnneeScolaire.DateDebut <= now
                        && i.AnneeScolaire.DateFin >= now);
                    libelleAnnee = "Année en cours";
                }
                else
                {
                    var libelle = libelleAnneeScolaire.Trim().ToLower();
                    query = query.Where(i => i.AnneeScolaire != null
                        && i.AnneeScolaire.LibelleAnneeScolaire.ToLower() == libelle);
                    libelleAnnee = libelleAnneeScolaire.Trim();
                }

                inscriptions = await query
                    .OrderByDescending(i => i.DateInscription)
                    .ToListAsync(cancellationToken);

                // Si toutes les années courantes partagent le même libellé, l’exposer tel quel.
                if (string.IsNullOrWhiteSpace(libelleAnneeScolaire))
                {
                    var labels = inscriptions
                        .Select(i => i.AnneeScolaire?.LibelleAnneeScolaire)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    if (labels.Count == 1)
                        libelleAnnee = labels[0];
                }
            }

            // Une ligne par couple élève/école (enfant multi-écoles rare mais possible).
            var dernieres = inscriptions
                .GroupBy(i => new { i.IdEleve, i.IdEcole })
                .Select(g => g.OrderByDescending(i => i.DateInscription).First())
                .ToList();

            var eleveIds = dernieres.Select(i => i.IdEleve).Distinct().ToList();
            var anneeByEleveEcole = dernieres.ToDictionary(
                i => (i.IdEleve, i.IdEcole),
                i => i.IdAnneeScolaire);
            var anneeIds = anneeByEleveEcole.Values.Distinct().ToList();

            var paiementsRaw = eleveIds.Count == 0 || anneeIds.Count == 0
                ? new List<(int IdEleve, int IdAnneeScolaire, decimal Montant)>()
                : (await (
                    from p in _context.Paiements.AsNoTracking()
                    join f in _context.Frais.AsNoTracking() on p.IdFrais equals f.IdFrais
                    where p.Statut == true
                        && p.IdEleve != null
                        && eleveIds.Contains(p.IdEleve.Value)
                        && anneeIds.Contains(f.IdAnneeScolaire)
                    select new { IdEleve = p.IdEleve!.Value, f.IdAnneeScolaire, p.Montant }
                ).ToListAsync(cancellationToken))
                .Select(x => (IdEleve: x.IdEleve, IdAnneeScolaire: x.IdAnneeScolaire, Montant: (decimal)x.Montant))
                .ToList();

            // Paiements rattachés à l'année de l'inscription (via Frais.IdAnneeScolaire).
            var paiementsParEleveAnnee = paiementsRaw
                .GroupBy(p => (p.IdEleve, p.IdAnneeScolaire))
                .ToDictionary(g => g.Key, g => new
                {
                    Nombre = g.Count(),
                    Montant = g.Sum(x => x.Montant)
                });

            var enfants = new List<EnfantTuteurDto>();
            foreach (var inscription in dernieres)
            {
                var eleve = inscription.Eleve!;
                var stats = paiementsParEleveAnnee.TryGetValue(
                    (inscription.IdEleve, inscription.IdAnneeScolaire), out var s)
                    ? s
                    : new { Nombre = 0, Montant = 0m };

                enfants.Add(new EnfantTuteurDto
                {
                    IdEleve = eleve.IdEleve,
                    Matricule = eleve.Matricule,
                    NomComplet = eleve.NomComplet,
                    IdEcole = inscription.IdEcole,
                    NomEcole = inscription.Ecole?.Nom,
                    IdAnneeScolaire = inscription.IdAnneeScolaire,
                    IdClasse = inscription.IdClasse,
                    NomClasse = inscription.Classe?.NomClasse,
                    StatutInscription = inscription.StatutInscription,
                    NombrePaiements = stats.Nombre,
                    MontantPaye = stats.Montant,
                    AlertePaiement = stats.Nombre > 0 ? "Paiements enregistrés" : "Aucun paiement"
                });
            }

            var elevesAyantPaye = enfants.Count(e => e.NombrePaiements > 0);
            var elevesEnRetard = enfants.Count(e => e.NombrePaiements == 0);
            var alertes = new List<AlerteDto>();
            if (elevesEnRetard > 0)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "warning",
                    Message = $"{elevesEnRetard} enfant(s) n'ont pas encore de paiement enregistré pour l'année scolaire.",
                    Action = "Consulter les élèves concernés"
                });
            }

            var ecoles = dernieres
                .GroupBy(i => i.IdEcole)
                .Select(g =>
                {
                    var first = g.First();
                    return new EcoleInfoDto
                    {
                        IdEcole = g.Key,
                        NomEcole = first.Ecole?.Nom ?? string.Empty,
                        Logo = first.Ecole?.Logo
                    };
                })
                .OrderBy(e => e.NomEcole)
                .ToList();

            if (ecoleMono != null && ecoles.Count == 0)
                ecoles.Add(ecoleMono);

            return new DashboardTuteurDto
            {
                Ecole = ecoleMono,
                Ecoles = ecoles,
                IdAnneeScolaire = idAnneeMono,
                LibelleAnneeScolaire = libelleAnnee,
                Periode = periode,
                Resume = new ResumeTuteurDto
                {
                    NombreEnfants = enfants.Count,
                    ElevesActifs = enfants.Count,
                    ElevesAyantPaye = elevesAyantPaye,
                    ElevesEnRetardPaiement = elevesEnRetard,
                    MontantTotalPaye = enfants.Sum(e => e.MontantPaye),
                    NombreClasses = enfants.Select(e => e.IdClasse).Distinct().Count()
                },
                Enfants = enfants,
                Alertes = alertes
            };
        }

        private IQueryable<Inscription> BaseInscriptionsQuery(int idTuteur) =>
            _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Where(i => i.Statut == true
                    && i.Eleve != null
                    && i.Eleve.IdTuteur == idTuteur
                    && i.Eleve.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")));

        private static PeriodeDto BuildJourPeriode(DateTime targetDate)
        {
            var culture = new System.Globalization.CultureInfo("fr-FR");
            return new PeriodeDto
            {
                Type = "jour",
                Date = targetDate,
                DateDebut = targetDate,
                DateFin = targetDate,
                JoursOuvrables = targetDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 0 : 1,
                Libelle = targetDate.ToString("dddd dd MMMM yyyy", culture)
            };
        }
    }
}
