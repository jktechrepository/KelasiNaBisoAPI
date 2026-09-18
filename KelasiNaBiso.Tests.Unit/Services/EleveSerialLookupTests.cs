using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveSerialLookupTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public EleveSerialLookupTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new EleveService(
                _context,
                Mock.Of<IInscriptionRepository>(),
                resolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2025-2026",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(6)));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent"));
            var eleve = TestDataBuilder.CreateEleve(1, 1, "Alice");
            eleve.SerialNumber = "SN-ALICE-001";
            _context.Eleves.Add(eleve);
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetBySerialNumberLookupAsync_ReturnsIdClasseAndNomClasse()
        {
            var dto = await _service.GetBySerialNumberLookupAsync("SN-ALICE-001");

            dto.Should().NotBeNull();
            dto!.IdEleve.Should().Be(1);
            dto.SerialNumber.Should().Be("SN-ALICE-001");
            dto.IdClasse.Should().Be(10);
            dto.NomClasse.Should().Be("6e A");
            dto.Tuteur.Should().NotBeNull();
        }

        [Fact]
        public async Task GetBySerialNumberLookupAsync_UnknownSerial_ReturnsNull()
        {
            var dto = await _service.GetBySerialNumberLookupAsync("SN-UNKNOWN");
            dto.Should().BeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
