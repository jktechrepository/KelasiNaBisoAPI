using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Services.Sync;
using KelasiNaBiso.Services.Tarif;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class SyncPaymentBatchServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly SyncPaymentBatchService _sut;
        private readonly DateTime _now = DateTime.Now;

        public SyncPaymentBatchServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, anneeRepo);
            var paiementService = new PaiementService(
                _context,
                NullLogger<PaiementService>.Instance,
                Mock.Of<ICacheService>(),
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                scope);
            var idempotency = new SyncIdempotencyService(_context);
            var fraisDu = new FraisDuCalculator(_context);
            _sut = new SyncPaymentBatchService(
                _context,
                paiementService,
                anneeRepo,
                idempotency,
                fraisDu,
                NullLogger<SyncPaymentBatchService>.Instance);
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
            _context.Frais.Add(TestDataBuilder.CreateFrais(1, 1, 100, "Minerval", PorteeFrais.Direction, 100));
            _context.FraisDirections.Add(TestDataBuilder.CreateFraisDirection(1, 1));
            _context.SaveChanges();
        }

        private static PaymentBatchItemDto Item(
            string uuid,
            string methode = "Cash",
            decimal montant = 40m,
            int idEleve = 1,
            int idFrais = 1)
            => new()
            {
                ClientRequestId = uuid,
                IdEleve = idEleve,
                IdFrais = idFrais,
                MontantPaye = montant,
                DatePaiementUtc = DateTime.UtcNow.AddHours(-2),
                MethodePaiement = methode,
                DeviceId = "test-device",
                CodeDevisePaiement = "USD"
            };

        [Fact]
        public async Task ProcessBatch_Cash_CreatesPaymentAndIdempotentDuplicate()
        {
            var uuid = Guid.NewGuid().ToString();
            var request = new PaymentBatchRequestDto { Items = { Item(uuid, "Espèces", 40m) } };

            var first = await _sut.ProcessBatchAsync(1, request, idUtilisateur: 9);
            first.Summary.Created.Should().Be(1);
            first.Results[0].Status.Should().Be(SyncStatus.Created);
            first.Results[0].IdPaiement.Should().NotBeNull();
            first.Results[0].NewMontantDu.Should().Be(60m);

            var second = await _sut.ProcessBatchAsync(1, request, idUtilisateur: 9);
            second.Summary.Duplicates.Should().Be(1);
            second.Results[0].Status.Should().Be(SyncStatus.Duplicate);
            second.Results[0].IdPaiement.Should().Be(first.Results[0].IdPaiement);

            _context.Paiements.Count(p => p.IdEleve == 1 && p.IdFrais == 1).Should().Be(1);
        }

        [Fact]
        public async Task ProcessBatch_MobileMoney_IsRejected()
        {
            var uuid = Guid.NewGuid().ToString();
            var result = await _sut.ProcessBatchAsync(1, new PaymentBatchRequestDto
            {
                Items = { Item(uuid, "Mobile Money", 10m) }
            }, null);

            result.Summary.Rejected.Should().Be(1);
            result.Results[0].ErrorCode.Should().Be(SyncCashPaymentMethods.ErrorMokoNotAllowed);

            var replay = await _sut.ProcessBatchAsync(1, new PaymentBatchRequestDto
            {
                Items = { Item(uuid, "Cash", 10m) }
            }, null);
            replay.Results[0].Status.Should().Be(SyncStatus.Duplicate);
            _context.Paiements.Count().Should().Be(0);
        }

        [Fact]
        public async Task ProcessBatch_UnknownMethod_IsRejected()
        {
            var result = await _sut.ProcessBatchAsync(1, new PaymentBatchRequestDto
            {
                Items = { Item(Guid.NewGuid().ToString(), "Crypto", 10m) }
            }, null);

            result.Results[0].Status.Should().Be(SyncStatus.Rejected);
            result.Results[0].ErrorCode.Should().Be(SyncCashPaymentMethods.ErrorMethodNotAllowed);
        }

        [Fact]
        public async Task ProcessBatch_EleveNotInSchool_IsRejected()
        {
            _context.Eleves.Add(TestDataBuilder.CreateEleve(99, null, "Orphelin"));
            await _context.SaveChangesAsync();

            var result = await _sut.ProcessBatchAsync(1, new PaymentBatchRequestDto
            {
                Items = { Item(Guid.NewGuid().ToString(), "Cash", 10m, idEleve: 99) }
            }, null);

            result.Results[0].Status.Should().Be(SyncStatus.Rejected);
            result.Results[0].ErrorCode.Should().Be("ELEVE_NOT_IN_SCHOOL");
        }

        [Theory]
        [InlineData("Cash", "Cash")]
        [InlineData("Espèces", "Cash")]
        [InlineData("Chèque", "Chèque")]
        [InlineData("Virement", "Virement")]
        public void TryNormalize_CashLike_Succeeds(string input, string expected)
        {
            SyncCashPaymentMethods.TryNormalize(input, out var mode, out _, out _).Should().BeTrue();
            mode.Should().Be(expected);
        }

        public void Dispose() => _context.Dispose();
    }
}
