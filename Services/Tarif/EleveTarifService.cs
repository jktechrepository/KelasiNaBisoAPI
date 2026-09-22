using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services.Tarif
{
    public class EleveTarifService : IEleveTarifService
    {
        private readonly KelasiNaBisoDbContext _context;

        public EleveTarifService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CategorieEleveTarifDto>> GetCategoriesAsync(
            int idEcole, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var q = _context.CategoriesEleveTarif.AsNoTracking()
                .Where(c => c.IdEcole == idEcole);
            if (!includeInactive)
                q = q.Where(c => c.Statut);

            return await q.OrderBy(c => c.Code)
                .Select(c => MapCategorie(c))
                .ToListAsync(cancellationToken);
        }

        public async Task<CategorieEleveTarifDto?> GetCategorieByIdAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var c = await _context.CategoriesEleveTarif.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCategorieEleveTarif == id, cancellationToken);
            return c == null ? null : MapCategorie(c);
        }

        public async Task<CategorieEleveTarifDto> CreateCategorieAsync(
            int idEcole, CreateCategorieEleveTarifDto dto, CancellationToken cancellationToken = default)
        {
            var code = NormalizeCode(dto.Code);
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Le code catégorie est obligatoire.");

            var exists = await _context.CategoriesEleveTarif
                .AnyAsync(c => c.IdEcole == idEcole && c.Code == code, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Une catégorie avec le code '{code}' existe déjà pour cette école.");

            var entity = new CategorieEleveTarif
            {
                IdEcole = idEcole,
                Code = code,
                Libelle = dto.Libelle.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Statut = dto.Statut,
                DateCreation = DateTime.UtcNow
            };

            _context.CategoriesEleveTarif.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return MapCategorie(entity);
        }

        public async Task<CategorieEleveTarifDto> UpdateCategorieAsync(
            int id, UpdateCategorieEleveTarifDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.CategoriesEleveTarif
                .FirstOrDefaultAsync(c => c.IdCategorieEleveTarif == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");

            entity.Libelle = dto.Libelle.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.Statut = dto.Statut;
            await _context.SaveChangesAsync(cancellationToken);
            return MapCategorie(entity);
        }

        public async Task SoftDeleteCategorieAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.CategoriesEleveTarif
                .FirstOrDefaultAsync(c => c.IdCategorieEleveTarif == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Catégorie {id} introuvable.");

            entity.Statut = false;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AffectationEleveCategorieTarifDto>> GetAffectationsEleveAsync(
            int idEleve, int? idAnneeScolaire = null, CancellationToken cancellationToken = default)
        {
            var q = from a in _context.AffectationsEleveCategorieTarif.AsNoTracking()
                    join c in _context.CategoriesEleveTarif.AsNoTracking()
                        on a.IdCategorieEleveTarif equals c.IdCategorieEleveTarif
                    where a.IdEleve == idEleve
                    select new { a, c };

            if (idAnneeScolaire.HasValue)
                q = q.Where(x => x.a.IdAnneeScolaire == idAnneeScolaire.Value);

            return await q.OrderByDescending(x => x.a.DateDebut)
                .Select(x => MapAffectation(x.a, x.c.Code, x.c.Libelle))
                .ToListAsync(cancellationToken);
        }

        public async Task<AffectationEleveCategorieTarifDto?> GetAffectationByIdAsync(
            int idAffectation, CancellationToken cancellationToken = default)
        {
            var row = await (
                from a in _context.AffectationsEleveCategorieTarif.AsNoTracking()
                join c in _context.CategoriesEleveTarif.AsNoTracking()
                    on a.IdCategorieEleveTarif equals c.IdCategorieEleveTarif
                where a.IdAffectationEleveCategorieTarif == idAffectation
                select new { a, c }
            ).FirstOrDefaultAsync(cancellationToken);

            return row == null ? null : MapAffectation(row.a, row.c.Code, row.c.Libelle);
        }

        public async Task<AffectationEleveCategorieTarifDto?> GetAffectationActiveAsync(
            int idEleve, int idAnneeScolaire, DateTime? asOfUtc = null, CancellationToken cancellationToken = default)
        {
            var asOf = asOfUtc ?? DateTime.UtcNow;
            var row = await (
                from a in _context.AffectationsEleveCategorieTarif.AsNoTracking()
                join c in _context.CategoriesEleveTarif.AsNoTracking()
                    on a.IdCategorieEleveTarif equals c.IdCategorieEleveTarif
                where a.IdEleve == idEleve
                      && a.IdAnneeScolaire == idAnneeScolaire
                      && a.DateDebut <= asOf
                      && (a.DateFin == null || a.DateFin >= asOf)
                orderby a.DateFin == null descending, a.DateDebut descending
                select new { a, c }
            ).FirstOrDefaultAsync(cancellationToken);

            return row == null ? null : MapAffectation(row.a, row.c.Code, row.c.Libelle);
        }

        public async Task<AffectationEleveCategorieTarifDto> AffecterAsync(
            CreateAffectationEleveCategorieTarifDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default)
        {
            var eleveExists = await _context.Eleves.AsNoTracking()
                .AnyAsync(e => e.IdEleve == dto.IdEleve, cancellationToken);
            if (!eleveExists)
                throw new KeyNotFoundException($"Élève {dto.IdEleve} introuvable.");

            var categorie = await _context.CategoriesEleveTarif
                .FirstOrDefaultAsync(c => c.IdCategorieEleveTarif == dto.IdCategorieEleveTarif && c.Statut, cancellationToken)
                ?? throw new KeyNotFoundException($"Catégorie {dto.IdCategorieEleveTarif} introuvable ou inactive.");

            var anneeExists = await _context.AnneeScolaires.AsNoTracking()
                .AnyAsync(a => a.IdAnneeScolaire == dto.IdAnneeScolaire, cancellationToken);
            if (!anneeExists)
                throw new KeyNotFoundException($"Année scolaire {dto.IdAnneeScolaire} introuvable.");

            var dateDebut = dto.DateDebut?.ToUniversalTime() ?? DateTime.UtcNow;
            if (dateDebut.Kind == DateTimeKind.Unspecified)
                dateDebut = DateTime.SpecifyKind(dateDebut, DateTimeKind.Utc);

            // Clôturer toute affectation ouverte (DateFin null) pour cet élève / année
            var ouvertes = await _context.AffectationsEleveCategorieTarif
                .Where(a => a.IdEleve == dto.IdEleve
                            && a.IdAnneeScolaire == dto.IdAnneeScolaire
                            && a.DateFin == null)
                .ToListAsync(cancellationToken);

            foreach (var o in ouvertes)
            {
                var fin = dateDebut > o.DateDebut ? dateDebut.AddTicks(-1) : dateDebut;
                o.DateFin = fin;
            }

            var entity = new AffectationEleveCategorieTarif
            {
                IdEleve = dto.IdEleve,
                IdCategorieEleveTarif = dto.IdCategorieEleveTarif,
                IdAnneeScolaire = dto.IdAnneeScolaire,
                DateDebut = dateDebut,
                DateFin = null,
                Motif = string.IsNullOrWhiteSpace(dto.Motif) ? null : dto.Motif.Trim(),
                IdAuteur = idAuteur,
                DateCreation = DateTime.UtcNow
            };

            _context.AffectationsEleveCategorieTarif.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return MapAffectation(entity, categorie.Code, categorie.Libelle);
        }

        public async Task<AffectationEleveCategorieTarifDto> CloturerAffectationAsync(
            int idAffectation,
            DateTime? dateFinUtc = null,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.AffectationsEleveCategorieTarif
                .FirstOrDefaultAsync(a => a.IdAffectationEleveCategorieTarif == idAffectation, cancellationToken)
                ?? throw new KeyNotFoundException($"Affectation {idAffectation} introuvable.");

            if (entity.DateFin.HasValue)
                throw new InvalidOperationException("Cette affectation est déjà clôturée.");

            var fin = dateFinUtc?.ToUniversalTime() ?? DateTime.UtcNow;
            if (fin.Kind == DateTimeKind.Unspecified)
                fin = DateTime.SpecifyKind(fin, DateTimeKind.Utc);

            if (fin < entity.DateDebut)
                throw new InvalidOperationException("La date de fin ne peut pas être antérieure à la date de début.");

            entity.DateFin = fin;
            await _context.SaveChangesAsync(cancellationToken);

            var cat = await _context.CategoriesEleveTarif.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategorieEleveTarif == entity.IdCategorieEleveTarif, cancellationToken);

            return MapAffectation(entity, cat?.Code, cat?.Libelle);
        }

        public async Task<IReadOnlyList<RegleExonerationFraisDto>> GetReglesAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idCategorie = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var q = from r in _context.ReglesExonerationFrais.AsNoTracking()
                    join c in _context.CategoriesEleveTarif.AsNoTracking()
                        on r.IdCategorieEleveTarif equals c.IdCategorieEleveTarif
                    join f in _context.Frais.AsNoTracking()
                        on r.IdFrais equals f.IdFrais
                    where r.IdEcole == idEcole
                    select new { r, c, f };

            if (!includeInactive)
                q = q.Where(x => x.r.Statut);
            if (idAnneeScolaire.HasValue)
                q = q.Where(x => x.r.IdAnneeScolaire == idAnneeScolaire.Value);
            if (idCategorie.HasValue)
                q = q.Where(x => x.r.IdCategorieEleveTarif == idCategorie.Value);

            return await q.OrderBy(x => x.c.Code).ThenBy(x => x.f.LibelleFrais)
                .Select(x => MapRegle(x.r, x.c.Code, x.f.LibelleFrais))
                .ToListAsync(cancellationToken);
        }

        public async Task<RegleExonerationFraisDto?> GetRegleByIdAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var row = await (
                from r in _context.ReglesExonerationFrais.AsNoTracking()
                join c in _context.CategoriesEleveTarif.AsNoTracking()
                    on r.IdCategorieEleveTarif equals c.IdCategorieEleveTarif
                join f in _context.Frais.AsNoTracking()
                    on r.IdFrais equals f.IdFrais
                where r.IdRegleExonerationFrais == id
                select new { r, c, f }
            ).FirstOrDefaultAsync(cancellationToken);

            return row == null ? null : MapRegle(row.r, row.c.Code, row.f.LibelleFrais);
        }

        public async Task<RegleExonerationFraisDto> UpsertRegleAsync(
            int idEcole,
            UpsertRegleExonerationFraisDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default)
        {
            if (!TypeRegleExonerationFrais.IsKnown(dto.TypeRegle))
                throw new InvalidOperationException(
                    "TypeRegle invalide. Valeurs : Totale, Pourcentage, MontantReduction, MontantDuFixe.");

            if (dto.TypeRegle == TypeRegleExonerationFrais.Pourcentage && (dto.Valeur < 0 || dto.Valeur > 100))
                throw new InvalidOperationException("Pour un type Pourcentage, Valeur doit être entre 0 et 100.");

            var categorie = await _context.CategoriesEleveTarif.AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.IdCategorieEleveTarif == dto.IdCategorieEleveTarif
                    && c.IdEcole == idEcole, cancellationToken)
                ?? throw new KeyNotFoundException("Catégorie introuvable pour cette école.");

            var frais = await _context.Frais.AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.IdFrais == dto.IdFrais
                    && f.IdEcole == idEcole
                    && f.IdAnneeScolaire == dto.IdAnneeScolaire, cancellationToken)
                ?? throw new KeyNotFoundException(
                    "Frais introuvable pour cette école / année scolaire.");

            var existing = await _context.ReglesExonerationFrais
                .FirstOrDefaultAsync(r =>
                    r.IdAnneeScolaire == dto.IdAnneeScolaire
                    && r.IdCategorieEleveTarif == dto.IdCategorieEleveTarif
                    && r.IdFrais == dto.IdFrais, cancellationToken);

            if (existing == null)
            {
                existing = new RegleExonerationFrais
                {
                    IdEcole = idEcole,
                    IdAnneeScolaire = dto.IdAnneeScolaire,
                    IdCategorieEleveTarif = dto.IdCategorieEleveTarif,
                    IdFrais = dto.IdFrais,
                    DateCreation = DateTime.UtcNow
                };
                _context.ReglesExonerationFrais.Add(existing);
            }
            else
            {
                existing.DateModification = DateTime.UtcNow;
            }

            existing.TypeRegle = dto.TypeRegle;
            existing.Valeur = dto.TypeRegle == TypeRegleExonerationFrais.Totale ? 0m : dto.Valeur;
            existing.Statut = dto.Statut;
            existing.IdAuteur = idAuteur;

            await _context.SaveChangesAsync(cancellationToken);
            return MapRegle(existing, categorie.Code, frais.LibelleFrais);
        }

        public async Task SoftDeleteRegleAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ReglesExonerationFrais
                .FirstOrDefaultAsync(r => r.IdRegleExonerationFrais == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Règle {id} introuvable.");

            entity.Statut = false;
            entity.DateModification = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string NormalizeCode(string code) =>
            (code ?? string.Empty).Trim().ToUpperInvariant();

        private static CategorieEleveTarifDto MapCategorie(CategorieEleveTarif c) => new()
        {
            IdCategorieEleveTarif = c.IdCategorieEleveTarif,
            IdEcole = c.IdEcole,
            Code = c.Code,
            Libelle = c.Libelle,
            Description = c.Description,
            Statut = c.Statut
        };

        private static AffectationEleveCategorieTarifDto MapAffectation(
            AffectationEleveCategorieTarif a, string? code, string? libelle) => new()
        {
            IdAffectationEleveCategorieTarif = a.IdAffectationEleveCategorieTarif,
            IdEleve = a.IdEleve,
            IdCategorieEleveTarif = a.IdCategorieEleveTarif,
            CodeCategorie = code,
            LibelleCategorie = libelle,
            IdAnneeScolaire = a.IdAnneeScolaire,
            DateDebut = a.DateDebut,
            DateFin = a.DateFin,
            Motif = a.Motif
        };

        private static RegleExonerationFraisDto MapRegle(
            RegleExonerationFrais r, string? codeCat, string? libelleFrais) => new()
        {
            IdRegleExonerationFrais = r.IdRegleExonerationFrais,
            IdEcole = r.IdEcole,
            IdAnneeScolaire = r.IdAnneeScolaire,
            IdCategorieEleveTarif = r.IdCategorieEleveTarif,
            CodeCategorie = codeCat,
            IdFrais = r.IdFrais,
            LibelleFrais = libelleFrais,
            TypeRegle = r.TypeRegle,
            Valeur = r.Valeur,
            Statut = r.Statut
        };
    }
}
