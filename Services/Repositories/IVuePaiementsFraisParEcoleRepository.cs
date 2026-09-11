using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IVuePaiementsFraisParEcoleRepository
    {
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetAllAsync();
        Task<VuePaiementsFraisParEcoleDTO?> GetByIdAsync(int idPaiement);

        // Filtres par �cole
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEcoleNameAsync(string nomEcole);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTypeEcoleAsync(string typeEcole);

        // Filtres par �l�ve
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveAsync(int idEleve, int? idAnneeScolaire = null);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveReferenceAsync(Guid referenceEleve);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveMatriculeAsync(string matricule, int? idAnneeScolaire = null);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveNameAsync(string nomEleve);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveGenreAsync(string genre);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveStatutAsync(bool statut);

        // Filtres par classe
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByClasseNameAsync(string nomClasse);

        // Filtres par section
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetBySectionAsync(int idSection);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetBySectionNameAsync(string nomSection);

        // Filtres par direction
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDirectionAsync(int idDirection);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDirectionNameAsync(string nomDirection);

        // Filtres par option
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByOptionAsync(int idOption);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByOptionNameAsync(string nomOption);

        // Filtres par tuteur
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurAsync(int idTuteur);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurNameAsync(string nomTuteur);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurContactAsync(string contact);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurStatutAsync(bool statut);

        // Filtres par frais
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisAsync(int idFrais);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisLibelleAsync(string libelleFrais);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisMontantRangeAsync(double minMontant, double maxMontant);

        // Filtres par paiement
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByPaiementStatutAsync(string statut);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByModePaiementAsync(string modePaiement);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDeviseAsync(string devise);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByMontantRangeAsync(double minMontant, double maxMontant);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDatePaiementRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDatePaiementAsync(DateTime datePaiement);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByReferenceTransactionAsync(string referenceTransaction);

        // Filtres par localisation
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByProvinceAsync(string province);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByVilleAsync(string ville);
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByCommuneAsync(string commune);

        // Recherche g�n�rale
        Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> SearchAsync(string searchTerm);

        // Comptages
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByClasseAsync(int idClasse);
        Task<int> GetCountBySectionAsync(int idSection);
        Task<int> GetCountByDirectionAsync(int idDirection);
        Task<int> GetCountByTuteurAsync(int idTuteur);
        Task<int> GetCountByFraisAsync(int idFrais);
        Task<int> GetCountByPaiementStatutAsync(string statut);
        Task<int> GetCountByModePaiementAsync(string modePaiement);
        Task<int> GetCountByDatePaiementRangeAsync(DateTime dateDebut, DateTime dateFin);

        // Statistiques
        Task<double> GetTotalMontantByEcoleAsync(int idEcole);
        Task<double> GetTotalMontantByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<double> GetTotalMontantByPaiementStatutAsync(string statut);
        Task<double> GetTotalMontantByModePaiementAsync(string modePaiement);
    }
}
