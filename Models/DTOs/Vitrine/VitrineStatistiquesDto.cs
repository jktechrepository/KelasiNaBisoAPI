namespace KelasiNaBiso.Models.DTOs.Vitrine
{
    /// <summary>Statistiques globales affichées sur le site vitrine.</summary>
    public class VitrineStatistiquesDto
    {
        public int NombreEcoles { get; set; }
        public int NombreEleves { get; set; }
        public int NombrePersonnel { get; set; }
        public int NombreParentsConnectes { get; set; }
        public DateTime DateMiseAJour { get; set; }
    }
}
