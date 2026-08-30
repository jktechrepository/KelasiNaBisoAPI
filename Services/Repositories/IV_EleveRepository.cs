using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IV_EleveRepository
    {
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null);
        Task<V_Eleve?> GetByIdAsync(int id);
        Task<V_Eleve?> GetByReferenceAsync(Guid reference);
        Task<V_Eleve?> GetByMatriculeAsync(string matricule);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByTuteurAsync(int idTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByStatutAsync(bool statut, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByGenreAsync(string genre, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNationaliteAsync(string nationalite, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByProvinceAsync(string province, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByVilleAsync(string ville, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByCommuneAsync(string commune, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateCreationAsync(DateTime date, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateCreationRangeAsync(DateTime dateDebut, DateTime dateFin, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateNaissanceAsync(DateTime dateNaissance, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateNaissanceRangeAsync(DateTime dateDebut, DateTime dateFin, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNomCompletAsync(string nomComplet, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNomAsync(string nom, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByPrenomAsync(string prenom, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByLieuNaissanceAsync(string lieuNaissance, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetBySectionAsync(int idSection, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByOptionAsync(int idOption, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByStatutTuteurAsync(bool statutTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByEmailTuteurAsync(string emailTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByTelephoneTuteurAsync(string telephoneTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<bool> ExistsByMatriculeAsync(string matricule);
        Task<ElevesAnneeScopedResult<int>> GetCountAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByTuteurAsync(int idTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByStatutAsync(bool statut, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByGenreAsync(string genre, int idEcole, int? idAnneeScolaire = null);
    }
}
