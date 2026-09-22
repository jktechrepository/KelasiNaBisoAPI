using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Sync;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class SyncPresenceBatchServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly SyncPresenceBatchService _sut;
        private readonly DateTime _now = DateTime.Now;
        private readonly DateTime _today;

        public SyncPresenceBatchServiceTests()
        {
            _today = DateTime.Today;
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, anneeRepo);
            var presenceService = new PresenceService(
                _context,
                NullLogger<PresenceService>.Instance,
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                scope);
            var idempotency = new SyncIdempotencyService(_context);
            _sut = new SyncPresenceBatchService(
                _context,
                presenceService,
                anneeRepo,
                idempotency,
                NullLogger<SyncPresenceBatchService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-3), _now.AddMonths(6)));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));
            _context.Agents.Add(TestDataBuilder.CreateAgent(5, "Bob", idEcole: 1));
            _context.SaveChanges();
        }

        private static PresenceBatchItemDto EleveItem(string uuid, DateTime date, string heure = "07:30")
            => new()
            {
                ClientRequestId = uuid,
                IdEleve = 1,
                IsPresent = true,
                HeureArrivee = heure,
                DateDuJour = date,
                DeviceId = "ctrl-1"
            };

        [Fact]
        public async Task ProcessBatch_Eleve_CreatesAndIdempotentDuplicate()
        {
            var uuid = Guid.NewGuid().ToString();
            var request = new PresenceBatchRequestDto { Items = { EleveItem(uuid, _today) } };

            var first = await _sut.ProcessBatchAsync(1, request, 9);
            first.Summary.Created.Should().Be(1);
            first.Results[0].IdPresence.Should().NotBeNull();

            var second = await _sut.ProcessBatchAsync(1, request, 9);
            second.Summary.Duplicates.Should().Be(1);
            second.Results[0].IdPresence.Should().Be(first.Results[0].IdPresence);
            _context.Presences.Count().Should().Be(1);
        }

        [Fact]
        public async Task ProcessBatch_SecondPointageSameDay_IsRejected()
        {
            await _sut.ProcessBatchAsync(1, new PresenceBatchRequestDto
            {
                Items = { EleveItem(Guid.NewGuid().ToString(), _today) }
            }, null);

            var otherUuid = Guid.NewGuid().ToString();
            var result = await _sut.ProcessBatchAsync(1, new PresenceBatchRequestDto
            {
                Items = { EleveItem(otherUuid, _today, "08:00") }
            }, null);

            result.Results[0].Status.Should().Be(SyncStatus.Rejected);
            result.Results[0].ErrorCode.Should().Be("ALREADY_POINTED");
            result.Results[0].IdPresence.Should().NotBeNull();
            _context.Presences.Count().Should().Be(1);
        }

        [Fact]
        public async Task ProcessBatch_Agent_Creates()
        {
            var result = await _sut.ProcessBatchAsync(1, new PresenceBatchRequestDto
            {
                Items =
                {
                    new PresenceBatchItemDto
                    {
                        ClientRequestId = Guid.NewGuid().ToString(),
                        IdAgent = 5,
                        IsPresent = true,
                        HeureArrivee = "08:15",
                        DateDuJour = _today
                    }
                }
            }, null);

            result.Summary.Created.Should().Be(1);
            _context.Presences.Single().TypePresence.Should().Be("AGENT");
        }

        [Fact]
        public async Task ProcessBatch_BothEleveAndAgent_IsRejected()
        {
            var result = await _sut.ProcessBatchAsync(1, new PresenceBatchRequestDto
            {
                Items =
                {
                    new PresenceBatchItemDto
                    {
                        ClientRequestId = Guid.NewGuid().ToString(),
                        IdEleve = 1,
                        IdAgent = 5,
                        HeureArrivee = "07:00",
                        DateDuJour = _today
                    }
                }
            }, null);

            result.Results[0].Status.Should().Be(SyncStatus.Rejected);
            result.Results[0].ErrorCode.Should().Be("INVALID_TARGET");
        }

        public void Dispose() => _context.Dispose();
    }
}
