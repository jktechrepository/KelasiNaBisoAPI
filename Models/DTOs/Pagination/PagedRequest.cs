using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Pagination
{
    /// <summary>
    /// DTO pour les requêtes de pagination offset-based (classique avec numéros de pages)
    /// Idéal pour : Tableaux admin, interfaces web avec navigation par pages
    /// </summary>
    public class PagedRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 15;

        /// <summary>
        /// Numéro de la page (commence à 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Le numéro de page doit être supérieur ou égal à 1")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Nombre d'éléments par page (min: 1, max: 100, défaut: 15)
        /// </summary>
        [Range(1, 100, ErrorMessage = "La taille de page doit être entre 1 et 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : (value > 100 ? 100 : value);
        }

        /// <summary>
        /// Champ de tri (ex: "NomComplet", "DateCreation")
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Tri décroissant (DESC) si true, croissant (ASC) si false
        /// </summary>
        public bool SortDescending { get; set; } = false;

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

