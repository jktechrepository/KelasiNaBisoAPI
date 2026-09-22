namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Entrée publique minimale du registre école (endpoint anonyme).
    /// Pas d'idEcole, pas de contacts, pas de logo.
    /// </summary>
    public class RegistreEcoleDto
    {
        public string? Nom { get; set; }
        public string? Type { get; set; }
        public string? Province { get; set; }
        public string? Ville { get; set; }
        public string? Commune { get; set; }
        public string? ProvinceEducationnel { get; set; }
    }
}
