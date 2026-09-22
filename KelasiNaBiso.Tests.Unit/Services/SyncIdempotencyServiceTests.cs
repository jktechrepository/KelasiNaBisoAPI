using FluentAssertions;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Services.Sync;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class SyncIdempotencyServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly SyncIdempotencyService _sut;

        public SyncIdempotencyServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new SyncIdempotencyService(_context);
        }

        [Fact]
        public async Task TryRegisterCreated_FirstCall_IsCreated()
        {
            var clientRequestId = Guid.NewGuid().ToString();

            var outcome = await _sut.TryRegisterCreatedAsync(
                idEcole: 1,
                clientRequestId: clientRequestId,
                resourceType: SyncResourceType.Presence,
                resourceId: 42,
                message: "Présence créée");

            outcome.IsDuplicate.Should().BeFalse();
            outcome.Record.Status.Should().Be(SyncStatus.Created);
            outcome.Record.ResourceId.Should().Be(42);

            var item = outcome.ToBatchItemResult();
            item.Status.Should().Be(SyncStatus.Created);
            item.ClientRequestId.Should().Be(clientRequestId);
            item.ResourceId.Should().Be(42);

            (await _context.SyncClientRequests.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task TryRegisterCreated_SameUuidSameEcole_IsDuplicate()
        {
            var clientRequestId = Guid.NewGuid().ToString();

            await _sut.TryRegisterCreatedAsync(1, clientRequestId, SyncResourceType.Presence, 10);
            var second = await _sut.TryRegisterCreatedAsync(1, clientRequestId, SyncResourceType.Presence, 99);

            second.IsDuplicate.Should().BeTrue();
            second.Record.ResourceId.Should().Be(10);

            var item = second.ToBatchItemResult();
            item.Status.Should().Be(SyncStatus.Duplicate);
            item.ResourceId.Should().Be(10);

            (await _context.SyncClientRequests.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task TryRegisterCreated_SameUuidOtherEcole_IsIndependent()
        {
            var clientRequestId = Guid.NewGuid().ToString();

            await _sut.TryRegisterCreatedAsync(1, clientRequestId, SyncResourceType.Payment, 1);
            var otherEcole = await _sut.TryRegisterCreatedAsync(2, clientRequestId, SyncResourceType.Payment, 2);

            otherEcole.IsDuplicate.Should().BeFalse();
            otherEcole.Record.IdEcole.Should().Be(2);
            (await _context.SyncClientRequests.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task TryRegisterRejected_ThenReplay_IsDuplicate()
        {
            var clientRequestId = Guid.NewGuid().ToString();

            var first = await _sut.TryRegisterRejectedAsync(
                1,
                clientRequestId,
                SyncResourceType.Payment,
                "Méthode Moko refusée offline",
                errorCode: "MOKO_NOT_ALLOWED");

            first.IsDuplicate.Should().BeFalse();
            first.Record.Status.Should().Be(SyncStatus.Rejected);

            var replay = await _sut.TryRegisterCreatedAsync(1, clientRequestId, SyncResourceType.Payment, 7);
            replay.IsDuplicate.Should().BeTrue();
            replay.ToBatchItemResult().Status.Should().Be(SyncStatus.Duplicate);
            replay.Record.Status.Should().Be(SyncStatus.Rejected);
        }

        [Fact]
        public void CreateWatermark_IsOpaqueAndStableFormat()
        {
            var fixedNow = new DateTime(2026, 9, 22, 8, 0, 0, DateTimeKind.Utc);
            var wm = _sut.CreateWatermark(fixedNow);

            wm.Should().StartWith("2026-09-22T08:00:00.0000000Z_");
            wm.Should().Contain(fixedNow.Ticks.ToString());
        }

        [Fact]
        public void SyncBatchResult_BuildSummary_CountsStatuses()
        {
            var results = new List<SyncBatchItemResultDto>
            {
                new() { ClientRequestId = "a", Status = SyncStatus.Created },
                new() { ClientRequestId = "b", Status = SyncStatus.Duplicate },
                new() { ClientRequestId = "c", Status = SyncStatus.Rejected },
                new() { ClientRequestId = "d", Status = SyncStatus.Error },
                new() { ClientRequestId = "e", Status = SyncStatus.Created }
            };

            var summary = SyncBatchResultDto.BuildSummary(results);
            summary.Total.Should().Be(5);
            summary.Created.Should().Be(2);
            summary.Duplicates.Should().Be(1);
            summary.Rejected.Should().Be(1);
            summary.Errors.Should().Be(1);
        }

        public void Dispose() => _context.Dispose();
    }
}
