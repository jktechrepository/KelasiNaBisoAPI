using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Serialization;

namespace KelasiNaBiso.Models.DTOs;

/// <summary>Payload for creating a recurring weekly course schedule entry.</summary>
public record CreateHoraireDeCoursRequest
{
    [Required]
    public JourSemaine Jour { get; init; }

    [Required]
    [JsonConverter(typeof(HhMmTimeSpanJsonConverter))]
    public TimeSpan HeureDebut { get; init; }

    [Required]
    [JsonConverter(typeof(HhMmTimeSpanJsonConverter))]
    public TimeSpan HeureFin { get; init; }

    [Range(1, int.MaxValue)]
    public int IdCours { get; init; }

    [Range(1, int.MaxValue)]
    public int IdClasse { get; init; }

    [Range(1, int.MaxValue)]
    public int IdAgent { get; init; }

    [Range(1, int.MaxValue)]
    public int IdAnneeScolaire { get; init; }

    [MaxLength(150)]
    public string? Salle { get; init; }
}

/// <summary>Payload for replacing a recurring weekly course schedule entry.</summary>
public sealed record UpdateHoraireDeCoursRequest
{
    [Required]
    public JourSemaine Jour { get; init; }

    [Required]
    [JsonConverter(typeof(HhMmTimeSpanJsonConverter))]
    public TimeSpan HeureDebut { get; init; }

    [Required]
    [JsonConverter(typeof(HhMmTimeSpanJsonConverter))]
    public TimeSpan HeureFin { get; init; }

    [Range(1, int.MaxValue)]
    public int IdCours { get; init; }

    [Range(1, int.MaxValue)]
    public int IdClasse { get; init; }

    [Range(1, int.MaxValue)]
    public int IdAgent { get; init; }

    [Range(1, int.MaxValue)]
    public int IdAnneeScolaire { get; init; }

    [MaxLength(150)]
    public string? Salle { get; init; }

    [Required]
    public bool Statut { get; init; } = true;
}

/// <summary>Represents a recurring weekly course schedule entry.</summary>
public sealed record HoraireDeCoursResponse(
    int IdHoraireDeCours,
    JourSemaine Jour,
    [property: JsonConverter(typeof(HhMmTimeSpanJsonConverter))] TimeSpan HeureDebut,
    [property: JsonConverter(typeof(HhMmTimeSpanJsonConverter))] TimeSpan HeureFin,
    int IdCours,
    string? NomCours,
    int IdClasse,
    string? NomClasse,
    int IdAgent,
    string? NomAgent,
    int IdAnneeScolaire,
    string? LibelleAnneeScolaire,
    string? Salle,
    bool Statut);
