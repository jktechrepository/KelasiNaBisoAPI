using System.Text.Json;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Bulletin;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KelasiNaBiso.Services
{
    public class BulletinService : IBulletinService
    {
        private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PeriodeCotationResolver _periodeResolver;

        public BulletinService(
            KelasiNaBisoDbContext context,
            IInscriptionActiveResolver inscriptionResolver,
            IServiceScopeFactory scopeFactory,
            PeriodeCotationResolver periodeResolver)
        {
            _context = context;
            _inscriptionResolver = inscriptionResolver;
            _scopeFactory = scopeFactory;
            _periodeResolver = periodeResolver;
        }

        public async Task<BulletinEleveDto?> GetBulletinEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default)
        {
            var eleveExists = await _context.Eleves.AsNoTracking()
                .AnyAsync(e => e.IdEleve == idEleve && e.Statut == true, cancellationToken);
            if (!eleveExists)
                return null;

            var inscription = await _inscriptionResolver.GetInscriptionActiveAsync(
                idEleve, idAnneeScolaire, cancellationToken);
            if (inscription == null)
                return null;

            var classBulletins = await GetBulletinsClasseAsync(
                inscription.IdClasse, idAnneeScolaire, periode, idPeriode, cancellationToken);
            return classBulletins.FirstOrDefault(b => b.IdEleve == idEleve);
        }

        public async Task<IReadOnlyList<BulletinEleveDto>> GetBulletinsClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default)
        {
            var filter = await ResolvePeriodeFilterAsync(periode, idPeriode, cancellationToken);
            var bulletins = (await ComputeLiveBulletinsClasseAsync(
                idClasse, idAnneeScolaire, periode, idPeriode, cancellationToken)).ToList();
            await ApplySnapshotsAsync(bulletins, idAnneeScolaire, filter.IdPeriode, cancellationToken);
            return bulletins;
        }

        public async Task<BulletinEleveDto> FigerEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            int? idAuteurValidation = null,
            CancellationToken cancellationToken = default)
        {
            var filter = await ResolvePeriodeFilterAsync(periode, idPeriode, cancellationToken);
            if (!filter.IdPeriode.HasValue)
                throw new InvalidOperationException(
                    "Impossible de figer sans période résolue. Utilisez idPeriode (GET /api/PeriodeCotation).");

            var resolvedIdPeriode = filter.IdPeriode.Value;
            if (await EstFigeAsync(idEleve, idAnneeScolaire, resolvedIdPeriode, cancellationToken))
                throw new InvalidOperationException(
                    $"Le bulletin de l'élève {idEleve} est déjà figé pour cette période.");

            var live = await ComputeLiveBulletinEleveAsync(
                idEleve, idAnneeScolaire, periode, idPeriode, cancellationToken);
            if (live == null)
                throw new InvalidOperationException(
                    $"Bulletin introuvable pour l'élève {idEleve} (inscription ou période).");

            return await PersistSnapshotAsync(live, resolvedIdPeriode, idAuteurValidation, cancellationToken);
        }

        public async Task<FigerBulletinResultDto> FigerClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            int? idAuteurValidation = null,
            CancellationToken cancellationToken = default)
        {
            var filter = await ResolvePeriodeFilterAsync(periode, idPeriode, cancellationToken);
            if (!filter.IdPeriode.HasValue)
                throw new InvalidOperationException(
                    "Impossible de figer sans période résolue. Utilisez idPeriode (GET /api/PeriodeCotation).");

            var resolvedIdPeriode = filter.IdPeriode.Value;
            var liveList = await ComputeLiveBulletinsClasseAsync(
                idClasse, idAnneeScolaire, periode, idPeriode, cancellationToken);

            var eleveIds = liveList.Select(b => b.IdEleve).ToList();
            var existingIds = await _context.BulletinsFiges.AsNoTracking()
                .Where(f => f.IdAnneeScolaire == idAnneeScolaire
                    && f.IdPeriode == resolvedIdPeriode
                    && eleveIds.Contains(f.IdEleve))
                .Select(f => f.IdEleve)
                .ToListAsync(cancellationToken);
            var already = new HashSet<int>(existingIds);

            var figes = new List<BulletinEleveDto>();
            var dejaFiges = 0;
            foreach (var live in liveList)
            {
                if (already.Contains(live.IdEleve))
                {
                    dejaFiges++;
                    continue;
                }

                figes.Add(await PersistSnapshotAsync(live, resolvedIdPeriode, idAuteurValidation, cancellationToken));
            }

            return new FigerBulletinResultDto
            {
                Figes = figes.Count,
                DejaFiges = dejaFiges,
                Bulletins = figes
            };
        }

        public async Task DeverrouillerEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default)
        {
            var filter = await ResolvePeriodeFilterAsync(periode, idPeriode, cancellationToken);
            if (!filter.IdPeriode.HasValue)
                throw new InvalidOperationException(
                    "Impossible de déverrouiller sans période résolue. Utilisez idPeriode (GET /api/PeriodeCotation).");

            var snapshot = await _context.BulletinsFiges
                .FirstOrDefaultAsync(
                    f => f.IdEleve == idEleve
                        && f.IdAnneeScolaire == idAnneeScolaire
                        && f.IdPeriode == filter.IdPeriode.Value,
                    cancellationToken);

            if (snapshot == null)
                throw new InvalidOperationException(
                    $"Aucun bulletin figé pour l'élève {idEleve} sur cette période.");

            _context.BulletinsFiges.Remove(snapshot);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> EstFigeAsync(
            int idEleve,
            int idAnneeScolaire,
            int idPeriode,
            CancellationToken cancellationToken = default)
            => _context.BulletinsFiges.AsNoTracking()
                .AnyAsync(
                    f => f.IdEleve == idEleve
                        && f.IdAnneeScolaire == idAnneeScolaire
                        && f.IdPeriode == idPeriode,
                    cancellationToken);

        public async Task<BulletinDecisionDto> UpsertDecisionAsync(
            int idEleve,
            int idAnneeScolaire,
            int idPeriode,
            string? decision,
            string? appreciationGenerale,
            int? idAuteur,
            CancellationToken cancellationToken = default)
        {
            var eleveOk = await _context.Eleves.AsNoTracking()
                .AnyAsync(e => e.IdEleve == idEleve && e.Statut == true, cancellationToken);
            if (!eleveOk)
                throw new InvalidOperationException($"Élève {idEleve} introuvable.");

            var anneeOk = await _context.AnneeScolaires.AsNoTracking()
                .AnyAsync(a => a.IdAnneeScolaire == idAnneeScolaire && a.Statut == true, cancellationToken);
            if (!anneeOk)
                throw new InvalidOperationException($"Année scolaire {idAnneeScolaire} introuvable.");

            var periodeOk = await _context.PeriodesCotation.AsNoTracking()
                .AnyAsync(p => p.IdPeriode == idPeriode && p.Statut, cancellationToken);
            if (!periodeOk)
                throw new InvalidOperationException($"Période {idPeriode} introuvable ou inactive.");

            if (await EstFigeAsync(idEleve, idAnneeScolaire, idPeriode, cancellationToken))
                throw new InvalidOperationException(
                    "Impossible de modifier la décision : le bulletin est figé. Déverrouillez-le d'abord (direction).");

            var existing = await _context.BulletinDecisions
                .FirstOrDefaultAsync(
                    d => d.IdEleve == idEleve
                        && d.IdAnneeScolaire == idAnneeScolaire
                        && d.IdPeriode == idPeriode,
                    cancellationToken);

            var now = DateTime.UtcNow;
            if (existing == null)
            {
                existing = new BulletinDecision
                {
                    IdEleve = idEleve,
                    IdAnneeScolaire = idAnneeScolaire,
                    IdPeriode = idPeriode,
                    Decision = string.IsNullOrWhiteSpace(decision) ? null : decision.Trim(),
                    AppreciationGenerale = string.IsNullOrWhiteSpace(appreciationGenerale)
                        ? null
                        : appreciationGenerale.Trim(),
                    IdAuteur = idAuteur,
                    DateCreation = now,
                    DateModification = now
                };
                _context.BulletinDecisions.Add(existing);
            }
            else
            {
                existing.Decision = string.IsNullOrWhiteSpace(decision) ? null : decision.Trim();
                existing.AppreciationGenerale = string.IsNullOrWhiteSpace(appreciationGenerale)
                    ? null
                    : appreciationGenerale.Trim();
                existing.IdAuteur = idAuteur;
                existing.DateModification = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new BulletinDecisionDto
            {
                IdBulletinDecision = existing.IdBulletinDecision,
                IdEleve = existing.IdEleve,
                IdAnneeScolaire = existing.IdAnneeScolaire,
                IdPeriode = existing.IdPeriode,
                Decision = existing.Decision,
                AppreciationGenerale = existing.AppreciationGenerale,
                IdAuteur = existing.IdAuteur,
                DateModification = existing.DateModification
            };
        }

        private async Task ApplySnapshotsAsync(
            List<BulletinEleveDto> bulletins,
            int idAnneeScolaire,
            int? idPeriode,
            CancellationToken cancellationToken)
        {
            foreach (var b in bulletins)
            {
                b.EstFige = false;
                b.DateValidation = null;
            }

            if (!idPeriode.HasValue || bulletins.Count == 0)
                return;

            var eleveIds = bulletins.Select(b => b.IdEleve).ToList();
            var snapshots = await _context.BulletinsFiges.AsNoTracking()
                .Where(f => f.IdAnneeScolaire == idAnneeScolaire
                    && f.IdPeriode == idPeriode.Value
                    && eleveIds.Contains(f.IdEleve))
                .ToListAsync(cancellationToken);

            if (snapshots.Count == 0)
                return;

            var byEleve = snapshots.ToDictionary(f => f.IdEleve);
            for (var i = 0; i < bulletins.Count; i++)
            {
                if (!byEleve.TryGetValue(bulletins[i].IdEleve, out var snap))
                    continue;

                var dto = DeserializeSnapshot(snap);
                if (dto != null)
                    bulletins[i] = dto;
            }
        }

        private async Task<BulletinEleveDto?> ComputeLiveBulletinEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode,
            int? idPeriode,
            CancellationToken cancellationToken)
        {
            var eleveExists = await _context.Eleves.AsNoTracking()
                .AnyAsync(e => e.IdEleve == idEleve && e.Statut == true, cancellationToken);
            if (!eleveExists)
                return null;

            var inscription = await _inscriptionResolver.GetInscriptionActiveAsync(
                idEleve, idAnneeScolaire, cancellationToken);
            if (inscription == null)
                return null;

            var classBulletins = await ComputeLiveBulletinsClasseAsync(
                inscription.IdClasse, idAnneeScolaire, periode, idPeriode, cancellationToken);
            return classBulletins.FirstOrDefault(b => b.IdEleve == idEleve);
        }

        /// <summary>Calcul live sans overlay snapshot (utilisé pour figer).</summary>
        private async Task<IReadOnlyList<BulletinEleveDto>> ComputeLiveBulletinsClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string? periode,
            int? idPeriode,
            CancellationToken cancellationToken)
        {
            var filter = await ResolvePeriodeFilterAsync(periode, idPeriode, cancellationToken);

            var classe = await _context.Classes.AsNoTracking()
                .Include(c => c.Direction)
                .ThenInclude(d => d!.Ecole)
                .FirstOrDefaultAsync(c => c.IdClasse == idClasse && c.Statut == true, cancellationToken);
            if (classe == null)
                return Array.Empty<BulletinEleveDto>();

            var idEcole = classe.Direction?.IdEcole ?? 0;
            var nomEcole = classe.Direction?.Ecole?.Nom ?? string.Empty;

            var annee = await _context.AnneeScolaires.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == idAnneeScolaire, cancellationToken);

            var eleves = await _inscriptionResolver
                .FilterElevesInClasse(_context.Eleves.AsNoTracking(), idClasse, idAnneeScolaire)
                .OrderBy(e => e.NomComplet)
                .ToListAsync(cancellationToken);

            var eleveIds = eleves.Select(e => e.IdEleve).ToList();
            var notesQuery = _context.Notes.AsNoTracking()
                .Include(n => n.Evaluation)
                .ThenInclude(ev => ev!.Course)
                .Where(n => n.Statut == true
                    && n.IdAnneeScolaire == idAnneeScolaire
                    && eleveIds.Contains(n.IdEleve)
                    && n.Evaluation != null
                    && n.Evaluation.Statut == true);

            notesQuery = ApplyPeriodeFilter(notesQuery, filter);

            var allNotes = await notesQuery.ToListAsync(cancellationToken);

            var notesByEleve = allNotes.GroupBy(n => n.IdEleve)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bulletins = new List<BulletinEleveDto>();
            foreach (var eleve in eleves)
            {
                notesByEleve.TryGetValue(eleve.IdEleve, out var notes);
                notes ??= new List<Note>();

                bulletins.Add(BuildBulletinDto(
                    eleve,
                    idClasse,
                    classe.NomClasse ?? string.Empty,
                    idEcole,
                    nomEcole,
                    idAnneeScolaire,
                    annee?.LibelleAnneeScolaire ?? string.Empty,
                    filter.DisplayLabel,
                    notes));
            }

            AssignCompetitionRanks(bulletins);
            foreach (var b in bulletins)
            {
                b.EffectifClasse = bulletins.Count;
                b.EstFige = false;
                b.DateValidation = null;
            }

            await ApplyDecisionsAsync(bulletins, idAnneeScolaire, filter.IdPeriode, cancellationToken);
            return bulletins;
        }

        private async Task<BulletinEleveDto> PersistSnapshotAsync(
            BulletinEleveDto live,
            int idPeriode,
            int? idAuteurValidation,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            live.EstFige = true;
            live.DateValidation = now;

            var entity = new BulletinFige
            {
                IdEleve = live.IdEleve,
                IdAnneeScolaire = live.IdAnneeScolaire,
                IdPeriode = idPeriode,
                PayloadJson = JsonSerializer.Serialize(live, SnapshotJsonOptions),
                MoyenneGenerale = live.MoyenneGenerale,
                Rang = live.Rang,
                EffectifClasse = live.EffectifClasse,
                Decision = live.Decision,
                AppreciationGenerale = live.AppreciationGenerale,
                IdAuteurValidation = idAuteurValidation,
                DateValidation = now
            };

            _context.BulletinsFiges.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return live;
        }

        private static BulletinEleveDto? DeserializeSnapshot(BulletinFige snap)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<BulletinEleveDto>(snap.PayloadJson, SnapshotJsonOptions);
                if (dto == null)
                    return null;

                dto.EstFige = true;
                dto.DateValidation = snap.DateValidation;
                // Métadonnées dénormalisées au cas où le JSON serait incomplet
                dto.MoyenneGenerale ??= snap.MoyenneGenerale;
                dto.Rang ??= snap.Rang;
                if (dto.EffectifClasse == 0 && snap.EffectifClasse > 0)
                    dto.EffectifClasse = snap.EffectifClasse;
                dto.Decision ??= snap.Decision;
                dto.AppreciationGenerale ??= snap.AppreciationGenerale;
                return dto;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private async Task ApplyDecisionsAsync(
            List<BulletinEleveDto> bulletins,
            int idAnneeScolaire,
            int? idPeriode,
            CancellationToken cancellationToken)
        {
            if (!idPeriode.HasValue || bulletins.Count == 0)
                return;

            var eleveIds = bulletins.Select(b => b.IdEleve).ToList();
            var decisions = await _context.BulletinDecisions.AsNoTracking()
                .Where(d => d.IdAnneeScolaire == idAnneeScolaire
                    && d.IdPeriode == idPeriode.Value
                    && eleveIds.Contains(d.IdEleve))
                .ToListAsync(cancellationToken);

            if (decisions.Count == 0)
                return;

            var byEleve = decisions.ToDictionary(d => d.IdEleve);
            foreach (var b in bulletins)
            {
                if (!byEleve.TryGetValue(b.IdEleve, out var d))
                    continue;
                b.Decision = d.Decision;
                b.AppreciationGenerale = d.AppreciationGenerale;
            }
        }

        public async Task<byte[]> GenerateBulletinPdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IBulletinReportService>();
            return await reportService.GetElevePdfAsync(idEleve, idAnneeScolaire, periode, idPeriode, cancellationToken);
        }

        private async Task<PeriodeFilter> ResolvePeriodeFilterAsync(
            string? periode,
            int? idPeriode,
            CancellationToken cancellationToken)
        {
            if (!idPeriode.HasValue && string.IsNullOrWhiteSpace(periode))
                throw new InvalidOperationException("Le paramètre idPeriode ou periode est requis.");

            var resolved = await _periodeResolver.ResolveAsync(idPeriode, periode, cancellationToken);
            if (resolved != null)
            {
                var aliases = PeriodeCotationAliases.GetAliases(resolved.Code).ToList();
                if (!aliases.Contains(resolved.Libelle, StringComparer.OrdinalIgnoreCase))
                    aliases.Add(resolved.Libelle);
                if (!aliases.Contains(resolved.Code, StringComparer.OrdinalIgnoreCase))
                    aliases.Add(resolved.Code);

                return new PeriodeFilter(resolved.IdPeriode, aliases, resolved.Libelle);
            }

            if (idPeriode.HasValue && idPeriode.Value > 0)
                throw new InvalidOperationException($"Période de cotation {idPeriode.Value} introuvable ou inactive.");

            var periodeNorm = periode!.Trim();
            return new PeriodeFilter(null, new List<string> { periodeNorm }, periodeNorm);
        }

        private static IQueryable<Note> ApplyPeriodeFilter(IQueryable<Note> query, PeriodeFilter filter)
        {
            if (filter.IdPeriode.HasValue)
            {
                var id = filter.IdPeriode.Value;
                var aliases = filter.Aliases;
                return query.Where(n =>
                    n.Evaluation!.IdPeriode == id
                    || (n.Evaluation.IdPeriode == null
                        && n.Evaluation.Periode != null
                        && aliases.Contains(n.Evaluation.Periode)));
            }

            var exact = filter.Aliases[0];
            return query.Where(n => n.Evaluation!.Periode == exact);
        }

        private static BulletinEleveDto BuildBulletinDto(
            Eleve eleve,
            int idClasse,
            string nomClasse,
            int idEcole,
            string nomEcole,
            int idAnneeScolaire,
            string libelleAnnee,
            string periode,
            List<Note> notes)
        {
            var lignes = notes
                .Where(n => n.Evaluation != null)
                .GroupBy(n => n.Evaluation!.IdCours)
                .Select(g =>
                {
                    var details = g.Select(n =>
                    {
                        var coeff = ResolveCoeff(n.Evaluation!.Coefficient);
                        return new BulletinNoteDetailDto
                        {
                            IdNote = n.IdNote,
                            IdEvaluation = n.IdEvaluation,
                            TitreEvaluation = n.Evaluation.TitreEvaluation,
                            TypeEvaluation = n.Evaluation.TypeEvaluation,
                            NoteObtenue = n.NoteObtenue,
                            CoefficientEvaluation = coeff,
                            Appreciation = n.Appreciation
                        };
                    }).ToList();

                    var sumCoeff = details.Sum(d => d.CoefficientEvaluation);
                    double? moyenneCours = sumCoeff > 0
                        ? details.Sum(d => d.NoteObtenue * d.CoefficientEvaluation) / sumCoeff
                        : null;

                    var first = g.First();
                    var ponderationCours = ResolvePonderationCours(first.Evaluation?.Course?.Ponderation);
                    return new BulletinLigneCoursDto
                    {
                        IdCours = g.Key,
                        NomCours = first.Evaluation?.Course?.NomCours ?? $"Cours {g.Key}",
                        Coefficient = sumCoeff,
                        PonderationCours = ponderationCours,
                        Notes = details,
                        MoyenneCours = moyenneCours
                    };
                })
                .OrderBy(l => l.NomCours)
                .ToList();

            double? moyenneGenerale = null;
            var lignesAvecMoyenne = lignes.Where(l => l.MoyenneCours.HasValue && l.PonderationCours > 0).ToList();
            if (lignesAvecMoyenne.Count > 0)
            {
                var totalPonderation = lignesAvecMoyenne.Sum(l => l.PonderationCours);
                if (totalPonderation > 0)
                {
                    moyenneGenerale = lignesAvecMoyenne.Sum(l => l.MoyenneCours!.Value * l.PonderationCours) / totalPonderation;
                }
            }

            return new BulletinEleveDto
            {
                IdEleve = eleve.IdEleve,
                NomCompletEleve = eleve.NomComplet ?? string.Empty,
                IdClasse = idClasse,
                NomClasse = nomClasse,
                IdEcole = idEcole,
                NomEcole = nomEcole,
                IdAnneeScolaire = idAnneeScolaire,
                LibelleAnneeScolaire = libelleAnnee,
                Periode = periode,
                Lignes = lignes,
                MoyenneGenerale = moyenneGenerale,
                Decision = null,
                AppreciationGenerale = null
            };
        }

        private static double ResolveCoeff(double? coefficient)
            => coefficient.HasValue && coefficient.Value > 0 ? coefficient.Value : 1d;

        private static double ResolvePonderationCours(int? ponderation)
            => ponderation.HasValue && ponderation.Value > 0 ? ponderation.Value : 1d;

        /// <summary>Competition ranking : 1, 2, 2, 4…</summary>
        private static void AssignCompetitionRanks(List<BulletinEleveDto> bulletins)
        {
            var ordered = bulletins
                .OrderByDescending(b => b.MoyenneGenerale.HasValue)
                .ThenByDescending(b => b.MoyenneGenerale ?? double.MinValue)
                .ThenBy(b => b.NomCompletEleve)
                .ToList();

            var i = 0;
            while (i < ordered.Count)
            {
                var start = i;
                var moy = ordered[i].MoyenneGenerale;
                while (i + 1 < ordered.Count
                    && Nullable.Equals(ordered[i + 1].MoyenneGenerale, moy))
                {
                    i++;
                }

                var rank = start + 1;
                for (var j = start; j <= i; j++)
                    ordered[j].Rang = moy.HasValue ? rank : null;

                i++;
            }
        }

        private sealed record PeriodeFilter(int? IdPeriode, List<string> Aliases, string DisplayLabel);
    }
}
