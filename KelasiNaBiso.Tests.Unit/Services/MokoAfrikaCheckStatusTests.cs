using System.Text.Json;
using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.MokoAfrika;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
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
        private readonly MokoAfrikaService _service;

        public MokoAfrikaCheckStatusTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 10,
                IdEleve = 1,
                IdFrais = 1,
                Montant = 80,
                Devise = "CDF",
                ModePaiement = "Mobile Money",
                StatutPaiement = "En attente",
                Statut = true,
                DatePaiement = DateTime.Now,
                DateCreation = DateTime.Now
            });
            _context.TransactionsMoko.Add(new TransactionMoko
            {
                IdTransactionMoko = 1,
                Reference = "MOKO_TEST_REF",
                IdPaiement = 10,
                IdEcole = 1,
                Action = MokoActions.Debit,
                Amount = 83.6m,
                AmountNet = 80m,
                Devise = "CDF",
                Method = "mpesa",
                Status = MokoTransactionStatuses.Pending,
                GatewayTransactionId = "PD123",
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
                Mock.Of<IDashboardHubService>(),
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
        public async Task CheckStatus_StatusErrorWithoutResultCodeError_KeepsPending()
        {
            SetupGatewayBody("{\"Status\":\"Error\",\"Comment\":\"Waiting customer\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Pending);
            dto.IsDefinitive.Should().BeFalse();
            dto.StatusDescription.Should().NotBeNullOrEmpty();

            var paiement = await _context.Paiements.FindAsync(10);
            paiement!.StatutPaiement.Should().Be("En attente");
        }

        [Fact]
        public async Task CheckStatus_StatusErrorWithResultCodeError_MarksEchoue()
        {
            SetupGatewayBody("{\"Status\":\"Error\",\"resultCodeError\":\"404\",\"resultCodeErrorDescription\":\"not found\"}");

            var dto = await _service.CheckStatusAsync("MOKO_TEST_REF");

            dto!.Status.Should().Be(MokoTransactionStatuses.Error);
            dto.IsDefinitive.Should().BeTrue();

            var paiement = await _context.Paiements.FindAsync(10);
            paiement!.StatutPaiement.Should().Be("Echoue");
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
