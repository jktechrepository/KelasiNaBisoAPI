using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IVuePointagePresenceParEcoleRepository
    {
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetAllAsync();
        Task<VuePointagePresenceParEcoleDTO?> GetByIdAsync(int idPresence);

        // Filtres par école
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEcoleAsync(int idEcole);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEcoleNameAsync(string nomEcole);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTypeEcoleAsync(string typeEcole);

        // Filtres par élève
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveAsync(int idEleve);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveReferenceAsync(Guid referenceEleve);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveMatriculeAsync(string matricule);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveNameAsync(string nomEleve);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveGenreAsync(string genre);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveStatutAsync(bool statut);

        // Filtres par classe
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByClasseAsync(int idClasse);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByClasseNameAsync(string nomClasse);

        // Filtres par section
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetBySectionAsync(int idSection);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetBySectionNameAsync(string nomSection);

        // Filtres par direction
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDirectionAsync(int idDirection);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDirectionNameAsync(string nomDirection);

        // Filtres par option
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByOptionAsync(int idOption);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByOptionNameAsync(string nomOption);

        // Filtres par tuteur
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurAsync(int idTuteur);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurNameAsync(string nomTuteur);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurContactAsync(string contact);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurStatutAsync(bool statut);

        // Filtres par horaire/vacation
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVacationAsync(int IdHoraire);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVacationNameAsync(string nomVacation);

        // Filtres par présence
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByPresenceStatutAsync(string statut);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDateDuJourAsync(DateTime dateDuJour);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByHeureArriveeRangeAsync(TimeSpan heureDebut, TimeSpan heureFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByHeureDepartRangeAsync(TimeSpan heureDebut, TimeSpan heureFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByRetardAsync(TimeSpan heureLimite);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByAbsenceAsync();
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByPresenceAsync();

        // Filtres par localisation
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByProvinceAsync(string province);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVilleAsync(string ville);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByCommuneAsync(string commune);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByLocalisationRangeAsync(string latMin, string latMax, string longMin, string longMax);

        // Recherche générale
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> SearchAsync(string searchTerm);

        // Comptages
        Task<int> GetCountByEcoleAsync(int idEcole);
        Task<int> GetCountByClasseAsync(int idClasse);
        Task<int> GetCountBySectionAsync(int idSection);
        Task<int> GetCountByDirectionAsync(int idDirection);
        Task<int> GetCountByTuteurAsync(int idTuteur);
        Task<int> GetCountByVacationAsync(int IdHoraire);
        Task<int> GetCountByPresenceStatutAsync(string statut);
        Task<int> GetCountByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
        Task<int> GetCountByDateDuJourAsync(DateTime dateDuJour);

        // Statistiques de présence
        Task<int> GetCountPresenceByEcoleAsync(int idEcole, DateTime dateDuJour);
        Task<int> GetCountAbsenceByEcoleAsync(int idEcole, DateTime dateDuJour);
        Task<int> GetCountRetardByEcoleAsync(int idEcole, DateTime dateDuJour);
        Task<double> GetTauxPresenceByEcoleAsync(int idEcole, DateTime dateDebut, DateTime dateFin);
        Task<double> GetTauxPresenceByClasseAsync(int idClasse, DateTime dateDebut, DateTime dateFin);
        Task<double> GetTauxPresenceByEleveAsync(int idEleve, DateTime dateDebut, DateTime dateFin);

        // Statistiques temporelles
        Task<TimeSpan> GetMoyenneHeureArriveeByEcoleAsync(int idEcole, DateTime dateDuJour);
        Task<TimeSpan> GetMoyenneHeureDepartByEcoleAsync(int idEcole, DateTime dateDuJour);
        Task<TimeSpan> GetMoyenneHeureArriveeByClasseAsync(int idClasse, DateTime dateDuJour);
        Task<TimeSpan> GetMoyenneHeureDepartByClasseAsync(int idClasse, DateTime dateDuJour);

        // === NOUVEAUX ENDPOINTS POUR LE SUIVI PARENTAL ===

        // Suivi détaillé par parent
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAsync(string contactParent);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAndDateAsync(string contactParent, DateTime date);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAndDateRangeAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetRetardsEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetAbsencesEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetPresencesEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin);

        // Statistiques hebdomadaires
        Task<object> GetStatistiquesHebdomadairesEnfantAsync(string contactParent, DateTime dateDebutSemaine);
        Task<object> GetStatistiquesHebdomadairesEnfantAsync(string contactParent, int semaine, int annee);

        // Statistiques mensuelles
        Task<object> GetStatistiquesMensuellesEnfantAsync(string contactParent, int mois, int annee);
        Task<object> GetStatistiquesMensuellesEnfantAsync(string contactParent, DateTime date);

        // Statistiques trimestrielles
        Task<object> GetStatistiquesTrimestriellesEnfantAsync(string contactParent, int trimestre, int annee);
        Task<object> GetStatistiquesTrimestriellesEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);

        // Statistiques annuelles
        Task<object> GetStatistiquesAnnuellesEnfantAsync(string contactParent, int annee);

        // Moyennes et tendances
        Task<object> GetMoyennePresenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetMoyenneHeuresArriveeEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetMoyenneHeuresDepartEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetTendancePresenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);

        // Alertes et notifications
        Task<object> GetAlertesRetardEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetAlertesAbsenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetResumeJourEnfantAsync(string contactParent, DateTime date);

        // Comparaisons
        Task<object> GetComparaisonPresenceEnfantAsync(string contactParent, DateTime periode1Debut, DateTime periode1Fin, DateTime periode2Debut, DateTime periode2Fin);
        Task<object> GetComparaisonAvecClasseEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);

        // Rapports détaillés
        Task<object> GetRapportCompletEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin);
        Task<object> GetRapportMensuelEnfantAsync(string contactParent, int mois, int annee);
        Task<object> GetRapportTrimestrielEnfantAsync(string contactParent, int trimestre, int annee);
        
        // Méthode temporaire pour recréer la vue
        void RecreateView();
    }
}
