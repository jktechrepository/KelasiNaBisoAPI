using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class PalmaresEcoleService : IPalmaresEcoleService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly PeriodeCotationResolver _periodeResolver;

        public PalmaresEcoleService(
            KelasiNaBisoDbContext context,
            EleveAnneeScopeHelper scope,
            PeriodeCotationResolver periodeResolver)
        {
            _context = context;
            _scope = scope;
            _periodeResolver = periodeResolver;
        }

        public async Task<PalmaresEcoleDto> GetPalmaresEcoleAsync(
            int idEcole,
            int? idAnneeScolaire = null,
            int? idPeriode = null,
            string? periode = null,
            int? idClasse = null,
            int? idDirection = null,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            const int maxLimit = 100;
            if (limit < 1) limit = 20;
            if (limit > maxLimit) limit = maxLimit;

            if (idEcole <= 0)
                throw new InvalidOperationException("idEcole est obligatoire.");

            var ecole = await _context.Ecoles.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole, cancellationToken)
                ?? throw new InvalidOperationException($"École {idEcole} introuvable.");

            var resolvedAnneeId = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            var annee = await _context.AnneeScolaires.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == resolvedAnneeId, cancellationToken);

            // idClasse prioritaire sur idDirection
            int? filterClasse = idClasse is > 0 ? idClasse : null;
            int? filterDirection = filterClasse.HasValue ? null : (idDirection is > 0 ? idDirection : null);

            string? nomClasseFilter = null;
            string? nomDirectionFilter = null;

            if (filterClasse.HasValue)
            {
                var classe = await _context.Classes.AsNoTracking()
                    .Include(c => c.Direction)
                    .FirstOrDefaultAsync(c => c.IdClasse == filterClasse.Value, cancellationToken);
                if (classe == null)
                    throw new InvalidOperationException($"Classe {filterClasse.Value} introuvable.");
                if (classe.Direction?.IdEcole != idEcole)
                    throw new InvalidOperationException($"La classe {filterClasse.Value} n'appartient pas à l'école {idEcole}.");
                nomClasseFilter = classe.NomClasse;
            }
            else if (filterDirection.HasValue)
            {
                var direction = await _context.Directions.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.IdDirection == filterDirection.Value, cancellationToken);
                if (direction == null)
                    throw new InvalidOperationException($"Direction {filterDirection.Value} introuvable.");
                if (direction.IdEcole != idEcole)
                    throw new InvalidOperationException($"La direction {filterDirection.Value} n'appartient pas à l'école {idEcole}.");
                nomDirectionFilter = direction.NomDirection;
            }

            var inscriptionsQuery = _context.Inscriptions.AsNoTracking()
                .Include(i => i.Classe)
                .Include(i => i.Eleve)
                .Where(i => i.IdEcole == idEcole
                    && i.IdAnneeScolaire == resolvedAnneeId
                    && i.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")));

            if (filterClasse.HasValue)
                inscriptionsQuery = inscriptionsQuery.Where(i => i.IdClasse == filterClasse.Value);
            else if (filterDirection.HasValue)
                inscriptionsQuery = inscriptionsQuery.Where(i =>
                    i.Classe != null && i.Classe.IdDirection == filterDirection.Value);

            var inscriptions = await inscriptionsQuery.ToListAsync(cancellationToken);

            // Une inscription par élève (la plus récente)
            var inscriptionByEleve = inscriptions
                .GroupBy(i => i.IdEleve)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(i => i.DateInscription).First());

            var eleveIds = inscriptionByEleve.Keys.ToList();

            PeriodeCotation? periodeResolue = null;
            if ((idPeriode.HasValue && idPeriode.Value > 0) || !string.IsNullOrWhiteSpace(periode))
            {
                periodeResolue = await _periodeResolver.ResolveAsync(idPeriode, periode, cancellationToken);
                if (periodeResolue == null)
                    throw new InvalidOperationException("Période de cotation introuvable ou inactive.");
            }
            else if (eleveIds.Count > 0)
            {
                // P1 : dernière période ayant au moins un bulletin figé pour ce périmètre
                var latest = await (
                    from b in _context.BulletinsFiges.AsNoTracking()
                    join p in _context.PeriodesCotation.AsNoTracking() on b.IdPeriode equals p.IdPeriode
                    where b.IdAnneeScolaire == resolvedAnneeId
                        && eleveIds.Contains(b.IdEleve)
                        && p.Statut
                    orderby p.Ordre descending
                    select p
                ).FirstOrDefaultAsync(cancellationToken);

                periodeResolue = latest;
            }

            var dto = new PalmaresEcoleDto
            {
                IdEcole = idEcole,
                NomEcole = ecole.Nom,
                IdAnneeScolaire = resolvedAnneeId,
                LibelleAnneeScolaire = annee?.LibelleAnneeScolaire,
                IdPeriode = periodeResolue?.IdPeriode,
                CodePeriode = periodeResolue?.Code,
                LibellePeriode = periodeResolue?.Libelle,
                IdClasse = filterClasse,
                NomClasse = nomClasseFilter,
                IdDirection = filterDirection,
                NomDirection = nomDirectionFilter,
                Limit = limit
            };

            if (periodeResolue == null || eleveIds.Count == 0)
            {
                dto.EffectifPrisEnCompte = 0;
                dto.Lignes = Array.Empty<PalmaresEcoleLigneDto>();
                return dto;
            }

            var figes = await _context.BulletinsFiges.AsNoTracking()
                .Where(b => b.IdAnneeScolaire == resolvedAnneeId
                    && b.IdPeriode == periodeResolue.IdPeriode
                    && eleveIds.Contains(b.IdEleve))
                .ToListAsync(cancellationToken);

            // Un figé par élève (le plus récent si doublons)
            var figeByEleve = figes
                .GroupBy(b => b.IdEleve)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(b => b.DateValidation).First());

            var candidats = new List<PalmaresEcoleLigneDto>(figeByEleve.Count);
            foreach (var (idEleve, fige) in figeByEleve)
            {
                if (!inscriptionByEleve.TryGetValue(idEleve, out var ins))
                    continue;

                var eleve = ins.Eleve;
                candidats.Add(new PalmaresEcoleLigneDto
                {
                    IdEleve = idEleve,
                    Matricule = eleve?.Matricule,
                    Nom = eleve?.Nom,
                    Postnom = eleve?.Postnom,
                    Prenom = eleve?.Prenom,
                    NomComplet = eleve?.NomComplet,
                    IdClasse = ins.IdClasse,
                    NomClasse = ins.Classe?.NomClasse,
                    MoyenneGenerale = fige.MoyenneGenerale,
                    Decision = fige.Decision,
                    AppreciationGenerale = fige.AppreciationGenerale
                });
            }

            AssignCompetitionRanks(candidats);
            dto.EffectifPrisEnCompte = candidats.Count;
            dto.Lignes = candidats
                .OrderBy(l => l.Rang ?? int.MaxValue)
                .ThenByDescending(l => l.MoyenneGenerale ?? double.MinValue)
                .ThenBy(l => l.NomComplet)
                .Take(limit)
                .ToList();

            return dto;
        }

        /// <summary>Competition ranking : 1, 2, 2, 4… (même logique que BulletinService).</summary>
        public static void AssignCompetitionRanks(List<PalmaresEcoleLigneDto> lignes)
        {
            var ordered = lignes
                .OrderByDescending(l => l.MoyenneGenerale.HasValue)
                .ThenByDescending(l => l.MoyenneGenerale ?? double.MinValue)
                .ThenBy(l => l.NomComplet)
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
    }
}
