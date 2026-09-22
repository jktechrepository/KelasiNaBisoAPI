using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>Paramètres query communs des endpoints pull (delta / cursor).</summary>
    public class SyncRequestDto
    {
        /// <summary>Watermark opaque (réponse bootstrap / nextSince).</summary>
        public string? Since { get; set; }

        /// <summary>Curseur opaque de pagination.</summary>
        public string? Cursor { get; set; }

        /// <summary>Taille de page (défaut 1000, max 5000).</summary>
        [Range(1, 5000)]
        public int? PageSize { get; set; }

        /// <summary>Token de cohérence de session de sync.</summary>
        public string? Snapshot { get; set; }

        public const int DefaultPageSize = 1000;
        public const int MaxPageSize = 5000;

        public int ResolvePageSize()
        {
            var size = PageSize ?? DefaultPageSize;
            if (size < 1) return DefaultPageSize;
            return size > MaxPageSize ? MaxPageSize : size;
        }
    }

    /// <summary>Query spécifique soldes / frais dus.</summary>
    public class SyncFraisDusRequestDto : SyncRequestDto
    {
        /// <summary>Si true (défaut), ne retourne que les postes avec reste dû &gt; 0.</summary>
        public bool OnlyOutstanding { get; set; } = true;
    }

    /// <summary>Query deletions — <c>since</c> obligatoire.</summary>
    public class SyncDeletionsRequestDto
    {
        [Required]
        public string Since { get; set; } = string.Empty;

        public string? Snapshot { get; set; }
    }
}
