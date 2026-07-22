namespace KelasiNaBiso.Services.Repositories
{
    /// <summary>
    /// Abstraction d'un planificateur de tâches pour l'envoi des campagnes de communication.
    /// Permet d'intégrer plus tard Hangfire, Azure Queue, etc.
    /// </summary>
    public interface ICommunicationDispatchScheduler
    {
        Task EnqueueDispatchAsync(int idCampaign, int initiatedByUserId, CancellationToken cancellationToken = default);
    }
}

