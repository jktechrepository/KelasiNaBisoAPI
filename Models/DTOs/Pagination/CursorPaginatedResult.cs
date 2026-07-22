namespace KelasiNaBiso.Models.DTOs.Pagination
{
    /// <summary>
    /// Résultat paginé générique pour pagination cursor-based
    /// Optimisé pour scroll infini et performances constantes
    /// </summary>
    /// <typeparam name="T">Type des éléments retournés</typeparam>
    public class CursorPaginatedResult<T>
    {
        /// <summary>
        /// Liste des données chargées
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Curseur pour charger la page suivante
        /// NULL s'il n'y a plus de données
        /// </summary>
        public string? NextCursor { get; set; }

        /// <summary>
        /// Indique s'il reste des données à charger
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// Nombre d'éléments retournés dans cette réponse
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Timestamp de génération de la réponse (pour debugging)
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Constructeur vide
        /// </summary>
        public CursorPaginatedResult()
        {
        }

        /// <summary>
        /// Constructeur avec données
        /// </summary>
        public CursorPaginatedResult(List<T> data, string? nextCursor, bool hasMore)
        {
            Data = data;
            NextCursor = nextCursor;
            HasMore = hasMore;
            Count = data.Count;
        }
    }
}

