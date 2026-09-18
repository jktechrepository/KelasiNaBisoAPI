using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class CategorieDepenseService : ICategorieDepenseService
    {
        private readonly KelasiNaBisoDbContext _context;

        public CategorieDepenseService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CategorieDepenseDto>> GetByEcoleAsync(
            int idEcole,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.CategoriesDepense.AsNoTracking()
                .Where(c => c.IdEcole == idEcole);

            if (!includeInactive)
                query = query.Where(c => c.Statut);

            var list = await query
                .OrderBy(c => c.NomCategorie)
                .ToListAsync(cancellationToken);

            return list.Select(Map).ToList();
        }

        public async Task<CategorieDepenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.CategoriesDepense.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategorieDepense == id, cancellationToken);
            return entity == null ? null : Map(entity);
        }

        public async Task<CategorieDepenseDto> CreateAsync(
            CreateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            var ecoleOk = await _context.Ecoles.AsNoTracking()
                .AnyAsync(e => e.IdEcole == dto.IdEcole && e.Statut == true, cancellationToken);
            if (!ecoleOk)
                throw new InvalidOperationException($"École {dto.IdEcole} introuvable ou inactive.");

            var nom = dto.NomCategorie.Trim();
            if (string.IsNullOrWhiteSpace(nom))
                throw new InvalidOperationException("Le nom de catégorie est requis.");

            var entity = new CategorieDepense
            {
                IdEcole = dto.IdEcole,
                NomCategorie = nom,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Statut = true,
                DateCreation = DateTime.UtcNow
            };

            _context.CategoriesDepense.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<CategorieDepenseDto> UpdateAsync(
            int id,
            UpdateCategorieDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.CategoriesDepense
                .FirstOrDefaultAsync(c => c.IdCategorieDepense == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");

            if (!string.IsNullOrWhiteSpace(dto.NomCategorie))
                entity.NomCategorie = dto.NomCategorie.Trim();

            if (dto.Description != null)
                entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

            if (dto.Statut.HasValue)
                entity.Statut = dto.Statut.Value;

            await _context.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.CategoriesDepense
                .FirstOrDefaultAsync(c => c.IdCategorieDepense == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");

            // Soft : désactivation (évite FK orphelines sur dépenses historiques)
            entity.Statut = false;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static CategorieDepenseDto Map(CategorieDepense c) => new()
        {
            IdCategorieDepense = c.IdCategorieDepense,
            IdEcole = c.IdEcole,
            NomCategorie = c.NomCategorie,
            Description = c.Description,
            Statut = c.Statut,
            DateCreation = c.DateCreation
        };
    }
}
