using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AgentDetailTitulaireActifTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly AgentService _sut;
        private readonly DateTime _now = DateTime.Now;

        public AgentDetailTitulaireActifTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new AgentService(
                _context,
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<KelasiNaBisoAPI.Services.Repositories.IEmailService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<AgentService>.Instance);

            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Agents.Add(TestDataBuilder.CreateAgent(10, "Kalala", idEcole: 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(1, "1ère Primaire A"));
            _context.Classes.Add(TestDataBuilder.CreateClasse(2, "2ème Primaire B"));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddMonths(-10),
                fin: _now.AddMonths(-1)));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                101, 1, "2025-2026",
                debut: _now.AddMonths(-1),
                fin: _now.AddMonths(8)));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetDetailById_SansTitulaire_ListeVide()
        {
            var detail = await _sut.GetDetailByIdAsync(10);

            detail.Should().NotBeNull();
            detail!.IdAgent.Should().Be(10);
            detail.Nom.Should().Be("Kalala");
            detail.TitulairesActifs.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDetailById_TitulaireAnneeCourante_RemplitDto()
        {
            _context.TitulairesClasses.Add(new TitulaireClasse
            {
                IdTitulaireClasse = 1,
                IdAgent = 10,
                IdClasse = 1,
                IdAnneeScolaire = 101,
                DateDebut = _now.AddMonths(-1),
                DateFin = null,
                Statut = true
            });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().ContainSingle();
            var t = detail.TitulairesActifs[0];
            t.IdTitulaireClasse.Should().Be(1);
            t.IdClasse.Should().Be(1);
            t.NomClasse.Should().Be("1ère Primaire A");
            t.IdAnneeScolaire.Should().Be(101);
            t.LibelleAnneeScolaire.Should().Be("2025-2026");
        }

        [Fact]
        public async Task GetDetailById_DeuxClassesAnneeCourante_RetourneLesDeux()
        {
            _context.TitulairesClasses.AddRange(
                new TitulaireClasse
                {
                    IdTitulaireClasse = 1,
                    IdAgent = 10,
                    IdClasse = 1,
                    IdAnneeScolaire = 101,
                    DateDebut = _now.AddMonths(-1),
                    DateFin = null,
                    Statut = true
                },
                new TitulaireClasse
                {
                    IdTitulaireClasse = 2,
                    IdAgent = 10,
                    IdClasse = 2,
                    IdAnneeScolaire = 101,
                    DateDebut = _now.AddDays(-5),
                    DateFin = null,
                    Statut = true
                });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().HaveCount(2);
            detail.TitulairesActifs.Select(x => x.IdClasse).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetDetailById_DeuxOuverts_NeGardeQueAnneeCourante()
        {
            _context.TitulairesClasses.AddRange(
                new TitulaireClasse
                {
                    IdTitulaireClasse = 1,
                    IdAgent = 10,
                    IdClasse = 2,
                    IdAnneeScolaire = 100,
                    DateDebut = _now.AddMonths(-8),
                    DateFin = null,
                    Statut = true
                },
                new TitulaireClasse
                {
                    IdTitulaireClasse = 2,
                    IdAgent = 10,
                    IdClasse = 1,
                    IdAnneeScolaire = 101,
                    DateDebut = _now.AddMonths(-1),
                    DateFin = null,
                    Statut = true
                });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().ContainSingle();
            detail.TitulairesActifs[0].IdTitulaireClasse.Should().Be(2);
            detail.TitulairesActifs[0].IdClasse.Should().Be(1);
            detail.TitulairesActifs[0].IdAnneeScolaire.Should().Be(101);
        }

        [Fact]
        public async Task GetDetailById_IgnoreDateFinRenseigneeEtStatutInactif()
        {
            _context.TitulairesClasses.AddRange(
                new TitulaireClasse
                {
                    IdTitulaireClasse = 1,
                    IdAgent = 10,
                    IdClasse = 1,
                    IdAnneeScolaire = 101,
                    DateDebut = _now.AddMonths(-2),
                    DateFin = _now.AddDays(-1),
                    Statut = true
                },
                new TitulaireClasse
                {
                    IdTitulaireClasse = 2,
                    IdAgent = 10,
                    IdClasse = 2,
                    IdAnneeScolaire = 101,
                    DateDebut = _now.AddDays(-5),
                    DateFin = null,
                    Statut = false
                });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDetailById_PasDeLigneAnneeCourante_ListeVide()
        {
            _context.TitulairesClasses.Add(new TitulaireClasse
            {
                IdTitulaireClasse = 1,
                IdAgent = 10,
                IdClasse = 2,
                IdAnneeScolaire = 100,
                DateDebut = _now.AddMonths(-8),
                DateFin = null,
                Statut = true
            });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDetailById_SansAnneeCourante_RetourneTousLesOuverts()
        {
            var precedente = await _context.AnneeScolaires.FindAsync(101);
            precedente!.Statut = false;
            await _context.SaveChangesAsync();

            _context.TitulairesClasses.AddRange(
                new TitulaireClasse
                {
                    IdTitulaireClasse = 1,
                    IdAgent = 10,
                    IdClasse = 2,
                    IdAnneeScolaire = 100,
                    DateDebut = _now.AddMonths(-8),
                    DateFin = null,
                    Statut = true
                },
                new TitulaireClasse
                {
                    IdTitulaireClasse = 2,
                    IdAgent = 10,
                    IdClasse = 1,
                    IdAnneeScolaire = 100,
                    DateDebut = _now.AddMonths(-6),
                    DateFin = null,
                    Statut = true
                });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailByIdAsync(10);

            detail!.TitulairesActifs.Should().HaveCount(2);
            detail.TitulairesActifs.Select(x => x.IdClasse).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetDetailById_AgentInconnu_Null()
        {
            (await _sut.GetDetailByIdAsync(999)).Should().BeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
