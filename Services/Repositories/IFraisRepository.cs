using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IFraisRepository
    {
        Task<IEnumerable<Frais>> GetAllAsync();
        Task<IEnumerable<Frais>> GetByEcoleAsync(int idEcole);
        Task<Frais> GetByIdAsync(int id);
        Task<Frais> GetByLibelleFraisAsync(string libelleFrais);
        Task<Frais> GetByEcoleAndLibelleAsync(int idEcole, string libelleFrais);
        Task<IEnumerable<Frais>> GetByDirectionAsync(int idDirection);
       // Task<IEnumerable<Frais>> GetByTypeAsync(string type);
      //  Task<IEnumerable<Frais>> GetByMontantAsync(decimal montant);
      //  Task<IEnumerable<Frais>> GetByStatutAsync(bool statut);
        Task<Frais> CreateAsync(Frais frais);
        Task<Frais> UpdateAsync(Frais frais);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByLibelleFraisAsync(string libelleFrais);
        Task<IEnumerable<Paiement>> GetPaiementsAsync(int idFrais);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
