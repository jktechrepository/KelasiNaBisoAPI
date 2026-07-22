using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Models
{
    public class Presence
    {
        [Key]
        public int IdPresence { get; set; }
        
        // ✅ POINTAGE FLEXIBLE: Peut concerner un élève OU un agent
        // Au moins un des deux doit être renseigné (validation métier)
        public int? IdEleve { get; set; }
        public int? IdAgent { get; set; }
        
        // ✅ SOFT DELETE: Statut actif/inactif (true = actif, false = désactivé)
        public bool? Statut { get; set; } = true;
        
        // ✅ INDICATEUR DE PRÉSENCE: Indique si la personne est effectivement présente
        // null = non renseigné, true = présent, false = absent
        public bool? IsPresent { get; set; }
        
        // ✅ TYPE DE PRÉSENCE: Indique si c'est un élève ou un agent
        // Valeurs possibles: "ELEVE" ou "AGENT"
        [MaxLength(10)]
        public string? TypePresence { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public TimeSpan HeureArrivee { get; set; }

        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public TimeSpan? HeureDepart { get; set; }
        [Required]
        public DateTime DateDuJour { get; set; }
        
        // ✅ OBSERVATION: Note ou observation sur la présence (ex: "Retard", "Absent justifié", etc.)
        [MaxLength(500)]
        public string? Observation { get; set; }
        
        public string? Longitute { get; set; }
        public string? Latitude { get; set; }
        
        // ✅ HORAIRE: Référence à l'horaire (nullable car ancien champ)
        public int? HoraireIdHoraire { get; set; }
        
        public int? IdVacation { get; set; }

        // Attributs Techniques
        [JsonIgnore]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        public Vacation? Vacation { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Eleve? Eleve { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public Agent? Agent { get; set; }
    }
}
