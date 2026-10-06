using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class CommunicationDispatchWorkerSignalRTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;

        public CommunicationDispatchWorkerSignalRTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task ExecuteAsync_InAppActive_EnvoieSignalRAuDestinataire()
        {
            SeedCampaign(inApp: true, push: false);
            var signalR = new Mock<ISignalRNotificationService>();
            signalR
                .Setup(s => s.SendNotificationToUserAsync(10, It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            var worker = CreateWorker(signalR.Object);

            await worker.ExecuteAsync(1, initiatedByUserId: 99);

            signalR.Verify(
                s => s.SendNotificationToUserAsync(
                    10,
                    It.Is<Notification>(n =>
                        n.TypeNotification == "COMMUNICATION"
                        && n.IdDestinataire == 10
                        && n.IdNotification > 0)),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_InAppDesactive_NEnvoiePasSignalR()
        {
            SeedCampaign(inApp: false, push: true);
            var signalR = new Mock<ISignalRNotificationService>();
            var firebase = new Mock<IFirebaseNotificationService>();
            firebase
                .Setup(f => f.EnvoyerNotificationAUtilisateurAsync(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>()))
                .ReturnsAsync(true);

            var worker = CreateWorker(signalR.Object, firebase.Object);

            await worker.ExecuteAsync(1, initiatedByUserId: 99);

            signalR.Verify(
                s => s.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<Notification>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_SignalREchoue_CampagneResteEnvoyee()
        {
            SeedCampaign(inApp: true, push: false);
            var signalR = new Mock<ISignalRNotificationService>();
            signalR
                .Setup(s => s.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<Notification>()))
                .ThrowsAsync(new InvalidOperationException("hub down"));

            var worker = CreateWorker(signalR.Object);

            await worker.ExecuteAsync(1, initiatedByUserId: 99);

            var campaign = await _context.CommunicationCampaigns.FindAsync(1);
            campaign!.Statut.Should().Be(CampaignStatuses.Envoye);
        }

        private CommunicationDispatchWorker CreateWorker(
            ISignalRNotificationService signalR,
            IFirebaseNotificationService? firebase = null)
        {
            return new CommunicationDispatchWorker(
                _context,
                firebase ?? Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<ISmsNotificationService>(),
                signalR,
                NullLogger<CommunicationDispatchWorker>.Instance);
        }

        private void SeedCampaign(bool inApp, bool push)
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Utilisateurs.Add(TestDataBuilder.CreateUtilisateur(10, "parent@test.cd", idEcole: 1));
            _context.Utilisateurs.Add(TestDataBuilder.CreateUtilisateur(99, "admin@test.cd", idEcole: 1));
            _context.CommunicationCampaigns.Add(new CommunicationCampaign
            {
                IdCampaign = 1,
                IdEcole = 1,
                IdAuteur = 99,
                Titre = "Communiqué test",
                ContenuMarkdown = "Bonjour **classe**",
                Statut = CampaignStatuses.EnCours,
                Importance = "Info",
                ChannelsJson = $"{{\"push\":{push.ToString().ToLowerInvariant()},\"email\":false,\"sms\":false,\"inApp\":{inApp.ToString().ToLowerInvariant()}}}"
            });
            _context.CampaignRecipients.Add(new CampaignRecipient
            {
                IdRecipient = 1,
                IdCampaign = 1,
                IdUtilisateur = 10,
                Status = "Planifie",
                DatePlanifie = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        public void Dispose() => _context.Dispose();
    }
}
