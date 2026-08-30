using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IFraisRepository
    {
        Task<IEnumerable<Frais>> GetAllAsync();
        Task<ElevesAnneeScopedResult<IEnumerable<Frais>>> GetByEcoleAsync(
            int idEcole, int? idAnneeScolaire = null, int? idClasse = null);
        Task<Frais?> GetByIdAsync(int id);
        Task<Frais?> GetByLibelleFraisAsync(string libelleFrais);
        Task<ElevesAnneeScopedResult<Frais?>> GetByEcoleAndLibelleAsync(
            int idEcole, string libelleFrais, int? idAnneeScolaire = null, int? idClasse = null);
        Task<ElevesAnneeScopedResult<IEnumerable<Frais>>> GetByDirectionAsync(
            int idDirection, int? idAnneeScolaire = null, int? idClasse = null);
        Task<IEnumerable<Frais>> GetByAnneeAsync(int idAnneeScolaire);
        Task<Frais> CreateAsync(Frais frais);
        Task<Frais?> UpdateAsync(Frais frais);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByLibelleFraisAsync(string libelleFrais);
        Task<IEnumerable<Paiement>> GetPaiementsAsync(int idFrais);
        Task<bool> ToggleStatutAsync(int id);
    }
}
