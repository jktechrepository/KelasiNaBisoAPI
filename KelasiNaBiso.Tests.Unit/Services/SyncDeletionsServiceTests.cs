using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Sync;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class SyncDeletionsServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly SyncDeletionsService _sut;
        private readonly DateTime _now = DateTime.Now;

        public SyncDeletionsServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new SyncDeletionsService(_context, new SyncIdempotencyService(_context));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Y", _now.AddMonths(-2), _now.AddMonths(6)));
            var actif = TestDataBuilder.CreateEleve(1, null, "Actif");
            var inactif = TestDataBuilder.CreateEleve(2, null, "Inactif");
            inactif.Statut = false;
            _context.Eleves.AddRange(actif, inactif);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100));

            var fraisInactif = TestDataBuilder.CreateFrais(9, 1, 100, "Inactif");
            fraisInactif.Statut = false;
            _context.Frais.Add(fraisInactif);

            _context.AuditLogs.AddRange(
                new AuditLog
                {
                    TableName = "Eleve",
                    RecordId = 50,
                    Action = "DELETE",
                    UserId = 1,
                    UserName = "admin",
                    IdEcole = 1,
                    DateAction = _now.AddMinutes(-5),
                    Success = true
                },
                new AuditLog
                {
                    TableName = "Paiement",
                    RecordId = 77,
                    Action = "DELETE",
                    UserId = 1,
                    UserName = "admin",
                    IdEcole = 1,
                    DateAction = _now.AddMinutes(-3),
                    Success = true
                },
                new AuditLog
                {
                    TableName = "Eleve",
                    RecordId = 3,
                    Action = "UPDATE",
                    UserId = 1,
                    UserName = "admin",
                    IdEcole = 1,
                    DateAction = _now.AddMinutes(-2),
                    ChangedFields = "Statut",
                    NewValues = "{\"Statut\":false}",
                    Success = true
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetDeletions_RequiresValidSince()
        {
            var act = () => _sut.GetDeletionsAsync(1, new SyncDeletionsRequestDto { Since = "" });
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetDeletions_ReturnsAuditDeletesAndSoftStatus()
        {
            var since = _idempotencyWatermarkHoursAgo(1);
            var dto = await _sut.GetDeletionsAsync(1, new SyncDeletionsRequestDto { Since = since });

            dto.DeletedEleveIds.Should().Contain(50);
            dto.DeletedPaymentIds.Should().Contain(77);
            dto.DeactivatedEleveIds.Should().Contain(2); // soft statut
            dto.DeactivatedEleveIds.Should().Contain(3); // audit UPDATE
            dto.RemovedFraisIds.Should().Contain(9);
            dto.NextSince.Should().NotBeNullOrWhiteSpace();
            dto.Snapshot.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GetDeletions_IgnoresAuditOlderThanSince()
        {
            var since = new SyncIdempotencyService(_context).CreateWatermark(_now.AddMinutes(-1).ToUniversalTime());
            // Force watermark in the future relative to older audits: use now+1h
            since = new SyncIdempotencyService(_context).CreateWatermark(DateTime.UtcNow.AddHours(1));

            var dto = await _sut.GetDeletionsAsync(1, new SyncDeletionsRequestDto { Since = since });

            dto.DeletedEleveIds.Should().BeEmpty();
            dto.DeletedPaymentIds.Should().BeEmpty();
            // Soft reconciliation still returns inactive eleves/frais
            dto.DeactivatedEleveIds.Should().Contain(2);
            dto.RemovedFraisIds.Should().Contain(9);
        }

        private string _idempotencyWatermarkHoursAgo(int hours)
            => new SyncIdempotencyService(_context).CreateWatermark(DateTime.UtcNow.AddHours(-hours));

        public void Dispose() => _context.Dispose();
    }
}
