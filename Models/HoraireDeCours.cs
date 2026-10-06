using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Models;

public class HoraireDeCours
{
    [Key]
    public int IdHoraireDeCours { get; set; }

    [Required]
    public JourSemaine Jour { get; set; }

    [Required]
    public TimeSpan HeureDebut { get; set; }

    [Required]
    public TimeSpan HeureFin { get; set; }

    [Required]
    public int IdCours { get; set; }

    [Required]
    public int IdClasse { get; set; }

    [Required]
    public int IdAgent { get; set; }

    [Required]
    public int IdAnneeScolaire { get; set; }

    [MaxLength(150)]
    public string? Salle { get; set; }

    public bool Statut { get; set; } = true;

    [JsonIgnore]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [JsonIgnore]
    public Cours? Cours { get; set; }

    [JsonIgnore]
    public Classe? Classe { get; set; }

    [JsonIgnore]
    public Agent? Agent { get; set; }

    [JsonIgnore]
    public AnneeScolaire? AnneeScolaire { get; set; }
}
