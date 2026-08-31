using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IInscriptionActiveResolver
    {
        /// <summary>Inscription confirmée active pour l'élève (année optionnelle).</summary>
        Task<Inscription?> GetInscriptionActiveAsync(int idEleve, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        /// <summary>Classe courante via inscription active uniquement.</summary>
        Task<int?> GetClasseCouranteAsync(int idEleve, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        /// <summary>École courante via inscription active.</summary>
        Task<int?> GetEcoleCouranteAsync(int idEleve, int? idAnneeScolaire = null, CancellationToken cancellationToken = default);

        IQueryable<Eleve> FilterElevesInClasse(IQueryable<Eleve> query, int idClasse, int? idAnneeScolaire = null);

        IQueryable<Eleve> FilterElevesInDirection(IQueryable<Eleve> query, int idDirection, int? idAnneeScolaire = null);

        IQueryable<Eleve> FilterElevesInEcole(IQueryable<Eleve> query, int idEcole, int? idAnneeScolaire = null);

        IQueryable<Tuteur> FilterTuteursInEcole(IQueryable<Tuteur> query, int idEcole, int? idAnneeScolaire = null);

        IQueryable<Utilisateur> FilterUtilisateursParentsInEcole(IQueryable<Utilisateur> query, int idEcole);

        IQueryable<Utilisateur> FilterUtilisateursParentsInClasse(IQueryable<Utilisateur> query, int idClasse);

        Task<bool> IsTuteurInEcoleAsync(int idTuteur, int idEcole, CancellationToken cancellationToken = default);
    }
}
