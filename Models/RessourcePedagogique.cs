using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class RessourcePedagogique
    {
        [Key]
        public int IdRessourcePedagogique { get; set; }
        public string TitreRessourcePedagogique { get; set; }
        public string FormatRessourcePedagogique{ get; set; }
        public string UrlRessourcePedagogique { get; set; }
        public int IdCours { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Technique
        [JsonIgnore]
        public DateTime DateCreation { get; set; }


        // Attributs de Navigation
        [JsonIgnore]
        [ValidateNever]
        public Cours? Cours { get; set; }
    }
}
