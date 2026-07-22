using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Direction
    {
        [Key]
        public int IdDirection { get; set; }
        [Required]
        public string? NomDirection { get; set; }
        public int? IdEcole { get; set; }
        
        // ✅ NOUVEAU : Niveau d'enseignement pour gestion intelligente des affectations
        // Valeurs possibles: "MATERNELLE", "PRIMAIRE", "SECONDAIRE"
        [MaxLength(20)]
        public string? NiveauEnseignement { get; set; }
        
        public bool? Statut { get; set; } = true;

        //Attributs Techniques
        [JsonIgnore]
        public DateTime? DateCreation { get; set; } = DateTime.Now;


        //Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Ecole? Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Classe>? Classes { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Frais>? Frais { get; set; }



    }
}
