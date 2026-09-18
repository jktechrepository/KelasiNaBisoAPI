using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveCompteServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly Mock<ISmsNotificationService> _smsMock;
        private readonly EleveCompteService _service;

        public EleveCompteServiceTests()
        {
            var options = new DbContextOptionsBuilder<KelasiNaBisoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new KelasiNaBisoDbContext(options);
            Seed();
            _smsMock = new Mock<ISmsNotificationService>();
            _service = new EleveCompteService(
                _context,
                _smsMock.Object,
                NullLogger<EleveCompteService>.Instance);
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            ecole.AcceptNotification = true;
            _context.Ecoles.Add(ecole);
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025"));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent", telephone: "+243900000001"));

            var withMat = TestDataBuilder.CreateEleve(1, 1, "Avec Matricule");
            withMat.Matricule = "MAT-BF-001";
            var noMat = TestDataBuilder.CreateEleve(2, 1, "Sans Matricule");
            noMat.Matricule = null;
            var already = TestDataBuilder.CreateEleve(3, 1, "Deja Compte");
            already.Matricule = "MAT-BF-003";

            _context.Eleves.AddRange(withMat, noMat, already);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100),
                TestDataBuilder.CreateInscription(3, 3, 1, 10, 100));

            var role = TestDataBuilder.CreateRole(8, "Eleve", niveau: 6);
            _context.Roles.Add(role);
            var user = TestDataBuilder.CreateUtilisateur(50, "deja@test.com");
            user.IdEleve = 3;
            user.DefaultUsername = "MAT-BF-003";
            user.IdRole = 8;
            user.IdEcole = 1;
            _context.Utilisateurs.Add(user);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateDefault_CreatesUser_WithMatriculeAndPassword()
        {
            var eleve = await _context.Eleves.FindAsync(1);
            var result = await _service.CreateDefaultEleveUserAsync(eleve!, 1);

            result.Outcome.Should().Be(EleveCompteCreateOutcome.Created);
            result.Info!.DefaultUsername.Should().Be("MAT-BF-001");
            result.Info.IdEleve.Should().Be(1);
            BCrypt.Net.BCrypt.Verify("123456", (await _context.Utilisateurs.FirstAsync(u => u.IdEleve == 1)).MotDePasseHash)
                .Should().BeTrue();
        }

        [Fact]
        public async Task CreateDefault_NoMatricule_ReturnsNoMatricule()
        {
            var eleve = await _context.Eleves.FindAsync(2);
            var result = await _service.CreateDefaultEleveUserAsync(eleve!, 1);
            result.Outcome.Should().Be(EleveCompteCreateOutcome.NoMatricule);
        }

        [Fact]
        public async Task CreateDefault_AlreadyLinked_IsIdempotent()
        {
            var eleve = await _context.Eleves.FindAsync(3);
            var result = await _service.CreateDefaultEleveUserAsync(eleve!, 1);
            result.Outcome.Should().Be(EleveCompteCreateOutcome.AlreadyLinked);
            result.Info!.IdUtilisateur.Should().Be(50);
        }

        [Fact]
        public async Task CreateDefault_ConflictUsername_WhenOtherEleveOwnsUsername()
        {
            var other = TestDataBuilder.CreateEleve(9, 1, "Conflict");
            other.Matricule = "MAT-BF-001";
            _context.Eleves.Add(other);
            await _context.SaveChangesAsync();

            // Créer d'abord le compte pour eleve 1
            await _service.CreateDefaultEleveUserAsync((await _context.Eleves.FindAsync(1))!, 1);

            var conflict = await _service.CreateDefaultEleveUserAsync(other, 1);
            conflict.Outcome.Should().Be(EleveCompteCreateOutcome.ConflictUsername);
        }

        [Fact]
        public async Task Backfill_DryRun_DoesNotPersistNewUsers()
        {
            var before = await _context.Utilisateurs.CountAsync();
            var result = await _service.BackfillByEcoleAsync(1, dryRun: true);

            result.DryRun.Should().BeTrue();
            result.Created.Should().BeGreaterThan(0); // candidat avec matricule sans compte
            result.SkippedAlreadyLinked.Should().BeGreaterThan(0);
            result.SkippedNoMatricule.Should().BeGreaterThan(0);
            (await _context.Utilisateurs.CountAsync()).Should().Be(before);
        }

        [Fact]
        public async Task Backfill_CreatesMissingAccounts()
        {
            var result = await _service.BackfillByEcoleAsync(1, dryRun: false);

            result.Created.Should().BeGreaterThan(0);
            result.SkippedAlreadyLinked.Should().BeGreaterThan(0);
            (await _context.Utilisateurs.AnyAsync(u => u.IdEleve == 1)).Should().BeTrue();
            (await _context.Utilisateurs.AnyAsync(u => u.IdEleve == 2)).Should().BeFalse();
        }

        [Fact]
        public async Task CreateDefault_SendsSmsToTuteur_WhenCreatedAndAcceptNotification()
        {
            var eleve = await _context.Eleves.FindAsync(1);
            await _service.CreateDefaultEleveUserAsync(eleve!, 1, sendSmsToTuteur: true);

            _smsMock.Verify(
                s => s.EnvoyerSmsAsync(
                    "+243900000001",
                    It.Is<string>(m => m.Contains("MAT-BF-001") && m.Contains("123456")),
                    "COMPTE_ELEVE"),
                Times.Once);
        }

        [Fact]
        public async Task CreateDefault_DoesNotSendSms_WhenSendSmsFalse()
        {
            var eleve = await _context.Eleves.FindAsync(1);
            await _service.CreateDefaultEleveUserAsync(eleve!, 1, sendSmsToTuteur: false);

            _smsMock.Verify(
                s => s.EnvoyerSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
                Times.Never);
        }

        public void Dispose() => _context.Dispose();
    }
}
