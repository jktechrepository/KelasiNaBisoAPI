using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IGroupeMessageRepository
    {
        Task<IEnumerable<GroupeMessage>> GetAllAsync();
        Task<GroupeMessage> GetByIdAsync(int id);
        Task<GroupeMessage> GetByNomAsync(string nom);
        Task<IEnumerable<GroupeMessage>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<GroupeMessage>> GetByUtilisateurAsync(int idUtilisateur);
       // Task<IEnumerable<GroupeMessage>> GetByTypeAsync(string type);
        Task<IEnumerable<GroupeMessage>> GetByDateCreationAsync(DateTime date);
        Task<GroupeMessage> CreateAsync(GroupeMessage groupeMessage);
        Task<GroupeMessage> UpdateAsync(GroupeMessage groupeMessage);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
        Task<IEnumerable<Message>> GetMessagesAsync(int idGroupe);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
