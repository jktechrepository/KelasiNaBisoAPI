namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Résultat léger d'une recherche de tuteur par nom complet.
    /// </summary>
    public sealed record TuteurRechercheDto(
        int IdTuteur,
        string? NomComplet,
        string? Genre,
        string? Email,
        string? Telephone,
        bool? Statut);
}
