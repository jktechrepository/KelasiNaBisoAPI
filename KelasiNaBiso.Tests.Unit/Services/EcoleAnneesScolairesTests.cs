using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EcoleAnneesScolairesTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EcoleService _service;
        private readonly DateTime _now = DateTime.Now;

        public EcoleAnneesScolairesTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var inscriptionResolver = new InscriptionActiveResolver(_context);
            _service = new EcoleService(
                _context,
                Mock.Of<IEmailService>(),
                inscriptionResolver);
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAnneeScolairesAsync_RetourneUniquementAnneesActives()
        {
            var active = TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6),
                statut: true);
            var inactive = TestDataBuilder.CreateAnneeScolaire(
                99, 1, "2023-2024",
                debut: _now.AddYears(-2),
                fin: _now.AddYears(-1),
                statut: false);

            _context.AnneeScolaires.AddRange(active, inactive);
            await _context.SaveChangesAsync();

            var result = (await _service.GetAnneeScolairesAsync(1)).ToList();

            result.Should().ContainSingle();
            result[0].IdAnneeScolaire.Should().Be(100);
            result[0].Statut.Should().BeTrue();
        }

        public void Dispose() => _context.Dispose();
    }
}
