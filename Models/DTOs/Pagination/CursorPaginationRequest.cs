using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Pagination
{
    /// <summary>
    /// DTO pour les requêtes de pagination cursor-based (scroll infini)
    /// Idéal pour : Apps mobiles, feeds, listes infinies
    /// Performances constantes même avec millions d'enregistrements
    /// </summary>
    public class CursorPaginationRequest
    {
        private int _limit = 15;

        /// <summary>
        /// Curseur de pagination (ID ou timestamp du dernier élément chargé)
        /// NULL pour la première page
        /// </summary>
        public string? Cursor { get; set; }

        /// <summary>
        /// Nombre d'éléments à retourner (min: 1, max: 100, défaut: 15)
        /// </summary>
        [Range(1, 100, ErrorMessage = "La limite doit être entre 1 et 100")]
        public int Limit
        {
            get => _limit;
            set => _limit = value < 1 ? 1 : (value > 100 ? 100 : value);
        }

        /// <summary>
        /// Terme de recherche pour filtrage
        /// </summary>
        [MaxLength(200, ErrorMessage = "Le terme de recherche ne peut pas dépasser 200 caractères")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Inclure les éléments désactivés (Statut = false)
        /// </summary>
        public bool IncludeInactive { get; set; } = false;
    }
}

