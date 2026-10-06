using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class InscriptionGetByEcoleSearchTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly InscriptionService _service;
        private readonly DateTime _now = DateTime.Now;

        public InscriptionGetByEcoleSearchTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new InscriptionService(
                _context,
                new ConfigurationBuilder().Build(),
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<ISignalRNotificationService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<InscriptionService>.Instance,
                resolver,
                scope,
                new EleveCompteService(_context, Mock.Of<ISmsNotificationService>(), NullLogger<EleveCompteService>.Instance));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6), statut: true));
            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, null, "Alpha"),
                TestDataBuilder.CreateEleve(2, null, "Beta"));
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 101),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 101));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEcoleAsync_WithoutSearchTerm_ReturnsAllCurrentYear()
        {
            var result = await _service.GetByEcoleAsync(1);

            result.Data.Should().HaveCount(2);
            result.Data.Select(i => i.IdEleve).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetByEcoleAsync_SearchTerm_FiltersByEleveNom()
        {
            var result = await _service.GetByEcoleAsync(1, searchTerm: "Alpha");

            result.Data.Should().ContainSingle();
            result.Data.Single().IdEleve.Should().Be(1);
        }

        [Fact]
        public async Task GetByEcoleAsync_SearchTerm_NoMatch_ReturnsEmpty()
        {
            var result = await _service.GetByEcoleAsync(1, searchTerm: "Gamma");

            result.Data.Should().BeEmpty();
        }

        public void Dispose() => _context.Dispose();
    }
}
