using System.Text.Json;
using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class MokoAfrikaCheckStatusTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly Mock<IMokoAfrikaGatewayClient> _gateway = new();
        private readonly Mock<IDashboardHubService> _hub = new();
        private readonly MokoAfrikaService _service;

        public MokoAfrikaCheckStatusTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 1,
                Reference = "MOKO_TEST_REF",
                IdPaiement = null,
                IdEcole = 1,
                Action = MokoActions.Debit,
                Amount = 83.6m,
                AmountNet = 80m,
                Devise = "CDF",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                GatewayTransactionId = "PD123",
                RawRequest = PayInRawRequestHelper.Serialize(
                    new PayInIntentSnapshot
                    {
                        IdEleve = 1,
                        IdFrais = 1,
                        MontantNet = 80m,
                        MontantCollecte = 83.6m,
                        MontantGatewayNet = 80m,
                        CodeDevisePrincipale = "CDF",
                        CodeDevisePaiement = "CDF",
                        ModePaiement = "Mobile Money",
                        OperateurMobileMoney = "mpesa"
                    },
                    new Dictionary<string, object?>()),
                DateCreation = DateTime.Now
            });
            _context.SaveChanges();

            var settings = Options.Create(new MokoSettings
            {
                MerchantId = "m",
                MerchantCode = "c",
                SecretKey = "s",
                PayInUssdWindowSeconds = 120
            });

            _service = new MokoAfrikaService(
                _context,
                _gateway.Object,
                Mock.Of<IMokoFeeCalculator>(),
                Mock.Of<IMokoWalletService>(),
                Mock.Of<IEcolePaiementMobileService>(),
                Mock.Of<IPaiementRepository>(),
                _hub.Object,
                settings,
                NullLogger<MokoAfrikaService>.Instance);
        }

        private void SetupGatewayBody(string json, Action<MokoGatewayResponse>? configure = null)
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var response = new MokoGatewayResponse
            {
                HttpStatusCode = 200,
                RawBody = json,
                Parsed = doc,
                IsSuccess = MokoGatewayResponseParser.IsDefinitiveSuccess(root),
                IsPending = MokoGatewayResponseParser.IsPending(root),
                IsFailure = MokoGatewayResponseParser.IsDefinitiveFailure(root),
                Status = MokoGatewayResponseParser.GetString(root, "Status", "trans_status", "status"),
                TransactionId = MokoGatewayResponseParser.GetString(root, "Transaction_id", "transaction_id"),
                ErrorMessage = MokoGatewayResponseParser.GetString(root, "Comment", "resultCodeErrorDescription")
            };
            configure?.Invoke(response);
            _gateway
                .Setup(g => g.SendAsync(It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithoutResultCodeError_WithinUssdWindow_KeepsPending()
        {
            // DateCreation = Now (ctor) → dans la fenêtre USSD.
            SetupGatewayBody("{\"Status\":\"Error\",\"Comment\":\"Waiting customer\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Pending);
            dto.IsDefinitive.Should().BeFalse();
            dto.GatewayStatusRaw.Should().Be("Error");
            dto.HasCallback.Should().BeFalse();
            (await _context.Paiements.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithoutResultCodeError_OutsideUssdWindow_MarksError()
        {
            var tx = await _context.TransactionsMoko.SingleAsync(t => t.Reference == "MOKO_TEST_REF");
            tx.DateCreation = DateTime.Now.AddMinutes(-3);
            await _context.SaveChangesAsync();

            SetupGatewayBody("{\"Status\":\"Error\",\"Comment\":\"Still Error after USSD\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();
            dto.GatewayStatusRaw.Should().Be("Error");
            dto.ResultCodeErrorDescription.Should().Contain("Still Error");
            (await _context.Paiements.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithResultCodeError_WithinUssdWindow_KeepsPending()
        {
            // DateCreation = Now (ctor) → dans la fenêtre ; resultCodeError technique ne doit pas tuer.
            SetupGatewayBody("{\"Status\":\"Error\",\"resultCodeError\":\"404\",\"resultCodeErrorDescription\":\"not found\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Pending);
            dto.IsDefinitive.Should().BeFalse();
            (await _context.Paiements.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CheckStatus_Cancelled_WithinUssdWindow_MarksError()
        {
            SetupGatewayBody("{\"Status\":\"cancelled\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(
                    1,
                    It.Is<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>(n =>
                        n.Reference == "MOKO_TEST_REF"
                        && n.StatutGateway == MokoTransactionStatuses.Error
                        && n.IdEleve == 1)),
                Times.Once);
        }

        [Fact]
        public async Task CheckStatus_TransStatusFailed_WithinUssdWindow_MarksError_AndNotifies()
        {
            SetupGatewayBody("{\"Trans_Status\":\"Failed\",\"Comment\":\"Customer cancelled\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();
            dto.GatewayStatusRaw.Should().Be("Failed");
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(1, It.IsAny<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckStatus_AmbiguousFailureWithinUssdWindow_KeepsPending()
        {
            // Status Failed sans Trans_Status = soft ; ne doit pas créer de Paiement ni SignalR failed
            SetupGatewayBody("{\"Status\":\"Failed\",\"Comment\":\"USSD pending\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Pending);
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(It.IsAny<int>(), It.IsAny<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>()),
                Times.Never);
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithResultCodeError_OutsideUssdWindow_MarksError()
        {
            var tx = await _context.TransactionsMoko.SingleAsync(t => t.Reference == "MOKO_TEST_REF");
            tx.DateCreation = DateTime.Now.AddMinutes(-3);
            await _context.SaveChangesAsync();

            SetupGatewayBody("{\"Status\":\"Error\",\"resultCodeError\":\"404\",\"resultCodeErrorDescription\":\"not found\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();
            (await _context.Paiements.CountAsync()).Should().Be(0);
            _hub.Verify(
                h => h.NotifyPayInFailedAsync(1, It.IsAny<KelasiNaBiso.Models.DTOs.MokoAfrika.PayInSignalRNotification>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckStatus_SendsActionVerify_NotCheck()
        {
            Dictionary<string, object?>? captured = null;
            _gateway
                .Setup(g => g.SendAsync(It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
                .Callback<Dictionary<string, object?>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(new MokoGatewayResponse
                {
                    HttpStatusCode = 200,
                    RawBody = "{\"Trans_Status\":\"Pending\",\"resultCode\":\"0\"}",
                    IsPending = true,
                    Status = "Pending"
                });

            await _service.CheckStatusAsync("MOKO_TEST_REF");

            captured.Should().NotBeNull();
            captured!["action"].Should().Be("verify");
            captured["action"].Should().NotBe("check");
            captured["reference"].Should().Be("MOKO_TEST_REF");
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithResultCodeError408_OutsideUssdWindow_MarksError()
        {
            var tx = await _context.TransactionsMoko.SingleAsync(t => t.Reference == "MOKO_TEST_REF");
            tx.DateCreation = DateTime.Now.AddMinutes(-3);
            await _context.SaveChangesAsync();

            SetupGatewayBody("{\"Status\":\"Error\",\"resultCodeError\":\"408\",\"resultCodeErrorDescription\":\"action not recognized by the system...\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();
            dto.ResultCodeError.Should().Be("408");
        }

        [Fact]
        public async Task CheckStatus_StatusSuccess_SetsSuccess()
        {
            SetupGatewayBody("{\"Status\":\"Success\",\"Transaction_id\":\"PD999\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Success);
            dto.IsDefinitive.Should().BeTrue();
            dto.GatewayTransactionId.Should().Be("PD999");
        }

        [Fact]
        public async Task CheckStatus_TransStatusSuccessful_SetsSuccess()
        {
            SetupGatewayBody("{\"Comment\":\"Transaction Found\",\"Trans_Status\":\"Successful\",\"Transaction_id\":\"PD888\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Success);
            dto.IsDefinitive.Should().BeTrue();
            dto.GatewayTransactionId.Should().Be("PD888");
            dto.GatewayStatusRaw.Should().Be("Successful");
        }

        [Fact]
        public async Task CheckStatus_ResultCodeZero_KeepsPending()
        {
            SetupGatewayBody("{\"resultCode\":\"0\",\"Comment\":\"Transaction Received\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Pending);
            dto.IsDefinitive.Should().BeFalse();
        }

        public void Dispose() => _context.Dispose();
    }
}
