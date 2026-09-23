using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>Corps pour réaffecter le tuteur d'un élève.</summary>
    public class AffecterTuteurEleveDto
    {
        [Required(ErrorMessage = "idTuteur est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "idTuteur doit être un identifiant positif")]
        public int IdTuteur { get; set; }
    }

    /// <summary>Résultat de l'affectation tuteur → élève.</summary>
    public class AffecterTuteurEleveResultDto
    {
        public int IdEleve { get; set; }
        public int IdTuteur { get; set; }
        public int? IdTuteurPrecedent { get; set; }
        public string? NomCompletEleve { get; set; }
        public string? NomCompletTuteur { get; set; }
    }
}
