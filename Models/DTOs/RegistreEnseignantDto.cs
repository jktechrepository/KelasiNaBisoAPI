namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Entrée publique minimale du registre enseignant (endpoint anonyme).
    /// Pas d'idAgent, pas de contacts, pas d'école.
    /// </summary>
    public class RegistreEnseignantDto
    {
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? Province { get; set; }
        public string? Ville { get; set; }
    }
}
