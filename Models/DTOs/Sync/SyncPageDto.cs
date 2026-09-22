namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>Page générique pull (eleves / frais-dus / agents).</summary>
    public class SyncPageDto<T>
    {
        public string Snapshot { get; set; } = string.Empty;
        public List<T> Items { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
        public string NextSince { get; set; } = string.Empty;
    }
}
