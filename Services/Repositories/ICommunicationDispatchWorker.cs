namespace KelasiNaBiso.Services.Repositories
{
    public interface ICommunicationDispatchWorker
    {
        Task ExecuteAsync(int idCampaign, int initiatedByUserId, CancellationToken cancellationToken = default);
    }
}

