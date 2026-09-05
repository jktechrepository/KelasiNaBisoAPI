using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class MokoCallbackHandlerSoftErrorTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly Mock<IMokoAfrikaService> _mokoService = new();
        private readonly MokoCallbackHandler _handler;

        public MokoCallbackHandlerSoftErrorTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 803,
                IdEleve = 1746,
                IdFrais = 101,
                Montant = 10,
                Devise = "CDF",
                ModePaiement = "Mobile Money",
                StatutPaiement = "En attente",
                Statut = true,
                DatePaiement = DateTime.Now,
                DateCreation = DateTime.Now
            });
            _context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 20,
                Reference = "MOKO_20260904141445_8948",
                IdPaiement = 803,
                IdEcole = 13,
                Action = MokoActions.Debit,
                Amount = 10.45m,
                AmountNet = 10m,
                Devise = "CDF",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                GatewayTransactionId = "PD20260904XA3HE0Z9Q059G",
                DateCreation = DateTime.Now
            });
            _context.SaveChanges();

            // HMAC vide → VerifySignature accepte sans header
            var settings = Options.Create(new MokoSettings { HmacKey = "" });

            _handler = new MokoCallbackHandler(
                _context,
                _mokoService.Object,
                Mock.Of<IPaiementMokoOrchestrator>(),
                Mock.Of<IMokoWalletService>(),
                settings,
                NullLogger<MokoCallbackHandler>.Instance);
        }

        [Fact]
        public async Task SoftErrorWithoutResultCodeError_KeepsPending_DoesNotMarkEchoue()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"Error\",\"Comment\":\"Error\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("pending");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Pending);
            tx.RawCallback.Should().Contain("Error");

            var paiement = await _context.Paiements.FirstAsync(p => p.IdPaiement == 803);
            paiement.StatutPaiement.Should().Be("En attente");

            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HardFailureWithResultCodeError_MarksEchoue()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"Error\",\"resultCodeError\":\"404\",\"resultCodeErrorDescription\":\"not found\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("échec");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Error);

            var paiement = await _context.Paiements.FirstAsync(p => p.IdPaiement == 803);
            paiement.StatutPaiement.Should().Be("Echoue");
        }

        [Fact]
        public async Task CancelledStatus_MarksEchoue()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"cancelled\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Error);

            var paiement = await _context.Paiements.FirstAsync(p => p.IdPaiement == 803);
            paiement.StatutPaiement.Should().Be("Echoue");
        }

        [Fact]
        public async Task SuccessStatus_ConfirmsPayIn()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"success\"}";

            _mokoService
                .Setup(s => s.ConfirmerPayInEtNotifierAsync(803, "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("succès");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Success);

            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(803, "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public void Dispose() => _context.Dispose();
    }
}
