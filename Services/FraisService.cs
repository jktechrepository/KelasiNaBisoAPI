using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
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

        public async Task<IEnumerable<Frais>> GetAllAsync()
        {
            return await _context.Frais
                .AsNoTracking()
                .Where(f => f.Statut == true)
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Frais>>> GetByEcoleAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var query = _context.Frais.AsNoTracking()
                .Where(f => f.Direction.IdEcole == ecole
                    && f.IdAnneeScolaire == annee
                    && f.Statut == true);

            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = query.Where(f => f.IdClasse == null || f.IdClasse == idClasse.Value);
            }

            var data = await query.OrderByDescending(f => f.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Frais>>(data, ecole, annee);
        }

        public async Task<Frais?> GetByIdAsync(int id)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Include(f => f.AnneeScolaire)
                .Include(f => f.Classe)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.IdFrais == id);
        }

        public async Task<Frais?> GetByLibelleFraisAsync(string libelleFrais)
        {
            return await _context.Frais
                .Include(f => f.Direction)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.LibelleFrais == libelleFrais);
        }

        public async Task<ElevesAnneeScopedResult<Frais?>> GetByEcoleAndLibelleAsync(
            int idEcole,
            string libelleFrais,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var libelle = libelleFrais.ToLower();

            var candidats = await _context.Frais.AsNoTracking()
                .Include(f => f.Direction)
                .Where(f => f.Direction.IdEcole == ecole
                    && f.IdAnneeScolaire == annee
                    && f.LibelleFrais.ToLower() == libelle
                    && f.Statut == true)
                .ToListAsync();

            Frais? match = null;
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                match = candidats.FirstOrDefault(f => f.IdClasse == idClasse.Value)
                    ?? candidats.FirstOrDefault(f => f.IdClasse == null);
            }
            else
            {
                match = candidats.FirstOrDefault(f => f.IdClasse == null)
                    ?? candidats.FirstOrDefault();
            }

            return EleveAnneeScopeHelper.Wrap(match, ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Frais>>> GetByDirectionAsync(
            int idDirection,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForDirectionAsync(idDirection);
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);

            var query = _context.Frais.AsNoTracking()
                .Where(f => f.IdDirection == idDirection
                    && f.IdAnneeScolaire == annee
                    && f.Statut == true);

            if (idClasse.HasValue && idClasse.Value > 0)
            {
                query = query.Where(f => f.IdClasse == null || f.IdClasse == idClasse.Value);
            }

            var data = await query.OrderByDescending(f => f.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Frais>>(data, ecole, annee);
        }

        public async Task<IEnumerable<Frais>> GetByAnneeAsync(int idAnneeScolaire)
        {
            return await _context.Frais.AsNoTracking()
                .Where(f => f.IdAnneeScolaire == idAnneeScolaire && f.Statut == true)
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();
        }

        public async Task<Frais> CreateAsync(Frais frais)
        {
            if (frais.IdAnneeScolaire <= 0)
            {
                var idEcole = await _scope.ResolveIdEcoleForDirectionAsync(frais.IdDirection);
                frais.IdAnneeScolaire = await _scope.ResolveIdAnneeScolaireAsync(idEcole, null);
            }

            await ValidateFraisRelationsAsync(frais);
            frais.DateCreation = DateTime.Now;
            _context.Frais.Add(frais);
            await _context.SaveChangesAsync();
            return frais;
        }

        public async Task<Frais?> UpdateAsync(Frais frais)
        {
            var existingFrais = await _context.Frais.FindAsync(frais.IdFrais);
            if (existingFrais == null)
                return null;

            await ValidateFraisRelationsAsync(frais);
            _context.Entry(existingFrais).CurrentValues.SetValues(frais);
            await _context.SaveChangesAsync();
            return existingFrais;
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

        private async Task ValidateFraisRelationsAsync(Frais frais)
        {
            if (frais.IdAnneeScolaire <= 0)
                throw new InvalidOperationException("IdAnneeScolaire est obligatoire.");

            var direction = await _context.Directions.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDirection == frais.IdDirection && d.Statut == true)
                ?? throw new InvalidOperationException($"Direction {frais.IdDirection} introuvable.");

            var annee = await _context.AnneeScolaires.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == frais.IdAnneeScolaire && a.Statut == true)
                ?? throw new InvalidOperationException($"Année scolaire {frais.IdAnneeScolaire} introuvable.");

            if (annee.IdEcole != direction.IdEcole)
            {
                throw new InvalidOperationException(
                    "L'année scolaire doit appartenir à la même école que la direction du frais.");
            }

            if (frais.IdClasse.HasValue && frais.IdClasse.Value > 0)
            {
                var classe = await _context.Classes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdClasse == frais.IdClasse.Value && c.Statut == true)
                    ?? throw new InvalidOperationException($"Classe {frais.IdClasse} introuvable.");

                if (classe.IdDirection != frais.IdDirection)
                {
                    throw new InvalidOperationException(
                        "La classe doit appartenir à la même direction que le frais.");
                }
            }
            else
            {
                frais.IdClasse = null;
            }
        }
    }
}
