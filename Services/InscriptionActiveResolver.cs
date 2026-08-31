using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class InscriptionActiveResolver : IInscriptionActiveResolver
    {
        private readonly KelasiNaBisoDbContext _context;

        public InscriptionActiveResolver(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<Inscription?> GetInscriptionActiveAsync(
            int idEleve,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.AnneeScolaire)
                .Include(i => i.Classe)
                .Where(i => i.IdEleve == idEleve && i.Statut == true);

            if (idAnneeScolaire.HasValue)
                query = query.Where(i => i.IdAnneeScolaire == idAnneeScolaire.Value);

            var candidates = await query
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync(cancellationToken);

            var active = candidates.Where(InscriptionActiveRules.IsActiveConfirmed).ToList();
            if (active.Count == 0)
                return null;

            if (idAnneeScolaire.HasValue)
                return active.First();

            return active
                .OrderByDescending(i => i.AnneeScolaire?.DateDebut ?? DateTime.MinValue)
                .ThenByDescending(i => i.DateInscription)
                .First();
        }

        public async Task<int?> GetClasseCouranteAsync(
            int idEleve,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var inscription = await GetInscriptionActiveAsync(idEleve, idAnneeScolaire, cancellationToken);
            return inscription?.IdClasse;
        }

        public async Task<int?> GetEcoleCouranteAsync(
            int idEleve,
            int? idAnneeScolaire = null,
            CancellationToken cancellationToken = default)
        {
            var inscription = await GetInscriptionActiveAsync(idEleve, idAnneeScolaire, cancellationToken);
            return inscription?.IdEcole;
        }

        public IQueryable<Eleve> FilterElevesInClasse(IQueryable<Eleve> query, int idClasse, int? idAnneeScolaire = null)
        {
            return query.Where(e =>
                e.Statut == true
                && e.Inscriptions.Any(i =>
                    i.Statut == true
                    && i.IdClasse == idClasse
                    && (!idAnneeScolaire.HasValue || i.IdAnneeScolaire == idAnneeScolaire.Value)
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm"))));
        }

        public IQueryable<Eleve> FilterElevesInDirection(IQueryable<Eleve> query, int idDirection, int? idAnneeScolaire = null)
        {
            var classeIdsInDirection = _context.Classes
                .Where(c => c.IdDirection == idDirection)
                .Select(c => c.IdClasse);

            return query.Where(e =>
                e.Statut == true
                && e.Inscriptions.Any(i =>
                    i.Statut == true
                    && classeIdsInDirection.Contains(i.IdClasse)
                    && (!idAnneeScolaire.HasValue || i.IdAnneeScolaire == idAnneeScolaire.Value)
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm"))));
        }

        public IQueryable<Eleve> FilterElevesInEcole(IQueryable<Eleve> query, int idEcole, int? idAnneeScolaire = null)
        {
            return query.Where(e =>
                e.Statut == true
                && e.Inscriptions.Any(i =>
                    i.Statut == true
                    && i.IdEcole == idEcole
                    && (!idAnneeScolaire.HasValue || i.IdAnneeScolaire == idAnneeScolaire.Value)
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm"))));
        }

        public IQueryable<Tuteur> FilterTuteursInEcole(IQueryable<Tuteur> query, int idEcole, int? idAnneeScolaire = null)
        {
            return query.Where(t =>
                t.Statut == true
                && t.Eleves.Any(e =>
                    e.Statut == true
                    && e.Inscriptions.Any(i =>
                        i.Statut == true
                        && i.IdEcole == idEcole
                        && (!idAnneeScolaire.HasValue || i.IdAnneeScolaire == idAnneeScolaire.Value)
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))));
        }

        public IQueryable<Utilisateur> FilterUtilisateursParentsInEcole(IQueryable<Utilisateur> query, int idEcole)
        {
            return query.Where(u =>
                u.IdTuteur != null
                && u.Statut == true
                && u.Tuteur != null
                && u.Tuteur.Eleves.Any(e =>
                    e.Statut == true
                    && e.Inscriptions.Any(i =>
                        i.Statut == true
                        && i.IdEcole == idEcole
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))));
        }

        public IQueryable<Utilisateur> FilterUtilisateursParentsInClasse(IQueryable<Utilisateur> query, int idClasse)
        {
            return query.Where(u =>
                u.IdTuteur != null
                && u.Statut == true
                && u.Tuteur != null
                && u.Tuteur.Eleves.Any(e =>
                    e.Statut == true
                    && e.Inscriptions.Any(i =>
                        i.Statut == true
                        && i.IdClasse == idClasse
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))));
        }

        public async Task<bool> IsTuteurInEcoleAsync(
            int idTuteur,
            int idEcole,
            CancellationToken cancellationToken = default)
        {
            return await FilterTuteursInEcole(_context.Tuteurs, idEcole)
                .AnyAsync(t => t.IdTuteur == idTuteur, cancellationToken);
        }
    }
}
