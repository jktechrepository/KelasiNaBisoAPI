using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IV_EleveRepository
    {
        Task<IEnumerable<V_Eleve>> GetAllAsync();
        Task<V_Eleve> GetByIdAsync(int id);
        Task<V_Eleve> GetByReferenceAsync(Guid reference);
        Task<V_Eleve> GetByMatriculeAsync(string matricule);
        Task<IEnumerable<V_Eleve>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<V_Eleve>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<V_Eleve>> GetByTuteurAsync(int idTuteur);
        Task<IEnumerable<V_Eleve>> GetByStatutAsync(bool statut);
        Task<IEnumerable<V_Eleve>> GetByGenreAsync(string genre);
        Task<IEnumerable<V_Eleve>> GetByNationaliteAsync(string nationalite);
        Task<IEnumerable<V_Eleve>> GetByProvinceAsync(string province);
        Task<IEnumerable<V_Eleve>> GetByVilleAsync(string ville);
        Task<IEnumerable<V_Eleve>> GetByCommuneAsync(string commune);
        Task<IEnumerable<V_Eleve>> GetByDateCreationAsync(DateTime date);
        Task<IEnumerable<V_Eleve>> GetByDateCreationRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<V_Eleve>> GetByDateNaissanceAsync(DateTime dateNaissance);
        Task<IEnumerable<V_Eleve>> GetByDateNaissanceRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<V_Eleve>> GetByNomCompletAsync(string nomComplet);
        Task<IEnumerable<V_Eleve>> GetByNomAsync(string nom);
        Task<IEnumerable<V_Eleve>> GetByPrenomAsync(string prenom);
        Task<IEnumerable<V_Eleve>> GetByLieuNaissanceAsync(string lieuNaissance);
        Task<IEnumerable<V_Eleve>> GetBySectionAsync(int idSection);
        Task<IEnumerable<V_Eleve>> GetByOptionAsync(int idOption);
        Task<IEnumerable<V_Eleve>> GetByStatutTuteurAsync(bool statutTuteur);
        Task<IEnumerable<V_Eleve>> GetByEmailTuteurAsync(string emailTuteur);
        Task<IEnumerable<V_Eleve>> GetByTelephoneTuteurAsync(string telephoneTuteur);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<bool> ExistsByMatriculeAsync(string matricule);
        Task<int> GetCountAsync();
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByClasseAsync(int idClasse);
        Task<int> GetCountByTuteurAsync(int idTuteur);
        Task<int> GetCountByStatutAsync(bool statut);
        Task<int> GetCountByGenreAsync(string genre);
    }
}
