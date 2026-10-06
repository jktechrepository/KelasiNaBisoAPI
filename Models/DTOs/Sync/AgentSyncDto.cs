namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>Agent actif à conserver dans le cache local pour le pointage offline.</summary>
    public class AgentSyncDto
    {
        public int IdAgent { get; set; }
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? Fonction { get; set; }
    }
}
