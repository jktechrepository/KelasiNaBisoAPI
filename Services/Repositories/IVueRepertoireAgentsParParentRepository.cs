using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IVueRepertoireAgentsParParentRepository
    {
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetAllAsync();
        Task<VueRepertoireAgentsParParentDTO?> GetByIdAsync(int idAgent);

        // Filtres par parent/tuteur
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentContactAsync(string contactParent);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentNameAsync(string nomParent);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentIdAsync(int idTuteur);

        // Filtres par élève
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveNameAsync(string nomEleve);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveMatriculeAsync(string matricule);

        // Filtres par agent
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentAsync(int idAgent);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentNameAsync(string nomAgent);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentContactAsync(string contactAgent);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentGenreAsync(string genre);

        // Filtres par cours
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByCoursAsync(int idCours);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByCoursNameAsync(string nomCours);

        // Filtres par classe
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByClasseNameAsync(string nomClasse);

        // Filtres par école
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEcoleNameAsync(string nomEcole);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByTypeEcoleAsync(string typeEcole);

        // Filtres par année scolaire
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAnneeScolaireAsync(int idAnneeScolaire);
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAnneeScolaireLibelleAsync(string libelleAnnee);

        // Recherche générale
        Task<IEnumerable<VueRepertoireAgentsParParentDTO>> SearchAsync(string searchTerm);

        // Comptages
        Task<int> GetCountByParentAsync(int idTuteur);
        Task<int> GetCountByAgentAsync(int idAgent);
        Task<int> GetCountByCoursAsync(int idCours);
        Task<int> GetCountByClasseAsync(int idClasse);
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByAnneeScolaireAsync(int idAnneeScolaire);

        // Statistiques
        Task<object> GetStatistiquesAgentsByParentAsync(string contactParent);
        Task<object> GetStatistiquesCoursByParentAsync(string contactParent);
        Task<object> GetRepertoireCompletByParentAsync(string contactParent);
    }
}

