using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Résolution année scolaire et jeux d'élèves inscrits (inscription confirmée/active).
    /// </summary>
    public class EleveAnneeScopeHelper
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly IAnneeScolaireRepository _anneeScolaireRepository;

        public EleveAnneeScopeHelper(
            KelasiNaBisoDbContext context,
            IInscriptionActiveResolver inscriptionResolver,
            IAnneeScolaireRepository anneeScolaireRepository)
        {
            _context = context;
            _inscriptionResolver = inscriptionResolver;
            _anneeScolaireRepository = anneeScolaireRepository;
        }

        public async Task<int> ResolveIdAnneeScolaireAsync(int idEcole, int? idAnneeScolaire)
        {
            if (idAnneeScolaire.HasValue && idAnneeScolaire.Value > 0)
                return idAnneeScolaire.Value;

            var courante = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);
            if (courante == null)
            {
                throw new InvalidOperationException(
                    $"Aucune année scolaire en cours pour l'école {idEcole}. " +
                    "Précisez idAnneeScolaire ou créez une année scolaire couvrant la date du jour.");
            }

            return courante.IdAnneeScolaire;
        }

        /// <summary>
        /// Année de référence pour la recherche réinscription (query ou N-1).
        /// </summary>
        public async Task<int> ResolveIdAnneeScolaireReferenceForReinscriptionAsync(
            int idEcole,
            int? idAnneeScolaire)
        {
            if (idAnneeScolaire.HasValue && idAnneeScolaire.Value > 0)
                return idAnneeScolaire.Value;

            var precedente = await _anneeScolaireRepository.GetAnneePrecedenteAsync(idEcole);
            if (precedente == null)
            {
                throw new InvalidOperationException(
                    $"Aucune année scolaire de référence pour l'école {idEcole}. " +
                    "Précisez idAnneeScolaire ou créez une année scolaire antérieure.");
            }

            return precedente.IdAnneeScolaire;
        }

        public async Task<int?> TryGetIdAnneeCouranteAsync(int idEcole)
        {
            var courante = await _anneeScolaireRepository.GetAnneeCouranteAsync(idEcole);
            return courante?.IdAnneeScolaire;
        }

        public async Task<int> ResolveIdEcoleForClasseAsync(int idClasse)
        {
            var idEcole = await _context.Classes
                .AsNoTracking()
                .Where(c => c.IdClasse == idClasse)
                .Select(c => c.Direction != null ? c.Direction.IdEcole : null)
                .FirstOrDefaultAsync();

            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Classe {idClasse} introuvable ou non rattachée à une école.");
            }

            return idEcole.Value;
        }

        public async Task<int> ResolveIdEcoleForDirectionAsync(int idDirection)
        {
            var idEcole = await _context.Directions
                .AsNoTracking()
                .Where(d => d.IdDirection == idDirection)
                .Select(d => d.IdEcole)
                .FirstOrDefaultAsync();

            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Direction {idDirection} introuvable ou non rattachée à une école.");
            }

            return idEcole.Value;
        }

        public async Task<int> ResolveIdEcoleForOptionAsync(int idOption)
        {
            var idEcole = await _context.Options
                .AsNoTracking()
                .Where(o => o.IdOption == idOption)
                .Select(o => o.Section != null ? o.Section.IdEcole : null)
                .FirstOrDefaultAsync();

            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Option {idOption} introuvable ou non rattachée à une école.");
            }

            return idEcole.Value;
        }

        public async Task<int> ResolveIdEcoleForSectionAsync(int idSection)
        {
            var idEcole = await _context.Sections
                .AsNoTracking()
                .Where(s => s.IdSection == idSection)
                .Select(s => s.IdEcole)
                .FirstOrDefaultAsync();

            if (!idEcole.HasValue || idEcole.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Section {idSection} introuvable ou non rattachée à une école.");
            }

            return idEcole.Value;
        }

        public async Task<(int IdEcole, int IdAnneeScolaire)> ResolveEcoleAnneeAsync(
            int idEcole,
            int? idAnneeScolaire)
        {
            var annee = await ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            return (idEcole, annee);
        }

        public async Task<(int IdEcole, int IdAnneeScolaire)> ResolveClasseAnneeAsync(
            int idClasse,
            int? idAnneeScolaire)
        {
            var idEcole = await ResolveIdEcoleForClasseAsync(idClasse);
            var annee = await ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            return (idEcole, annee);
        }

        public IQueryable<int> GetEleveIdsInEcoleAnnee(int idEcole, int idAnneeScolaire) =>
            _inscriptionResolver
                .FilterElevesInEcole(_context.Eleves.AsNoTracking(), idEcole, idAnneeScolaire)
                .Select(e => e.IdEleve);

        public IQueryable<int> GetEleveIdsInClasseAnnee(int idClasse, int idAnneeScolaire) =>
            _inscriptionResolver
                .FilterElevesInClasse(_context.Eleves.AsNoTracking(), idClasse, idAnneeScolaire)
                .Select(e => e.IdEleve);

        public static ElevesAnneeScopedResult<T> Wrap<T>(T data, int idEcole, int idAnneeScolaire) =>
            new()
            {
                Data = data,
                IdEcole = idEcole,
                IdAnneeScolaire = idAnneeScolaire
            };
    }
}
