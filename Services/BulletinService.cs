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
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly IServiceScopeFactory _scopeFactory;

        public BulletinService(
            KelasiNaBisoDbContext context,
            IInscriptionActiveResolver inscriptionResolver,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _inscriptionResolver = inscriptionResolver;
            _scopeFactory = scopeFactory;
        }

        public async Task<BulletinEleveDto?> GetBulletinEleveAsync(
            int idEleve,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(periode))
                throw new InvalidOperationException("Le paramètre periode est requis.");

            var eleveExists = await _context.Eleves.AsNoTracking()
                .AnyAsync(e => e.IdEleve == idEleve && e.Statut == true, cancellationToken);
            if (!eleveExists)
                return null;

            var inscription = await _inscriptionResolver.GetInscriptionActiveAsync(
                idEleve, idAnneeScolaire, cancellationToken);
            if (inscription == null)
                return null;

            var classBulletins = await GetBulletinsClasseAsync(
                inscription.IdClasse, idAnneeScolaire, periode, cancellationToken);
            return classBulletins.FirstOrDefault(b => b.IdEleve == idEleve);
        }

        public async Task<IReadOnlyList<BulletinEleveDto>> GetBulletinsClasseAsync(
            int idClasse,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(periode))
                throw new InvalidOperationException("Le paramètre periode est requis.");

            var periodeNorm = periode.Trim();

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
            var allNotes = await _context.Notes.AsNoTracking()
                .Include(n => n.Evaluation)
                .ThenInclude(ev => ev.Course)
                .Where(n => n.Statut == true
                    && n.IdAnneeScolaire == idAnneeScolaire
                    && eleveIds.Contains(n.IdEleve)
                    && n.Evaluation != null
                    && n.Evaluation.Statut == true
                    && n.Evaluation.Periode == periodeNorm)
                .ToListAsync(cancellationToken);

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
                    periodeNorm,
                    notes));
            }

            AssignCompetitionRanks(bulletins);
            foreach (var b in bulletins)
                b.EffectifClasse = bulletins.Count;

            return bulletins;
        }

        public async Task<byte[]> GenerateBulletinPdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IBulletinReportService>();
            return await reportService.GetElevePdfAsync(idEleve, idAnneeScolaire, periode, cancellationToken);
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
                    return new BulletinLigneCoursDto
                    {
                        IdCours = g.Key,
                        NomCours = first.Evaluation?.Course?.NomCours ?? $"Cours {g.Key}",
                        Coefficient = sumCoeff,
                        Notes = details,
                        MoyenneCours = moyenneCours
                    };
                })
                .OrderBy(l => l.NomCours)
                .ToList();

            double? moyenneGenerale = null;
            var lignesAvecMoyenne = lignes.Where(l => l.MoyenneCours.HasValue && l.Coefficient > 0).ToList();
            if (lignesAvecMoyenne.Count > 0)
            {
                var totalCoeff = lignesAvecMoyenne.Sum(l => l.Coefficient);
                if (totalCoeff > 0)
                {
                    moyenneGenerale = lignesAvecMoyenne.Sum(l => l.MoyenneCours!.Value * l.Coefficient) / totalCoeff;
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
    }
}
