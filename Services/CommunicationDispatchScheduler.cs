using KelasiNaBiso.Services.Repositories;

namespace KelasiNaBiso.Services
{
    public class CommunicationDispatchScheduler : ICommunicationDispatchScheduler
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CommunicationDispatchScheduler> _logger;

        public CommunicationDispatchScheduler(
            IServiceScopeFactory scopeFactory,
            ILogger<CommunicationDispatchScheduler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task EnqueueDispatchAsync(int idCampaign, int initiatedByUserId, CancellationToken cancellationToken = default)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var worker = scope.ServiceProvider.GetRequiredService<ICommunicationDispatchWorker>();
                    await worker.ExecuteAsync(idCampaign, initiatedByUserId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'exécution en arrière-plan de la campagne {IdCampaign}", idCampaign);
                }
            });

            return Task.CompletedTask;
        }
    }
}

