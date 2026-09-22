using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.DTOs.Vitrine;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IEcoleRepository
    {
        Task<IEnumerable<Ecole>> GetAllAsync();
        Task<Ecole> GetByIdAsync(int id);
        Task<Ecole> GetByNomAsync(string nom);
       // Task<Ecole> GetByCodeAsync(string code);
       // Task<IEnumerable<Ecole>> GetByStatutAsync(bool statut);
        Task<Ecole> CreateAsync(Ecole ecole);
        Task<Ecole> UpdateAsync(Ecole ecole);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNomAsync(string nom);
     //   Task<bool> ExistsByCodeAsync(string code);
        Task<IEnumerable<Classe>> GetClassesAsync(int idEcole);
        Task<IEnumerable<Utilisateur>> GetUtilisateursAsync(int idEcole);
        Task<IEnumerable<Tuteur>> GetTuteursAsync(int idEcole);
        Task<IEnumerable<Agent>> GetAgentsAsync(int idEcole);
        Task<PagedResult<Agent>> GetAgentsByRoleAsync(int idEcole, string roleNom, PagedRequest request);
        Task<IEnumerable<Section>> GetSectionsAsync(int idEcole);
        Task<IEnumerable<AnneeScolaire>> GetAnneeScolairesAsync(int idEcole);
        Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idEcole);
        Task<IEnumerable<GroupeMessage>> GetGroupesMessagesAsync(int idEcole);
        
        // ✅ SOFT DELETE
        Task<bool> ToggleStatutAsync(int id);
        Task<bool> SetStatutAsync(int id, bool statut);
        
        // ✅ GESTION ACCEPT NOTIFICATION
        Task<bool> ToggleAcceptNotificationAsync(int id);
        Task<bool> SetAcceptNotificationAsync(int id, bool acceptNotification);

        /// <summary>Écoles actives avec logo, pour le carrousel partenaires du site vitrine.</summary>
        Task<IEnumerable<EcolePartenaireDto>> GetPartenairesLogosAsync(int limit = 50);

        /// <summary>
        /// Registre public des écoles (données minimales, anonymes).
        /// Recherche obligatoire par nom (min. 3 caractères). Filtres optionnels province / ville.
        /// </summary>
        Task<IReadOnlyList<RegistreEcoleDto>> GetRegistreEcoleAsync(
            string nom,
            string? province = null,
            string? ville = null,
            int limit = 10,
            CancellationToken cancellationToken = default);
    }
}
