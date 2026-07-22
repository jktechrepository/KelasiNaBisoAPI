using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Interface pour le service de reporting de présence
    /// Fournit des méthodes avancées pour les statistiques, pourcentages et analyses
    /// </summary>
    public interface IPresenceReportingService
    {
        // ═══════════════════════════════════════════════════════
        // REPORTING ÉLÈVES
        // ═══════════════════════════════════════════════════════
        
        /// <summary>
        /// Calcule les pourcentages de présence d'un élève sur une période
        /// </summary>
        Task<PourcentageEleveDto> GetElevePourcentageAsync(
            int idEleve, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode);
        
        /// <summary>
        /// Analyse les retards d'un élève sur une période
        /// </summary>
        Task<RetardsEleveDto> GetEleveRetardsAsync(
            int idEleve, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            TimeSpan heureReference);
        
        /// <summary>
        /// Obtient le reporting de présence pour une classe
        /// </summary>
        Task<ClassePresenceDto> GetClassePresencesAsync(
            int idClasse, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            bool includeDetails);
        
        /// <summary>
        /// Obtient le reporting de présence pour une option (groupement de classes)
        /// </summary>
        Task<object> GetOptionPresencesAsync(
            int idOption, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string? groupBy);
        
        /// <summary>
        /// Obtient le reporting de présence pour une section
        /// </summary>
        Task<object> GetSectionPresencesAsync(
            int idSection, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string? groupBy);
        
        /// <summary>
        /// Obtient le reporting de présence pour une direction
        /// </summary>
        Task<object> GetDirectionPresencesAsync(
            int idDirection, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string? groupBy);
        
        /// <summary>
        /// Obtient le reporting global de présence élèves pour une école
        /// </summary>
        Task<object> GetEcolePresencesElevesAsync(
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string? groupBy);
        
        /// <summary>
        /// Analyse globale des retards élèves
        /// </summary>
        Task<RetardsAnalyseDto> GetElevesRetardsAnalyseAsync(
            int idEcole, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            TimeSpan heureReference, 
            string? groupBy);
        
        // ═══════════════════════════════════════════════════════
        // REPORTING AGENTS
        // ═══════════════════════════════════════════════════════
        
        /// <summary>
        /// Calcule les pourcentages de présence d'un agent sur une période
        /// </summary>
        Task<PourcentageAgentDto> GetAgentPourcentageAsync(
            int idAgent, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode);
        
        /// <summary>
        /// Analyse les retards d'un agent sur une période
        /// </summary>
        Task<object> GetAgentRetardsAsync(
            int idAgent, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            TimeSpan heureReference);
        
        /// <summary>
        /// Obtient le reporting de présence pour une fonction d'agents
        /// </summary>
        Task<FonctionPresenceDto> GetFonctionPresencesAsync(
            string fonction, 
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode);
        
        /// <summary>
        /// Comparatif de toutes les fonctions d'agents
        /// </summary>
        Task<object> GetFonctionsComparatifAsync(
            int idEcole, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode);
        
        /// <summary>
        /// Obtient le reporting global de présence agents pour une école
        /// </summary>
        Task<object> GetEcolePresencesAgentsAsync(
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string? groupBy);
        
        /// <summary>
        /// Analyse globale des retards agents
        /// </summary>
        Task<RetardsAnalyseDto> GetAgentsRetardsAnalyseAsync(
            int idEcole, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            TimeSpan heureReference, 
            string? groupBy);
        
        // ═══════════════════════════════════════════════════════
        // DASHBOARDS
        // ═══════════════════════════════════════════════════════
        
        /// <summary>
        /// Obtient le dashboard de présence pour une école
        /// </summary>
        Task<DashboardPresenceDto> GetDashboardEcoleAsync(
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin);
        
        /// <summary>
        /// Obtient la structure hiérarchique complète avec taux de présence
        /// </summary>
        Task<object> GetHierarchiePresencesAsync(
            int idEcole, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            string type);
    }
}

