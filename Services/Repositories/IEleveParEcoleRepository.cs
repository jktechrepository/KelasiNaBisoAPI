using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEleveParEcoleRepository
    {
        Task<IEnumerable<EleveParEcoleDTO>> GetAllAsync();
        Task<EleveParEcoleDTO?> GetByIdAsync(int idEleve);
        Task<IEnumerable<EleveParEcoleDTO>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<EleveParEcoleDTO>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<EleveParEcoleDTO>> GetByDirectionAsync(int idDirection);
        Task<IEnumerable<EleveParEcoleDTO>> GetByOptionAsync(int idOption);
        Task<IEnumerable<EleveParEcoleDTO>> GetByTuteurAsync(int idTuteur);
        Task<IEnumerable<EleveParEcoleDTO>> GetByStatutAsync(bool statut);
        Task<IEnumerable<EleveParEcoleDTO>> GetByGenreAsync(string genre);
        Task<IEnumerable<EleveParEcoleDTO>> GetByAgeRangeAsync(int minAge, int maxAge);
        Task<IEnumerable<EleveParEcoleDTO>> GetByProvinceAsync(string province);
        Task<IEnumerable<EleveParEcoleDTO>> GetByVilleAsync(string ville);
        Task<IEnumerable<EleveParEcoleDTO>> GetByCommuneAsync(string commune);
        Task<IEnumerable<EleveParEcoleDTO>> GetByTuteurContactAsync(string contact);
        Task<IEnumerable<EleveParEcoleDTO>> SearchAsync(string searchTerm);
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByClasseAsync(int idClasse);
        Task<int> GetCountByDirectionAsync(int idDirection);
    }
}
