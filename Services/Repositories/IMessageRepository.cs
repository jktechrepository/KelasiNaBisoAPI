using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetAllAsync();
        Task<Message> GetByIdAsync(int id);
        Task<IEnumerable<Message>> GetByExpediteurAsync(int idExpediteur);
        Task<IEnumerable<Message>> GetByDestinateurAsync(int idDestinateur);
        Task<IEnumerable<Message>> GetByGroupeAsync(int idGroupe);
        Task<IEnumerable<Message>> GetConversationAsync(int idExpediteur, int idDestinateur);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
    }
}
