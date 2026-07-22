namespace KelasiNaBiso.Models.DTOs.Reporting
{
    /// <summary>
    /// DTO pour le dashboard global combiné (Présence + Paiement)
    /// </summary>
    public class DashboardGlobalDto
    {
        public EcoleInfoDto Ecole { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public StatistiquesGeneralesDto Statistiques { get; set; } = new();
        public RepartitionElevesDto RepartitionEleves { get; set; } = new();
        public DashboardPresenceResumeDto Presence { get; set; } = new();
        public DashboardPaiementResumeDto Paiement { get; set; } = new();
    }

    /// <summary>
    /// Statistiques générales de l'école (Classes, Élèves, Enseignants, Directions)
    /// </summary>
    public class StatistiquesGeneralesDto
    {
        public int NombreClasses { get; set; }
        public int NombreEleves { get; set; }
        public int NombreEnseignants { get; set; }
        public int NombreDirections { get; set; }
        public int NombreElevesActifs { get; set; }
        public int NombreEnseignantsActifs { get; set; }
    }

    /// <summary>
    /// Résumé du dashboard de présence pour le dashboard global
    /// </summary>
    public class DashboardPresenceResumeDto
    {
        public ResumePresenceDto ResumeEleves { get; set; } = new();
        public ResumePresenceDto ResumeAgents { get; set; } = new();
        public List<AlerteDto>? Alertes { get; set; }
        public List<ClasseProblematiqueDto>? ClassesProblematiques { get; set; }
        public List<AgentAbsentDto>? AgentsAbsents { get; set; }
    }

    /// <summary>
    /// Résumé du dashboard de paiement pour le dashboard global
    /// </summary>
    public class DashboardPaiementResumeDto
    {
        public KelasiNaBiso.Models.DTOs.Paiement.ResumePaiementDto Resume { get; set; } = new();
        public KelasiNaBiso.Models.DTOs.Paiement.RepartitionModePaiementDto RepartitionParMode { get; set; } = new();
        public List<KelasiNaBiso.Models.DTOs.Paiement.Top5FraisDto>? Top5Frais { get; set; }
    }

    /// <summary>
    /// DTO pour la comparaison multi-écoles (à implémenter)
    /// </summary>
    public class DashboardComparaisonDto
    {
        public PeriodeDto Periode { get; set; } = new();
        public List<EcoleComparaisonDto> Ecoles { get; set; } = new();
        public StatistiquesGlobalesDto StatistiquesGlobales { get; set; } = new();
    }

    public class EcoleComparaisonDto
    {
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public decimal TauxPresenceEleves { get; set; }
        public decimal TauxPresenceAgents { get; set; }
        public decimal TauxRecouvrement { get; set; }
        public int NombreEleves { get; set; }
        public int NombreAgents { get; set; }
        public string Classement { get; set; } = string.Empty; // "Excellent", "Bon", "Moyen", "Faible"
    }

    public class StatistiquesGlobalesDto
    {
        public int NombreTotalEcoles { get; set; }
        public int NombreTotalEleves { get; set; }
        public int NombreTotalAgents { get; set; }
        public decimal TauxPresenceMoyenEleves { get; set; }
        public decimal TauxPresenceMoyenAgents { get; set; }
        public decimal TauxRecouvrementMoyen { get; set; }
        public decimal MontantTotalRecouvert { get; set; }
    }

    /// <summary>
    /// DTO pour les indicateurs clés de performance (à implémenter)
    /// </summary>
    public class DashboardKpiDto
    {
        public EcoleInfoDto Ecole { get; set; } = new();
        public PeriodeDto Periode { get; set; } = new();
        public KpiPresenceDto KpiPresence { get; set; } = new();
        public KpiPaiementDto KpiPaiement { get; set; } = new();
        public List<TendanceDto> Tendances { get; set; } = new();
    }

    public class KpiPresenceDto
    {
        public decimal TauxPresenceEleves { get; set; }
        public decimal EvolutionTauxPresenceEleves { get; set; } // % par rapport à la période précédente
        public decimal TauxPresenceAgents { get; set; }
        public decimal EvolutionTauxPresenceAgents { get; set; }
        public int NombreClassesCritiques { get; set; }
        public int NombreAgentsAbsents { get; set; }
    }

    public class KpiPaiementDto
    {
        public decimal TauxRecouvrement { get; set; }
        public decimal EvolutionTauxRecouvrement { get; set; } // % par rapport à la période précédente
        public decimal MontantTotalRecouvert { get; set; }
        public decimal EvolutionMontantRecouvert { get; set; }
        public int NombreElevesEnRetard { get; set; }
        public decimal PourcentageElevesEnRetard { get; set; }
    }

    public class TendanceDto
    {
        public DateTime Date { get; set; }
        public decimal TauxPresence { get; set; }
        public decimal TauxRecouvrement { get; set; }
    }

    /// <summary>
    /// DTO pour la répartition des élèves par école, direction, section et option
    /// </summary>
    public class RepartitionElevesDto
    {
        /// <summary>
        /// Nombre total d'élèves pour l'école (déjà dans StatistiquesGeneralesDto, mais inclus ici pour cohérence)
        /// </summary>
        public int TotalEleves { get; set; }

        /// <summary>
        /// Nombre d'élèves actifs
        /// </summary>
        public int TotalElevesActifs { get; set; }

        /// <summary>
        /// Répartition des élèves par direction
        /// </summary>
        public List<RepartitionDirectionDto> ParDirection { get; set; } = new();

        /// <summary>
        /// Répartition des élèves par section
        /// </summary>
        public List<RepartitionSectionDto> ParSection { get; set; } = new();

        /// <summary>
        /// Répartition des élèves par option
        /// </summary>
        public List<RepartitionOptionDto> ParOption { get; set; } = new();
    }

    /// <summary>
    /// Répartition des élèves par direction
    /// </summary>
    public class RepartitionDirectionDto
    {
        public int IdDirection { get; set; }
        public string NomDirection { get; set; } = string.Empty;
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public decimal Pourcentage { get; set; }
    }

    /// <summary>
    /// Répartition des élèves par section
    /// </summary>
    public class RepartitionSectionDto
    {
        public int IdSection { get; set; }
        public string NomSection { get; set; } = string.Empty;
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public decimal Pourcentage { get; set; }
    }

    /// <summary>
    /// Répartition des élèves par option
    /// </summary>
    public class RepartitionOptionDto
    {
        public int IdOption { get; set; }
        public string NomOption { get; set; } = string.Empty;
        public int? IdSection { get; set; }
        public string? NomSection { get; set; }
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public decimal Pourcentage { get; set; }
    }

    /// <summary>
    /// DTO pour le dashboard Super-Admin (vue d'ensemble complète de toutes les écoles)
    /// </summary>
    public class DashboardSuperAdminDto
    {
        /// <summary>
        /// Période de référence pour les données
        /// </summary>
        public PeriodeDto Periode { get; set; } = new();

        /// <summary>
        /// Statistiques générales globales (toutes écoles confondues)
        /// </summary>
        public StatistiquesGeneralesGlobalesDto StatistiquesGlobales { get; set; } = new();

        /// <summary>
        /// Répartition des élèves par école
        /// </summary>
        public List<RepartitionEcoleDto> ParEcole { get; set; } = new();

        /// <summary>
        /// Répartition des élèves par province
        /// </summary>
        public List<RepartitionProvinceDto> ParProvince { get; set; } = new();

        /// <summary>
        /// Répartition des élèves par ville
        /// </summary>
        public List<RepartitionVilleDto> ParVille { get; set; } = new();

        /// <summary>
        /// Résumé des présences (agrégé toutes écoles)
        /// </summary>
        public DashboardPresenceGlobalDto? PresenceGlobale { get; set; }

        /// <summary>
        /// Résumé des paiements (agrégé toutes écoles)
        /// </summary>
        public DashboardPaiementGlobalDto? PaiementGlobal { get; set; }

        /// <summary>
        /// Top 10 des écoles par nombre d'élèves
        /// </summary>
        public List<RepartitionEcoleDto> Top10Ecoles { get; set; } = new();

        /// <summary>
        /// Alertes et points d'attention globaux
        /// </summary>
        public List<AlerteGlobaleDto> Alertes { get; set; } = new();
    }

    /// <summary>
    /// Statistiques générales globales (toutes écoles confondues)
    /// </summary>
    public class StatistiquesGeneralesGlobalesDto
    {
        /// <summary>
        /// Nombre total d'écoles actives
        /// </summary>
        public int TotalEcoles { get; set; }

        /// <summary>
        /// Nombre total de directions (toutes écoles)
        /// </summary>
        public int TotalDirections { get; set; }

        /// <summary>
        /// Nombre total de classes (toutes écoles)
        /// </summary>
        public int TotalClasses { get; set; }

        /// <summary>
        /// Nombre total d'élèves (toutes écoles)
        /// </summary>
        public int TotalEleves { get; set; }

        /// <summary>
        /// Nombre total d'élèves actifs
        /// </summary>
        public int TotalElevesActifs { get; set; }

        /// <summary>
        /// Nombre total d'élèves filles (toutes écoles)
        /// </summary>
        public int TotalElevesFilles { get; set; }

        /// <summary>
        /// Nombre total d'élèves garçons (toutes écoles)
        /// </summary>
        public int TotalElevesGarcons { get; set; }

        /// <summary>
        /// Nombre total d'élèves filles actives
        /// </summary>
        public int TotalElevesFillesActives { get; set; }

        /// <summary>
        /// Nombre total d'élèves garçons actifs
        /// </summary>
        public int TotalElevesGarconsActifs { get; set; }

        /// <summary>
        /// Pourcentage de filles par rapport au total
        /// </summary>
        public decimal PourcentageFilles { get; set; }

        /// <summary>
        /// Pourcentage de garçons par rapport au total
        /// </summary>
        public decimal PourcentageGarcons { get; set; }

        /// <summary>
        /// Nombre total d'enseignants/agents (toutes écoles)
        /// </summary>
        public int TotalEnseignants { get; set; }

        /// <summary>
        /// Nombre total d'enseignants actifs
        /// </summary>
        public int TotalEnseignantsActifs { get; set; }

        /// <summary>
        /// Moyennes par école
        /// </summary>
        public MoyennesParEcoleDto Moyennes { get; set; } = new();
    }

    /// <summary>
    /// Moyennes calculées par école
    /// </summary>
    public class MoyennesParEcoleDto
    {
        /// <summary>
        /// Nombre moyen d'élèves par école
        /// </summary>
        public decimal ElevesParEcole { get; set; }

        /// <summary>
        /// Nombre moyen d'enseignants par école
        /// </summary>
        public decimal EnseignantsParEcole { get; set; }

        /// <summary>
        /// Nombre moyen de classes par école
        /// </summary>
        public decimal ClassesParEcole { get; set; }

        /// <summary>
        /// Ratio élèves/enseignants global
        /// </summary>
        public decimal RatioElevesEnseignants { get; set; }
    }

    /// <summary>
    /// Résumé des présences agrégé toutes écoles
    /// </summary>
    public class DashboardPresenceGlobalDto
    {
        /// <summary>
        /// Résumé global des présences élèves
        /// </summary>
        public ResumePresenceGlobalDto ResumeEleves { get; set; } = new();

        /// <summary>
        /// Résumé global des présences agents
        /// </summary>
        public ResumePresenceGlobalDto ResumeAgents { get; set; } = new();

        /// <summary>
        /// Taux de présence moyen toutes écoles
        /// </summary>
        public decimal TauxPresenceMoyen { get; set; }

        /// <summary>
        /// Nombre d'écoles avec des problèmes de présence
        /// </summary>
        public int EcolesProblematiques { get; set; }
    }

    /// <summary>
    /// Résumé des paiements agrégé toutes écoles
    /// </summary>
    public class DashboardPaiementGlobalDto
    {
        /// <summary>
        /// Montant total collecté (toutes écoles)
        /// </summary>
        public decimal MontantTotalCollecte { get; set; }

        /// <summary>
        /// Nombre total de paiements
        /// </summary>
        public int NombreTotalPaiements { get; set; }

        /// <summary>
        /// Taux de recouvrement moyen
        /// </summary>
        public decimal TauxRecouvrementMoyen { get; set; }

        /// <summary>
        /// Répartition par mode de paiement (agrégé)
        /// </summary>
        public List<RepartitionModeDto> RepartitionParMode { get; set; } = new();
    }

    /// <summary>
    /// Résumé de présence global
    /// </summary>
    public class ResumePresenceGlobalDto
    {
        /// <summary>
        /// Nombre total de présences
        /// </summary>
        public int TotalPresences { get; set; }

        /// <summary>
        /// Nombre total d'absences
        /// </summary>
        public int TotalAbsences { get; set; }

        /// <summary>
        /// Taux de présence global
        /// </summary>
        public decimal TauxPresence { get; set; }
    }

    /// <summary>
    /// Alerte globale pour le Super-Admin
    /// </summary>
    public class AlerteGlobaleDto
    {
        /// <summary>
        /// Type d'alerte (PRESENCE, PAIEMENT, SYSTEME)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Niveau de gravité (INFO, WARNING, ERROR, CRITICAL)
        /// </summary>
        public string Niveau { get; set; } = string.Empty;

        /// <summary>
        /// Message de l'alerte
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Nombre d'écoles concernées
        /// </summary>
        public int EcolesConcernees { get; set; }

        /// <summary>
        /// Détails supplémentaires
        /// </summary>
        public object? Details { get; set; }
    }

    /// <summary>
    /// Répartition par mode de paiement
    /// </summary>
    public class RepartitionModeDto
    {
        /// <summary>
        /// Mode de paiement
        /// </summary>
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de paiements
        /// </summary>
        public int Nombre { get; set; }

        /// <summary>
        /// Montant total pour ce mode
        /// </summary>
        public decimal Montant { get; set; }

        /// <summary>
        /// Pourcentage par rapport au total
        /// </summary>
        public decimal Pourcentage { get; set; }
    }

    /// <summary>
    /// Répartition des élèves par école
    /// </summary>
    public class RepartitionEcoleDto
    {
        public int IdEcole { get; set; }
        public string NomEcole { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? Ville { get; set; }
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public int NombreElevesFilles { get; set; }
        public int NombreElevesGarcons { get; set; }
        public int NombreElevesFillesActives { get; set; }
        public int NombreElevesGarconsActifs { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal PourcentageFilles { get; set; }
        public decimal PourcentageGarcons { get; set; }
    }

    /// <summary>
    /// Répartition des élèves par province
    /// </summary>
    public class RepartitionProvinceDto
    {
        public string Province { get; set; } = string.Empty;
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public int NombreElevesFilles { get; set; }
        public int NombreElevesGarcons { get; set; }
        public int NombreEcoles { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal PourcentageFilles { get; set; }
        public decimal PourcentageGarcons { get; set; }
    }

    /// <summary>
    /// Répartition des élèves par ville
    /// </summary>
    public class RepartitionVilleDto
    {
        public string Ville { get; set; } = string.Empty;
        public string? Province { get; set; }
        public int NombreEleves { get; set; }
        public int NombreElevesActifs { get; set; }
        public int NombreElevesFilles { get; set; }
        public int NombreElevesGarcons { get; set; }
        public int NombreEcoles { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal PourcentageFilles { get; set; }
        public decimal PourcentageGarcons { get; set; }
    }
}

