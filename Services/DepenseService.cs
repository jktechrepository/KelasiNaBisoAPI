using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class DepenseService : IDepenseService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ICurrencyConversionService _currencyConversion;

        public DepenseService(
            KelasiNaBisoDbContext context,
            ICurrencyConversionService currencyConversion)
        {
            _context = context;
            _currencyConversion = currencyConversion;
        }

        public async Task<PagedResult<DepenseDto>> GetPagedAsync(
            int? idEcole,
            DateTime? dateDebut,
            DateTime? dateFin,
            int? idCategorieDepense,
            string? statut,
            PagedRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = BuildBaseQuery(idEcole, dateDebut, dateFin, idCategorieDepense, statut, includeInactive: false);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(d =>
                    d.Libelle.Contains(term)
                    || (d.Beneficiaire != null && d.Beneficiaire.Contains(term))
                    || (d.ReferencePiece != null && d.ReferencePiece.Contains(term))
                    || (d.Description != null && d.Description.Contains(term)));
            }

            query = request.SortDescending
                ? query.OrderByDescending(d => d.DateDepense).ThenByDescending(d => d.IdDepense)
                : query.OrderBy(d => d.DateDepense).ThenBy(d => d.IdDepense);

            var total = await query.CountAsync(cancellationToken);
            var page = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);

            var entities = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            var data = entities
                .Select(d => Map(d, d.CategorieDepense?.NomCategorie, FormatNomCreateur(d.UtilisateurCreateur)))
                .ToList();
            return new PagedResult<DepenseDto>(data, total, page, size);
        }

        public async Task<DepenseMoisDto> GetMoisAsync(
            int idEcole,
            int mois,
            int annee,
            string? statut,
            CancellationToken cancellationToken = default)
        {
            if (mois < 1 || mois > 12)
                throw new InvalidOperationException("Le mois doit être entre 1 et 12.");
            if (annee < 2000 || annee > 2100)
                throw new InvalidOperationException("Année invalide.");

            var dateDebut = new DateTime(annee, mois, 1, 0, 0, 0, DateTimeKind.Utc);
            var dateFin = dateDebut.AddMonths(1).AddTicks(-1);
            var statutFilter = string.IsNullOrWhiteSpace(statut) ? DepenseStatuts.Validee : statut.Trim();

            ValidateStatutFilter(statutFilter);

            var query = BuildBaseQuery(idEcole, dateDebut, dateFin, null, statutFilter, includeInactive: false);
            var entities = await query
                .OrderByDescending(d => d.DateDepense)
                .ToListAsync(cancellationToken);

            var depenses = entities
                .Select(d => Map(d, d.CategorieDepense?.NomCategorie, FormatNomCreateur(d.UtilisateurCreateur)))
                .ToList();

            // Synthèse sur les lignes affichées ; totaux montant en devise principale si dispo
            var montantTotal = depenses.Sum(d => d.MontantDevisePrincipale ?? d.Montant);
            var synthese = new SyntheseDepenseDto
            {
                MontantTotal = montantTotal,
                NombreDepenses = depenses.Count,
                NombreValidees = depenses.Count(d => d.Statut == DepenseStatuts.Validee),
                NombreEnAttente = depenses.Count(d => d.Statut == DepenseStatuts.EnAttente),
                NombreAnnulees = depenses.Count(d => d.Statut == DepenseStatuts.Annulee)
            };

            return new DepenseMoisDto
            {
                Mois = mois,
                Annee = annee,
                DateDebut = dateDebut,
                DateFin = dateFin,
                Depenses = depenses,
                SyntheseDepense = synthese
            };
        }

        public async Task<DepenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Depenses.AsNoTracking()
                .Include(d => d.CategorieDepense)
                .Include(d => d.UtilisateurCreateur)
                .FirstOrDefaultAsync(d => d.IdDepense == id && d.Actif, cancellationToken);

            return entity == null
                ? null
                : Map(entity, entity.CategorieDepense?.NomCategorie, FormatNomCreateur(entity.UtilisateurCreateur));
        }

        public async Task<DepenseDto> CreateAsync(
            CreateDepenseDto dto,
            int? idUtilisateurCreateur,
            CancellationToken cancellationToken = default)
        {
            var ecole = await _context.Ecoles.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == dto.IdEcole && e.Statut == true, cancellationToken)
                ?? throw new InvalidOperationException($"École {dto.IdEcole} introuvable ou inactive.");

            var categorie = await _context.CategoriesDepense.AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.IdCategorieDepense == dto.IdCategorieDepense && c.IdEcole == dto.IdEcole,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Catégorie {dto.IdCategorieDepense} introuvable pour l'école {dto.IdEcole}.");

            if (!categorie.Statut)
                throw new InvalidOperationException("La catégorie de dépense est inactive.");

            var codeMontant = NormalizeDevise(dto.CodeDeviseMontant)
                ?? throw new InvalidOperationException("codeDeviseMontant est requis.");

            if (!await _currencyConversion.IsActiveDeviseAsync(dto.IdEcole, codeMontant, cancellationToken))
                throw new InvalidOperationException(
                    $"La devise {codeMontant} est absente ou inactive pour l'école {dto.IdEcole}.");

            var codePrincipale = NormalizeDevise(ecole.CodeDevisePrincipale) ?? codeMontant;

            if (!await _currencyConversion.IsActiveDeviseAsync(dto.IdEcole, codePrincipale, cancellationToken))
                throw new InvalidOperationException(
                    $"La devise principale {codePrincipale} est absente ou inactive pour l'école {dto.IdEcole}.");

            var conversion = await _currencyConversion.ConvertToPrincipalAsync(
                dto.IdEcole, codeMontant, dto.Montant, DateTime.UtcNow, cancellationToken);

            if (!conversion.Success)
            {
                throw new InvalidOperationException(
                    conversion.ErrorMessage
                    ?? $"Impossible de convertir {codeMontant} → {codePrincipale}.");
            }

            var entity = new Depense
            {
                IdEcole = dto.IdEcole,
                IdCategorieDepense = dto.IdCategorieDepense,
                Libelle = dto.Libelle.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Beneficiaire = string.IsNullOrWhiteSpace(dto.Beneficiaire) ? null : dto.Beneficiaire.Trim(),
                ReferencePiece = string.IsNullOrWhiteSpace(dto.ReferencePiece) ? null : dto.ReferencePiece.Trim(),
                Montant = dto.Montant,
                CodeDeviseMontant = codeMontant,
                CodeDevisePrincipale = conversion.CodeDevisePrincipale,
                TauxVersDevisePrincipale = conversion.Taux,
                MontantDevisePrincipale = conversion.MontantConverti,
                ModePaiement = string.IsNullOrWhiteSpace(dto.ModePaiement) ? null : dto.ModePaiement.Trim(),
                DateDepense = dto.DateDepense?.ToUniversalTime() ?? DateTime.UtcNow,
                StatutWorkflow = DepenseStatuts.Validee,
                Actif = true,
                IdUtilisateurCreateur = idUtilisateurCreateur,
                DateCreation = DateTime.UtcNow
            };

            _context.Depenses.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            string? nomCreateur = null;
            if (idUtilisateurCreateur.HasValue)
            {
                nomCreateur = await _context.Utilisateurs.AsNoTracking()
                    .Where(u => u.IdUtilisateur == idUtilisateurCreateur.Value)
                    .Select(u => u)
                    .FirstOrDefaultAsync(cancellationToken) is { } u
                    ? FormatNomCreateur(u)
                    : null;
            }

            return Map(entity, categorie.NomCategorie, nomCreateur);
        }

        public async Task<DepenseDto> UpdateAsync(
            int id,
            UpdateDepenseDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Depenses
                .Include(d => d.CategorieDepense)
                .Include(d => d.UtilisateurCreateur)
                .FirstOrDefaultAsync(d => d.IdDepense == id && d.Actif, cancellationToken)
                ?? throw new KeyNotFoundException($"Dépense {id} introuvable.");

            if (entity.StatutWorkflow != DepenseStatuts.Validee)
                throw new InvalidOperationException(
                    $"Seule une dépense Validee peut être modifiée (statut actuel : {entity.StatutWorkflow}).");

            if (dto.IdCategorieDepense.HasValue)
            {
                var cat = await _context.CategoriesDepense.AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.IdCategorieDepense == dto.IdCategorieDepense.Value
                            && c.IdEcole == entity.IdEcole
                            && c.Statut,
                        cancellationToken)
                    ?? throw new InvalidOperationException("Catégorie introuvable ou inactive pour cette école.");
                entity.IdCategorieDepense = cat.IdCategorieDepense;
                entity.CategorieDepense = null; // reload nom via query after save
            }

            if (!string.IsNullOrWhiteSpace(dto.Libelle))
                entity.Libelle = dto.Libelle.Trim();
            if (dto.Description != null)
                entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            if (dto.Beneficiaire != null)
                entity.Beneficiaire = string.IsNullOrWhiteSpace(dto.Beneficiaire) ? null : dto.Beneficiaire.Trim();
            if (dto.ReferencePiece != null)
                entity.ReferencePiece = string.IsNullOrWhiteSpace(dto.ReferencePiece) ? null : dto.ReferencePiece.Trim();
            if (dto.ModePaiement != null)
                entity.ModePaiement = string.IsNullOrWhiteSpace(dto.ModePaiement) ? null : dto.ModePaiement.Trim();
            if (dto.DateDepense.HasValue)
                entity.DateDepense = dto.DateDepense.Value.ToUniversalTime();

            await _context.SaveChangesAsync(cancellationToken);

            var nomCat = await _context.CategoriesDepense.AsNoTracking()
                .Where(c => c.IdCategorieDepense == entity.IdCategorieDepense)
                .Select(c => c.NomCategorie)
                .FirstOrDefaultAsync(cancellationToken);

            var nomCreateur = FormatNomCreateur(entity.UtilisateurCreateur);
            return Map(entity, nomCat, nomCreateur);
        }

        public async Task<DepenseDto> AnnulerAsync(
            int id,
            string? motifAnnulation,
            int? idUtilisateurAnnulation,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Depenses
                .Include(d => d.CategorieDepense)
                .Include(d => d.UtilisateurCreateur)
                .FirstOrDefaultAsync(d => d.IdDepense == id && d.Actif, cancellationToken)
                ?? throw new KeyNotFoundException($"Dépense {id} introuvable.");

            if (entity.StatutWorkflow == DepenseStatuts.Annulee)
                throw new InvalidOperationException("Cette dépense est déjà annulée.");

            if (entity.StatutWorkflow != DepenseStatuts.Validee)
                throw new InvalidOperationException(
                    $"Seule une dépense Validee peut être annulée (statut actuel : {entity.StatutWorkflow}).");

            entity.StatutWorkflow = DepenseStatuts.Annulee;
            entity.MotifAnnulation = string.IsNullOrWhiteSpace(motifAnnulation) ? null : motifAnnulation.Trim();
            entity.IdUtilisateurAnnulation = idUtilisateurAnnulation;
            entity.DateAnnulation = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Map(
                entity,
                entity.CategorieDepense?.NomCategorie,
                FormatNomCreateur(entity.UtilisateurCreateur));
        }

        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Depenses
                .FirstOrDefaultAsync(d => d.IdDepense == id && d.Actif, cancellationToken)
                ?? throw new KeyNotFoundException($"Dépense {id} introuvable.");

            entity.Actif = false;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<Depense> BuildBaseQuery(
            int? idEcole,
            DateTime? dateDebut,
            DateTime? dateFin,
            int? idCategorieDepense,
            string? statut,
            bool includeInactive)
        {
            var query = _context.Depenses.AsNoTracking()
                .Include(d => d.CategorieDepense)
                .Include(d => d.UtilisateurCreateur)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(d => d.Actif);

            if (idEcole.HasValue)
                query = query.Where(d => d.IdEcole == idEcole.Value);

            if (dateDebut.HasValue)
                query = query.Where(d => d.DateDepense >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(d => d.DateDepense <= dateFin.Value);

            if (idCategorieDepense.HasValue)
                query = query.Where(d => d.IdCategorieDepense == idCategorieDepense.Value);

            if (!string.IsNullOrWhiteSpace(statut) && !statut.Equals(DepenseStatuts.Tous, StringComparison.OrdinalIgnoreCase))
            {
                ValidateStatutFilter(statut);
                var s = statut.Trim();
                query = query.Where(d => d.StatutWorkflow == s);
            }

            return query;
        }

        private static void ValidateStatutFilter(string statut)
        {
            var s = statut.Trim();
            if (s.Equals(DepenseStatuts.Tous, StringComparison.OrdinalIgnoreCase)
                || s.Equals(DepenseStatuts.Validee, StringComparison.OrdinalIgnoreCase)
                || s.Equals(DepenseStatuts.Annulee, StringComparison.OrdinalIgnoreCase)
                || s.Equals(DepenseStatuts.EnAttente, StringComparison.OrdinalIgnoreCase))
                return;

            throw new InvalidOperationException(
                "statut invalide. Valeurs : Validee, EnAttente, Annulee, Tous.");
        }

        private static string? NormalizeDevise(string? code) =>
            string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

        private static string? FormatNomCreateur(Utilisateur? u)
        {
            if (u == null) return null;
            var nom = $"{u.PrenomUtilisateur} {u.NomUtilisateur}".Trim();
            return string.IsNullOrWhiteSpace(nom) ? u.DefaultUsername : nom;
        }

        private static DepenseDto Map(Depense d, string? nomCategorie, string? nomCreateur) => new()
        {
            IdDepense = d.IdDepense,
            IdEcole = d.IdEcole,
            IdCategorieDepense = d.IdCategorieDepense,
            NomCategorie = nomCategorie,
            Libelle = d.Libelle,
            Description = d.Description,
            Beneficiaire = d.Beneficiaire,
            ReferencePiece = d.ReferencePiece,
            Montant = d.Montant,
            CodeDeviseMontant = d.CodeDeviseMontant,
            CodeDevisePrincipale = d.CodeDevisePrincipale,
            TauxVersDevisePrincipale = d.TauxVersDevisePrincipale,
            MontantDevisePrincipale = d.MontantDevisePrincipale,
            ModePaiement = d.ModePaiement,
            DateDepense = d.DateDepense,
            Statut = d.StatutWorkflow,
            IdUtilisateurCreateur = d.IdUtilisateurCreateur,
            NomCreateur = nomCreateur,
            MotifAnnulation = d.MotifAnnulation,
            DateAnnulation = d.DateAnnulation,
            DateCreation = d.DateCreation
        };
    }
}
