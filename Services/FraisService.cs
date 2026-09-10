using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class FraisService : IFraisRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public FraisService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        private IQueryable<Frais> QueryWithPortee() =>
            _context.Frais
                .AsNoTracking()
                .Include(f => f.Ecole)
                .Include(f => f.AnneeScolaire)
                .Include(f => f.FraisDirections).ThenInclude(fd => fd.Direction)
                .Include(f => f.FraisClasses).ThenInclude(fc => fc.Classe);

        public async Task<IEnumerable<FraisDto>> GetAllAsync()
        {
            var data = await QueryWithPortee()
                .Where(f => f.Statut == true)
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
            return data.Select(FraisDto.FromEntity);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<FraisDto>>> GetByEcoleAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            IQueryable<Frais> query = QueryWithPortee()
                .Where(f => f.IdEcole == ecole
                    && f.IdAnneeScolaire == annee
                    && f.Statut == true);

            if (idClasse.HasValue && idClasse.Value > 0)
            {
                var idDirection = await _context.Classes.AsNoTracking()
                    .Where(c => c.IdClasse == idClasse.Value)
                    .Select(c => c.IdDirection)
                    .FirstOrDefaultAsync();

                if (!idDirection.HasValue || idDirection.Value <= 0)
                    throw new InvalidOperationException($"Classe {idClasse.Value} introuvable ou sans direction.");

                query = FraisEligibility.FilterForInscription(
                    query, ecole, idDirection.Value, annee, idClasse.Value);
            }

            var data = await query.OrderByDescending(f => f.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<FraisDto>>(
                data.Select(FraisDto.FromEntity).ToList(), ecole, annee);
        }

        public async Task<FraisDto?> GetByIdAsync(int id)
        {
            var frais = await QueryWithPortee()
                .FirstOrDefaultAsync(f => f.IdFrais == id);
            return frais == null ? null : FraisDto.FromEntity(frais);
        }

        public async Task<FraisDto?> GetByLibelleFraisAsync(string libelleFrais)
        {
            var frais = await QueryWithPortee()
                .FirstOrDefaultAsync(f => f.LibelleFrais == libelleFrais);
            return frais == null ? null : FraisDto.FromEntity(frais);
        }

        public async Task<ElevesAnneeScopedResult<FraisDto?>> GetByEcoleAndLibelleAsync(
            int idEcole,
            string libelleFrais,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var libelle = libelleFrais.ToLower();

            var candidats = await QueryWithPortee()
                .Where(f => f.IdEcole == ecole
                    && f.IdAnneeScolaire == annee
                    && f.LibelleFrais.ToLower() == libelle
                    && f.Statut == true)
                .ToListAsync();

            Frais? match;
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                var idDirection = await _context.Classes.AsNoTracking()
                    .Where(c => c.IdClasse == idClasse.Value)
                    .Select(c => c.IdDirection)
                    .FirstOrDefaultAsync();

                match = idDirection.HasValue && idDirection.Value > 0
                    ? candidats.FirstOrDefault(f =>
                        FraisEligibility.IsEligibleForInscription(
                            f, ecole, idDirection.Value, annee, idClasse.Value))
                    : null;
            }
            else
            {
                match = candidats.FirstOrDefault(f => f.Portee == PorteeFrais.Direction)
                        ?? candidats.FirstOrDefault();
            }

            return EleveAnneeScopeHelper.Wrap(
                match == null ? null : FraisDto.FromEntity(match), ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<FraisDto>>> GetByDirectionAsync(
            int idDirection,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForDirectionAsync(idDirection);
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);

            IQueryable<Frais> query = QueryWithPortee()
                .Where(f => f.IdEcole == ecole
                    && f.IdAnneeScolaire == annee
                    && f.Statut == true);

            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = FraisEligibility.FilterForInscription(
                    query, ecole, idDirection, annee, idClasse.Value);
            }
            else
            {
                query = query.Where(f =>
                    (f.Portee == PorteeFrais.Direction
                        && f.FraisDirections.Any(fd => fd.IdDirection == idDirection))
                    || (f.Portee == PorteeFrais.Classe
                        && f.FraisClasses.Any(fc => fc.Classe.IdDirection == idDirection)));
            }

            var data = await query.OrderByDescending(f => f.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<FraisDto>>(
                data.Select(FraisDto.FromEntity).ToList(), ecole, annee);
        }

        public async Task<IEnumerable<FraisDto>> GetByAnneeAsync(int idAnneeScolaire)
        {
            var data = await QueryWithPortee()
                .Where(f => f.IdAnneeScolaire == idAnneeScolaire && f.Statut == true)
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
            return data.Select(FraisDto.FromEntity);
        }

        public async Task<FraisDto> CreateAsync(CreateFraisDto dto)
        {
            var idAnnee = dto.IdAnneeScolaire;
            if (idAnnee <= 0)
                idAnnee = await _scope.ResolveIdAnneeScolaireAsync(dto.IdEcole, null);

            var frais = new Frais
            {
                LibelleFrais = dto.LibelleFrais,
                Montant = dto.Montant,
                Devise = dto.Devise,
                TypeFrais = dto.TypeFrais,
                Periodicite = dto.Periodicite,
                Description = dto.Description,
                IdEcole = dto.IdEcole,
                IdAnneeScolaire = idAnnee,
                Portee = dto.Portee,
                Statut = dto.Statut ?? true,
                DateCreation = DateTime.Now
            };

            await ValidateAndAssignPorteeAsync(frais, dto.Portee, dto.IdDirections, dto.IdClasses);
            _context.Frais.Add(frais);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(frais.IdFrais))!;
        }

        public async Task<FraisDto?> UpdateAsync(int id, UpdateFraisDto dto)
        {
            var existing = await _context.Frais
                .Include(f => f.FraisDirections)
                .Include(f => f.FraisClasses)
                .FirstOrDefaultAsync(f => f.IdFrais == id);
            if (existing == null)
                return null;

            existing.LibelleFrais = dto.LibelleFrais ?? existing.LibelleFrais;
            existing.Montant = dto.Montant;
            existing.Devise = dto.Devise ?? existing.Devise;
            existing.TypeFrais = dto.TypeFrais;
            existing.Periodicite = dto.Periodicite;
            existing.Description = dto.Description;

            if (dto.IdAnneeScolaire.HasValue && dto.IdAnneeScolaire.Value > 0)
                existing.IdAnneeScolaire = dto.IdAnneeScolaire.Value;

            var portee = dto.Portee ?? existing.Portee;
            var replacePortee = dto.Portee.HasValue
                || dto.IdDirections != null
                || dto.IdClasses != null;

            if (replacePortee)
            {
                await ValidateAndAssignPorteeAsync(existing, portee, dto.IdDirections, dto.IdClasses);
            }
            else
            {
                await ValidateEcoleAnneeAsync(existing.IdEcole, existing.IdAnneeScolaire);
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var frais = await _context.Frais.FindAsync(id);
            if (frais == null)
                return false;

            _context.Frais.Remove(frais);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Frais.AnyAsync(f => f.IdFrais == id);
        }

        public async Task<bool> ExistsByLibelleFraisAsync(string libelleFrais)
        {
            return await _context.Frais.AnyAsync(f => f.LibelleFrais == libelleFrais);
        }

        public async Task<IEnumerable<Paiement>> GetPaiementsAsync(int idFrais)
        {
            return await _context.Paiements
                .Include(p => p.Eleve)
                .Include(p => p.Utilisateur)
                .Where(p => p.IdFrais == idFrais)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<bool> ToggleStatutAsync(int id)
        {
            var frais = await _context.Frais.FindAsync(id);
            if (frais == null)
                return false;

            frais.Statut = frais.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidateEcoleAnneeAsync(int idEcole, int idAnneeScolaire)
        {
            var ecole = await _context.Ecoles.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole && e.Statut == true)
                ?? throw new InvalidOperationException($"École {idEcole} introuvable.");

            var annee = await _context.AnneeScolaires.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == idAnneeScolaire && a.Statut == true)
                ?? throw new InvalidOperationException($"Année scolaire {idAnneeScolaire} introuvable.");

            if (annee.IdEcole != idEcole)
            {
                throw new InvalidOperationException(
                    "L'année scolaire doit appartenir à la même école que le frais.");
            }
        }

        private async Task ValidateAndAssignPorteeAsync(
            Frais frais,
            PorteeFrais portee,
            List<int>? idDirections,
            List<int>? idClasses)
        {
            await ValidateEcoleAnneeAsync(frais.IdEcole, frais.IdAnneeScolaire);
            frais.Portee = portee;

            var directions = (idDirections ?? new List<int>()).Where(id => id > 0).Distinct().ToList();
            var classes = (idClasses ?? new List<int>()).Where(id => id > 0).Distinct().ToList();

            if (portee == PorteeFrais.Direction)
            {
                if (directions.Count == 0)
                    throw new InvalidOperationException("Un frais de portée Direction doit avoir au moins une direction.");
                if (classes.Count > 0)
                    throw new InvalidOperationException("Un frais de portée Direction ne peut pas lister des classes.");

                var found = await _context.Directions.AsNoTracking()
                    .Where(d => directions.Contains(d.IdDirection) && d.Statut == true)
                    .ToListAsync();
                if (found.Count != directions.Count)
                    throw new InvalidOperationException("Une ou plusieurs directions sont introuvables ou inactives.");
                if (found.Any(d => d.IdEcole != frais.IdEcole))
                    throw new InvalidOperationException("Toutes les directions doivent appartenir à l'école du frais.");

                _context.FraisDirections.RemoveRange(frais.FraisDirections);
                frais.FraisDirections = directions
                    .Select(id => new FraisDirection { IdFrais = frais.IdFrais, IdDirection = id })
                    .ToList();
                _context.FraisClasses.RemoveRange(frais.FraisClasses);
                frais.FraisClasses.Clear();
            }
            else if (portee == PorteeFrais.Classe)
            {
                if (classes.Count == 0)
                    throw new InvalidOperationException("Un frais de portée Classe doit avoir au moins une classe.");
                if (directions.Count > 0)
                    throw new InvalidOperationException("Un frais de portée Classe ne peut pas lister des directions.");

                var found = await _context.Classes.AsNoTracking()
                    .Include(c => c.Direction)
                    .Where(c => classes.Contains(c.IdClasse) && c.Statut == true)
                    .ToListAsync();
                if (found.Count != classes.Count)
                    throw new InvalidOperationException("Une ou plusieurs classes sont introuvables ou inactives.");
                if (found.Any(c => c.Direction?.IdEcole != frais.IdEcole))
                    throw new InvalidOperationException("Toutes les classes doivent appartenir à l'école du frais.");

                _context.FraisClasses.RemoveRange(frais.FraisClasses);
                frais.FraisClasses = classes
                    .Select(id => new FraisClasse { IdFrais = frais.IdFrais, IdClasse = id })
                    .ToList();
                _context.FraisDirections.RemoveRange(frais.FraisDirections);
                frais.FraisDirections.Clear();
            }
            else
            {
                throw new InvalidOperationException("Portée de frais invalide (Direction ou Classe).");
            }
        }
    }
}
