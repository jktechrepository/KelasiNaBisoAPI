using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public interface IDashboardEleveService
    {
        /// <summary>
        /// Dashboard élève : vue soi-même (inscription active + paiements année).
        /// </summary>
        Task<DashboardEleveDto> BuildAsync(
            int idEleve,
            int? idEcole,
            string? libelleAnneeScolaire,
            CancellationToken cancellationToken = default);
    }

    public class DashboardEleveService : IDashboardEleveService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public DashboardEleveService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<DashboardEleveDto> BuildAsync(
            int idEleve,
            int? idEcole,
            string? libelleAnneeScolaire,
            CancellationToken cancellationToken = default)
        {
            var targetDate = DateTime.Now.Date;
            var periode = BuildJourPeriode(targetDate);
            var now = DateTime.Now;

            IQueryable<Inscription> query = BaseInscriptionsQuery(idEleve)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Include(i => i.Classe)
                .Include(i => i.Eleve);

            string? libelleAnnee;
            int? idAnneeResolved = null;
            EcoleInfoDto? ecoleDto = null;

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
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new InvalidOperationException(
                            $"Aucune année scolaire active avec le libellé '{libelleAnneeScolaire.Trim()}' pour l'école {resolvedEcole}.");

                    idAnnee = annee.IdAnneeScolaire;
                    libelleAnnee = annee.LibelleAnneeScolaire;
                }

                var ecole = await _context.Ecoles.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEcole == resolvedEcole, cancellationToken)
                    ?? throw new InvalidOperationException($"École avec l'ID {resolvedEcole} introuvable.");

                ecoleDto = new EcoleInfoDto
                {
                    IdEcole = ecole.IdEcole,
                    NomEcole = ecole.Nom ?? string.Empty,
                    Logo = ecole.Logo
                };
                idAnneeResolved = idAnnee;
                query = query.Where(i => i.IdEcole == resolvedEcole && i.IdAnneeScolaire == idAnnee);
            }
            else
            {
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
            }

            var inscription = await query
                .OrderByDescending(i => i.DateInscription)
                .FirstOrDefaultAsync(cancellationToken);

            if (inscription == null)
            {
                return new DashboardEleveDto
                {
                    Ecole = ecoleDto,
                    IdAnneeScolaire = idAnneeResolved,
                    LibelleAnneeScolaire = libelleAnnee,
                    Periode = periode,
                    Profil = null,
                    Resume = new ResumeEleveDto
                    {
                        MessagePaiement = "Aucune inscription active trouvée"
                    },
                    Alertes =
                    {
                        new AlerteDto
                        {
                            Type = "info",
                            Message = "Aucune inscription confirmée pour cet élève sur la période demandée.",
                            Action = "Contacter le secrétariat"
                        }
                    }
                };
            }

            idAnneeResolved ??= inscription.IdAnneeScolaire;
            libelleAnnee = inscription.AnneeScolaire?.LibelleAnneeScolaire ?? libelleAnnee;
            ecoleDto ??= new EcoleInfoDto
            {
                IdEcole = inscription.IdEcole,
                NomEcole = inscription.Ecole?.Nom ?? string.Empty,
                Logo = inscription.Ecole?.Logo
            };

            var paiements = await (
                from p in _context.Paiements.AsNoTracking()
                join f in _context.Frais.AsNoTracking() on p.IdFrais equals f.IdFrais
                where p.Statut == true
                    && p.IdEleve == idEleve
                    && f.IdAnneeScolaire == inscription.IdAnneeScolaire
                select p.Montant
            ).ToListAsync(cancellationToken);

            var nombrePaiements = paiements.Count;
            var montantPaye = paiements.Sum(m => (decimal)m);
            var alertePaiement = nombrePaiements == 0;

            var alertes = new List<AlerteDto>();
            if (alertePaiement)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "warning",
                    Message = "Aucun paiement enregistré pour l'année scolaire.",
                    Action = "Consulter le secrétariat / parent"
                });
            }

            var eleve = inscription.Eleve!;
            return new DashboardEleveDto
            {
                Ecole = ecoleDto,
                IdAnneeScolaire = idAnneeResolved,
                LibelleAnneeScolaire = libelleAnnee,
                Periode = periode,
                Profil = new EleveProfilDto
                {
                    IdEleve = eleve.IdEleve,
                    Matricule = eleve.Matricule,
                    NomComplet = eleve.NomComplet,
                    IdEcole = inscription.IdEcole,
                    NomEcole = inscription.Ecole?.Nom,
                    IdAnneeScolaire = inscription.IdAnneeScolaire,
                    IdClasse = inscription.IdClasse,
                    NomClasse = inscription.Classe?.NomClasse,
                    StatutInscription = inscription.StatutInscription
                },
                Resume = new ResumeEleveDto
                {
                    NombrePaiements = nombrePaiements,
                    MontantPaye = montantPaye,
                    AlertePaiement = alertePaiement,
                    MessagePaiement = alertePaiement ? "Aucun paiement" : "Paiements enregistrés"
                },
                Alertes = alertes
            };
        }

        private IQueryable<Inscription> BaseInscriptionsQuery(int idEleve) =>
            _context.Inscriptions
                .AsNoTracking()
                .Where(i => i.Statut == true
                    && i.IdEleve == idEleve
                    && i.Eleve != null
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
