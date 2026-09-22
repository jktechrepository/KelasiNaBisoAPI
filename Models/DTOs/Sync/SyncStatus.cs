namespace KelasiNaBiso.Models.DTOs.Sync
{
    /// <summary>Statuts d'item de batch offline (contrat portable).</summary>
    public static class SyncStatus
    {
        public const string Created = "created";
        public const string Duplicate = "duplicate";
        public const string Rejected = "rejected";
        public const string Error = "error";
    }

    /// <summary>Types de ressources journalisées dans SyncClientRequests.</summary>
    public static class SyncResourceType
    {
        public const string Presence = "presence";
        public const string Payment = "payment";
    }
}
