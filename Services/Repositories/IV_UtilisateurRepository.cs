using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IV_UtilisateurRepository
    {
        Task<IEnumerable<V_Utilisateur>> GetAllAsync();
        Task<V_Utilisateur> GetByIdAsync(int id);
        Task<V_Utilisateur> GetByReferenceAsync(Guid reference);
        Task<V_Utilisateur> GetByEmailAsync(string email);
        Task<IEnumerable<V_Utilisateur>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<V_Utilisateur>> GetByRoleAsync(int idRole);
        Task<IEnumerable<V_Utilisateur>> GetByRoleNameAsync(string nomRole);
        Task<IEnumerable<V_Utilisateur>> GetByStatutAsync(bool statut);
        Task<IEnumerable<V_Utilisateur>> GetByConnecteAsync(bool isConnecte);
        Task<IEnumerable<V_Utilisateur>> GetByProvinceAsync(string province);
        Task<IEnumerable<V_Utilisateur>> GetByVilleAsync(string ville);
        Task<IEnumerable<V_Utilisateur>> GetByCommuneAsync(string commune);
        Task<IEnumerable<V_Utilisateur>> GetByDateCreationAsync(DateTime date);
        Task<IEnumerable<V_Utilisateur>> GetByDateCreationRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<V_Utilisateur>> GetByNomCompletAsync(string nomComplet);
        Task<IEnumerable<V_Utilisateur>> GetByTelephoneAsync(string telephone);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<int> GetCountAsync();
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByRoleAsync(int idRole);
        Task<int> GetCountByStatutAsync(bool statut);
    }
}
