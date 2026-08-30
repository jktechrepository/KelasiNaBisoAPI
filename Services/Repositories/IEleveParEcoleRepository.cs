using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEleveParEcoleRepository
    {
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null);
        Task<EleveParEcoleDTO?> GetByIdAsync(int idEleve);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByDirectionAsync(int idDirection, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByOptionAsync(int idOption, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteurAsync(int idTuteur, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByStatutAsync(bool statut, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByGenreAsync(string genre, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByAgeRangeAsync(int minAge, int maxAge, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByProvinceAsync(string province, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByVilleAsync(string ville, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByCommuneAsync(string commune, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteurContactAsync(string contact, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> SearchAsync(string searchTerm, int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByEcoleAsync(int idEcole, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByClasseAsync(int idClasse, int? idAnneeScolaire = null);
        Task<ElevesAnneeScopedResult<int>> GetCountByDirectionAsync(int idDirection, int? idAnneeScolaire = null);
    }
}
