using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Vacation
    {
        [Key]
        public int IdVacation { get; set; }
        [Required]
        [MaxLength(50)]
        public string NomVacation { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan HeureDebut { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan HeureFin { get; set; }
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan? HeureDebutPause { get; set; }
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan? HeureFinPause { get; set; }
        public int NombreJoursParSemaine { get; set; } = 5; // Par d�faut, 5 jours par semaine
        [Required]
        public int IdEcole { get; set; }
        public bool? Statut { get; set; } = true;

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Ecole Ecole { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Presence>? Presences { get; set; }

    }
}
