using KelasiNaBiso.Data;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Helper ACL pédagogique : TitulaireClasse ∪ AffectationCours, filtrés par année scolaire.
    /// Remplace progressivement les checks dispersés dans DevoirADomicileService / DevoirADomicileHub.
    /// </summary>
    public class PedagogieAuthorizationService : IPedagogieAuthorizationService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;
        private readonly ILogger<PedagogieAuthorizationService> _logger;

        public PedagogieAuthorizationService(
            KelasiNaBisoDbContext context,
            IAnneeScolaireRepository anneeScolaireRepository,
            ILogger<PedagogieAuthorizationService> logger)
        {
            _context = context;
            _anneeScolaireRepository = anneeScolaireRepository;
            _logger = logger;
        }

        public async Task<int?> ResolveIdAnneeScolairePourClasseAsync(
            int idClasse,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            if (idAnneeScolaire.HasValue && idAnneeScolaire.Value > 0)
                return idAnneeScolaire.Value;

            var idEcole = await _context.Classes
                .AsNoTracking()
                .Where(c => c.IdClasse == idClasse)
                .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                .FirstOrDefaultAsync(cancellationToken);

            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                _logger.LogWarning("Classe {IdClasse} sans école — impossible de résoudre l'année courante", idClasse);
                return null;
            }

            var courante = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole.Value);
            return courante?.IdAnneeScolaire;
        }

        public async Task<bool> AgentEnseigneClasseAsync(
            int idAgent,
            int idClasse,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var resolvedAnnee = await ResolveIdAnneeScolairePourClasseAsync(idClasse, idAnneeScolaire, cancellationToken);
            if (!resolvedAnnee.HasValue)
            {
                _logger.LogWarning(
                    "AgentEnseigneClasse : aucune année résolue pour agent {IdAgent} / classe {IdClasse}",
                    idAgent, idClasse);
                return false;
            }

            var estTitulaire = await _context.TitulairesClasses
                .AsNoTracking()
                .AnyAsync(tc =>
                    tc.IdAgent == idAgent
                    && tc.IdClasse == idClasse
                    && tc.IdAnneeScolaire == resolvedAnnee.Value
                    && tc.Statut == true,
                    cancellationToken);

            if (estTitulaire)
                return true;

            return await _context.AffectationsCours
                .AsNoTracking()
                .AnyAsync(ac =>
                    ac.IdAgent == idAgent
                    && ac.IdAnneeScolaire == resolvedAnnee.Value
                    && ac.Statut == true
                    && ac.Cours != null
                    && ac.Cours.IdClasse == idClasse
                    && ac.Cours.Statut == true,
                    cancellationToken);
        }

        public async Task<bool> AgentEnseigneCoursAsync(
            int idAgent,
            int idCours,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var idClasse = await _context.Cours
                .AsNoTracking()
                .Where(c => c.IdCours == idCours)
                .Select(c => (int?)c.IdClasse)
                .FirstOrDefaultAsync(cancellationToken);

            if (!idClasse.HasValue)
                return false;

            var resolvedAnnee = await ResolveIdAnneeScolairePourClasseAsync(idClasse.Value, idAnneeScolaire, cancellationToken);
            if (!resolvedAnnee.HasValue)
                return false;

            // Titulaire de la classe du cours = autorisé sur tous les cours de la classe
            var estTitulaire = await _context.TitulairesClasses
                .AsNoTracking()
                .AnyAsync(tc =>
                    tc.IdAgent == idAgent
                    && tc.IdClasse == idClasse.Value
                    && tc.IdAnneeScolaire == resolvedAnnee.Value
                    && tc.Statut == true,
                    cancellationToken);

            if (estTitulaire)
                return true;

            return await _context.AffectationsCours
                .AsNoTracking()
                .AnyAsync(ac =>
                    ac.IdAgent == idAgent
                    && ac.IdCours == idCours
                    && ac.IdAnneeScolaire == resolvedAnnee.Value
                    && ac.Statut == true,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<int>> GetClassesEnseignantAsync(
            int idAgent,
            int? idAnneeScolaire = null,
            int? idEcole = null,
            CancellationToken cancellationToken = default)
        {
            int? resolvedAnnee = idAnneeScolaire;
            if ((!resolvedAnnee.HasValue || resolvedAnnee.Value <= 0) && idEcole.HasValue && idEcole.Value > 0)
            {
                var courante = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole.Value);
                resolvedAnnee = courante?.IdAnneeScolaire;
            }

            if (!resolvedAnnee.HasValue || resolvedAnnee.Value <= 0)
            {
                _logger.LogWarning(
                    "GetClassesEnseignant : année non résolue pour agent {IdAgent} (passez idAnneeScolaire ou idEcole)",
                    idAgent);
                return Array.Empty<int>();
            }

            var classesTitulaire = await _context.TitulairesClasses
                .AsNoTracking()
                .Where(tc =>
                    tc.IdAgent == idAgent
                    && tc.IdAnneeScolaire == resolvedAnnee.Value
                    && tc.Statut == true)
                .Select(tc => tc.IdClasse)
                .ToListAsync(cancellationToken);

            var classesAffectation = await _context.AffectationsCours
                .AsNoTracking()
                .Where(ac =>
                    ac.IdAgent == idAgent
                    && ac.IdAnneeScolaire == resolvedAnnee.Value
                    && ac.Statut == true
                    && ac.Cours != null
                    && ac.Cours.Statut == true
                    && ac.Cours.IdClasse != null)
                .Select(ac => ac.Cours!.IdClasse!.Value)
                .ToListAsync(cancellationToken);

            return classesTitulaire
                .Concat(classesAffectation)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
        }
    }
}
