namespace KelasiNaBiso.Models.DTOs.DevoirADomicile
{
    /// <summary>
    /// Élève du tuteur concerné par un devoir (via sa classe d'inscription).
    /// </summary>
    public class EleveConcerneDevoirDto
    {
        public int IdEleve { get; set; }
        public string? NomComplet { get; set; }
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; }
    }

    /// <summary>
    /// Devoir à domicile pour un tuteur, avec les enfants concernés.
    /// </summary>
    public class DevoirADomicilePourTuteurDto : DevoirADomicileDto
    {
        public List<EleveConcerneDevoirDto> ElevesConcernes { get; set; } = new();
    }
}
