using KelasiNaBiso.Data;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services.MokoAfrika
{
    public class MokoPayoutWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MokoPayoutWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        public MokoPayoutWorker(IServiceScopeFactory scopeFactory, ILogger<MokoPayoutWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MokoPayoutWorker démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TraiterFilePayOutAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur cycle MokoPayoutWorker");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task TraiterFilePayOutAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            var orchestrator = scope.ServiceProvider.GetRequiredService<IPaiementMokoOrchestrator>();

            var now = DateTime.Now;
            var pendingIds = await context.FilePayoutsMoko
                .Where(f => f.Status == MokoPayoutQueueStatuses.Pending && f.ScheduledAt <= now)
                .OrderBy(f => f.ScheduledAt)
                .Take(10)
                .Select(f => f.IdFilePayoutMoko)
                .ToListAsync(cancellationToken);

            foreach (var id in pendingIds)
            {
                try
                {
                    _logger.LogInformation("Traitement PayOut file {Id}", id);
                    await orchestrator.ExecuterPayOutFileAsync(id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Échec traitement PayOut file {Id}", id);
                }
            }
        }
    }
}
