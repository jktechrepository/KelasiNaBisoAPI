using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EcoleRegistreEcoleTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EcoleService _sut;

        public EcoleRegistreEcoleTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new EcoleService(
                _context,
                Mock.Of<IEmailService>(),
                new InscriptionActiveResolver(_context));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.AddRange(
                new Ecole
                {
                    IdEcole = 1,
                    Nom = "Collège Saint Joseph",
                    Type = "Privée",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    Commune = "Gombe",
                    ProvinceEducationnel = "Kinshasa",
                    Telephone = "+243900000001",
                    EmailContact = "secret@ecole.cd",
                    Statut = true
                },
                new Ecole
                {
                    IdEcole = 2,
                    Nom = "Collège Saint Pierre",
                    Type = "Conventionnée",
                    Province = "Haut-Katanga",
                    Ville = "Lubumbashi",
                    Commune = "Kampemba",
                    ProvinceEducationnel = "Haut-Katanga",
                    Statut = true
                },
                new Ecole
                {
                    IdEcole = 3,
                    Nom = "École Primaire Centrale",
                    Type = "Publique",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    Commune = "Lingwala",
                    ProvinceEducationnel = "Kinshasa",
                    Statut = true
                },
                new Ecole
                {
                    IdEcole = 4,
                    Nom = "Collège Saint Inactif",
                    Type = "Privée",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    Statut = false
                });
            _context.SaveChanges();
        }

        [Fact]
        public async Task Registre_ReturnsMinimalFields_ActiveOnly()
        {
            var items = await _sut.GetRegistreEcoleAsync("Collège Saint");

            items.Should().HaveCount(2);
            items.Select(i => i.Nom).Should().BeEquivalentTo(new[]
            {
                "Collège Saint Joseph",
                "Collège Saint Pierre"
            });
            items.Should().OnlyContain(i =>
                i.Nom != null
                && i.Type != null
                && i.Province != null
                && i.Ville != null);
            items.Should().NotContain(i => i.Nom!.Contains("Inactif"));
        }

        [Fact]
        public async Task Registre_FiltersByProvinceAndVille()
        {
            var items = await _sut.GetRegistreEcoleAsync(
                "Collège Saint", province: "kinshasa", ville: "kinshasa");

            items.Should().ContainSingle();
            items[0].Nom.Should().Be("Collège Saint Joseph");
            items[0].Commune.Should().Be("Gombe");
            items[0].ProvinceEducationnel.Should().Be("Kinshasa");
        }

        [Fact]
        public async Task Registre_ShortNom_ReturnsEmpty()
        {
            var items = await _sut.GetRegistreEcoleAsync("Co");
            items.Should().BeEmpty();
        }

        [Fact]
        public async Task Registre_RespectsLimit()
        {
            var items = await _sut.GetRegistreEcoleAsync("Collège Saint", limit: 1);
            items.Should().ContainSingle();
        }

        public void Dispose() => _context.Dispose();
    }
}
