namespace KelasiNaBiso.Models.DTOs.Pagination
{
    /// <summary>
    /// Résultat paginé générique pour pagination offset-based
    /// Contient les données + métadonnées de pagination
    /// </summary>
    /// <typeparam name="T">Type des éléments retournés</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Liste des données de la page actuelle
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Numéro de la page actuelle
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Nombre d'éléments par page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Nombre total de pages disponibles
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Nombre total d'enregistrements (tous éléments confondus)
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Indique s'il existe une page précédente
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// Indique s'il existe une page suivante
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;

        /// <summary>
        /// Numéro de la première ligne de la page actuelle
        /// </summary>
        public int FirstRowOnPage => TotalRecords == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        /// <summary>
        /// Numéro de la dernière ligne de la page actuelle
        /// </summary>
        public int LastRowOnPage => Math.Min(PageNumber * PageSize, TotalRecords);

        /// <summary>
        /// Constructeur vide
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Constructeur avec données
        /// </summary>
        public PagedResult(List<T> data, int count, int pageNumber, int pageSize)
        {
            Data = data;
            TotalRecords = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}

