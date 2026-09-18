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
        private readonly Mock<KelasiNaBisoAPI.Services.Repositories.IDashboardHubService> _hub = new();
        private readonly MokoCallbackHandler _handler;

        public MokoCallbackHandlerSoftErrorTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 20,
                Reference = "MOKO_20260904141445_8948",
                IdPaiement = null,
                IdEcole = 13,
                Action = MokoActions.Debit,
                Amount = 10.45m,
                AmountNet = 10m,
                Devise = "CDF",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                GatewayTransactionId = "PD20260904XA3HE0Z9Q059G",
                RawRequest = PayInRawRequestHelper.Serialize(
                    new PayInIntentSnapshot
                    {
                        IdEleve = 1746,
                        IdFrais = 101,
                        MontantNet = 10m,
                        MontantCollecte = 10.45m,
                        MontantGatewayNet = 10m,
                        CodeDevisePrincipale = "CDF",
                        CodeDevisePaiement = "CDF",
                        ModePaiement = "Mobile Money",
                        OperateurMobileMoney = "mpesa"
                    },
                    new Dictionary<string, object?> { ["reference"] = "MOKO_20260904141445_8948" }),
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
                _hub.Object,
                settings,
                NullLogger<MokoCallbackHandler>.Instance);
        }

        [Fact]
        public async Task SoftErrorWithoutResultCodeError_KeepsPending_DoesNotCreatePaiement()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"Error\",\"Comment\":\"Error\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("pending");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Pending);
            tx.RawCallback.Should().Contain("Error");

            (await _context.Paiements.CountAsync()).Should().Be(0);

            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HardFailureWithResultCodeError_MarksTxError_NoPaiement()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"Error\",\"resultCodeError\":\"404\",\"resultCodeErrorDescription\":\"not found\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("échec");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Error);
            (await _context.Paiements.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CancelledStatus_MarksTxError_NoPaiement_AndNotifiesFailed()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"cancelled\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Error);
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(
                    13,
                    It.Is<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>(n =>
                        n.Reference == "MOKO_20260904141445_8948"
                        && n.StatutGateway == MokoTransactionStatuses.Error
                        && n.IdEleve == 1746)),
                Times.Once);
        }

        [Fact]
        public async Task TransStatusFailed_MarksTxError_NoPaiement_AndNotifiesFailed()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Trans_Status\":\"Failed\",\"Comment\":\"USSD cancelled\"}";

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("échec");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Error);
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(13, It.IsAny<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>()),
                Times.Once);
        }

        [Fact]
        public async Task SoftErrorWithoutResultCodeError_DoesNotNotifyFailed()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"Error\",\"Comment\":\"Error\"}";

            await _handler.HandleAsync(body, signature: null);

            _hub.Verify(
                h => h.NotifyPayInFailedAsync(It.IsAny<int>(), It.IsAny<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>()),
                Times.Never);
        }

        [Fact]
        public async Task SuccessStatus_ConfirmsPayInByReference()
        {
            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"success\"}";

            _mokoService
                .Setup(s => s.ConfirmerPayInEtNotifierAsync(
                    "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(999);

            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("succès");

            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status.Should().Be(MokoTransactionStatuses.Success);

            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(
                    "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SuccessWhenTxAlreadySuccessWithoutIdPaiement_RetriesConfirm()
        {
            var tx = await _context.TransactionsMoko.FirstAsync(t => t.IdTransactionMoko == 20);
            tx.Status = MokoTransactionStatuses.Success;
            tx.IdPaiement = null;
            await _context.SaveChangesAsync();

            _mokoService
                .Setup(s => s.ConfirmerPayInEtNotifierAsync(
                    "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1001);

            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"trans_status\":\"SUCCESS\"}";
            var result = await _handler.HandleAsync(body, signature: null);

            result.Accepted.Should().BeTrue();
            result.Message.Should().Contain("succès");
            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(
                    "MOKO_20260904141445_8948", It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task MissingSignature_WhenHmacConfigured_Rejects()
        {
            var secure = new MokoCallbackHandler(
                _context,
                _mokoService.Object,
                Mock.Of<IPaiementMokoOrchestrator>(),
                Mock.Of<IMokoWalletService>(),
                Mock.Of<KelasiNaBisoAPI.Services.Repositories.IDashboardHubService>(),
                Options.Create(new MokoSettings { HmacKey = "test-hmac-key" }),
                NullLogger<MokoCallbackHandler>.Instance);

            var body = "{\"reference\":\"MOKO_20260904141445_8948\",\"Status\":\"success\"}";
            var result = await secure.HandleAsync(body, signature: null);

            result.Accepted.Should().BeFalse();
            result.Message.Should().Contain("Signature");
            _mokoService.Verify(
                s => s.ConfirmerPayInEtNotifierAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        public void Dispose() => _context.Dispose();
    }
}
