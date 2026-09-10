using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IFraisRepository
    {
        Task<IEnumerable<FraisDto>> GetAllAsync();
        Task<ElevesAnneeScopedResult<IEnumerable<FraisDto>>> GetByEcoleAsync(
            int idEcole, int? idAnneeScolaire = null, int? idClasse = null);
        Task<FraisDto?> GetByIdAsync(int id);
        Task<FraisDto?> GetByLibelleFraisAsync(string libelleFrais);
        Task<ElevesAnneeScopedResult<FraisDto?>> GetByEcoleAndLibelleAsync(
            int idEcole, string libelleFrais, int? idAnneeScolaire = null, int? idClasse = null);
        Task<ElevesAnneeScopedResult<IEnumerable<FraisDto>>> GetByDirectionAsync(
            int idDirection, int? idAnneeScolaire = null, int? idClasse = null);
        Task<IEnumerable<FraisDto>> GetByAnneeAsync(int idAnneeScolaire);
        Task<FraisDto> CreateAsync(CreateFraisDto dto);
        Task<FraisDto?> UpdateAsync(int id, UpdateFraisDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByLibelleFraisAsync(string libelleFrais);
        Task<IEnumerable<Paiement>> GetPaiementsAsync(int idFrais);
        Task<bool> ToggleStatutAsync(int id);
    }
}
