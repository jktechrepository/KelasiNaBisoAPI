using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IUtilisateurRepository
    {
        Task<IEnumerable<V_Utilisateur>> GetAllAsync();
        Task<Utilisateur> GetByIdAsync(int id);
        Task<Utilisateur> GetByEmailAsync(string email);
        Task<Utilisateur> GetByDefaultUsernameAsync(string defaultUsername);
        Task<V_Utilisateur> GetByReferenceAsync(Guid reference);
        Task<IEnumerable<V_Utilisateur>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<V_Utilisateur>> GetByRoleAsync(int idRole);
        Task<IEnumerable<V_Utilisateur>> GetByStatutAsync(bool statut);
        Task<IEnumerable<V_Utilisateur>> GetByConnecteAsync(bool isConnecte);
        Task<IEnumerable<V_Utilisateur>> GetByDateCreationAsync(DateTime date);
        Task<IEnumerable<V_Utilisateur>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<V_Utilisateur>> GetByNomCompletAsync(string nomComplet);
        Task<IEnumerable<Utilisateur>> GetByTelephoneAsync(string telephone);
        Task<Utilisateur> CreateAsync(Utilisateur utilisateur);
        Task<Utilisateur> UpdateAsync(Utilisateur utilisateur);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByReferenceAsync(Guid reference);
        Task<bool> AuthentifierAsync(string email, string motDePasse);
        Task<bool> ChangerMotDePasseAsync(int id, string ancienMotDePasse, string nouveauMotDePasse);
        Task<bool> MarquerCommeConnecteAsync(int id);
        Task<bool> MarquerCommeDeconnecteAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationsEnvoyeesAsync(int idUtilisateur);
        Task<IEnumerable<Notification>> GetNotificationsRecuesAsync(int idUtilisateur);
        Task<IEnumerable<Message>> GetMessagesEnvoyesAsync(int idUtilisateur);
        Task<IEnumerable<Message>> GetMessagesRecusAsync(int idUtilisateur);
        Task<IEnumerable<Paiement>> GetPaiementsAsync(int idUtilisateur);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
        
        // ✅ RÉINITIALISATION MOT DE PASSE
        Task<int> ReinitialiserMotDePasseMasseAsync(int idEcole, int idRole, string nouveauMotDePasse);
        Task<bool> ReinitialiserMotDePasseIndividuelAsync(int idUtilisateur, string nouveauMotDePasse);

        // ✅ MULTI-RÔLES : gestion des rôles utilisateur
        Task<bool> AddRoleToUserAsync(int userId, int roleId, int? assignedByUserId = null, bool isPrimary = false);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    }
}
