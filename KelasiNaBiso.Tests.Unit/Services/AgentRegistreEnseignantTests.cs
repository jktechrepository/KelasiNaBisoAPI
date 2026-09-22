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
    public class AgentRegistreEnseignantTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly AgentService _sut;

        public AgentRegistreEnseignantTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new AgentService(
                _context,
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<KelasiNaBisoAPI.Services.Repositories.IEmailService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<AgentService>.Instance);

            _context.Agents.AddRange(
                new Agent
                {
                    IdAgent = 1,
                    Nom = "Mukendi",
                    Postnom = "Kabongo",
                    Prenom = "Jean",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    RoleAgent = "Enseignant",
                    Statut = true,
                    DateNaissance = new DateTime(1985, 1, 1),
                    TelephoneAgent = "+243900000001",
                    EmailAgent = "secret1@test.cd"
                },
                new Agent
                {
                    IdAgent = 2,
                    Nom = "Ilunga",
                    Postnom = "Mwamba",
                    Prenom = "Marie",
                    Province = "Haut-Katanga",
                    Ville = "Lubumbashi",
                    Fonction = "Professeur",
                    RoleAgent = null,
                    Statut = true,
                    DateNaissance = new DateTime(1988, 2, 2)
                },
                new Agent
                {
                    IdAgent = 3,
                    Nom = "Directeur",
                    Postnom = "X",
                    Prenom = "Y",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    RoleAgent = "Directeur",
                    Fonction = "Directeur",
                    Statut = true,
                    DateNaissance = new DateTime(1970, 3, 3)
                },
                new Agent
                {
                    IdAgent = 4,
                    Nom = "Inactif",
                    Postnom = "Z",
                    Prenom = "W",
                    Province = "Kinshasa",
                    Ville = "Kinshasa",
                    RoleAgent = "Enseignant",
                    Statut = false,
                    DateNaissance = new DateTime(1990, 4, 4)
                });
            _context.SaveChanges();
        }

        [Fact]
        public async Task Registre_ReturnsOnlyActiveTeachers_MinimalFields()
        {
            var items = await _sut.GetRegistreEnseignantAsync();

            items.Should().HaveCount(2);
            items.Should().OnlyContain(i =>
                i.Nom != null
                && !string.IsNullOrEmpty(i.Nom));
            // Pas de fuite : DTO n'a que 5 champs (compilateur) — vérifier absences via valeurs
            items.Select(i => i.Nom).Should().BeEquivalentTo(new[] { "Ilunga", "Mukendi" });
            items.Should().NotContain(i => i.Nom == "Directeur" || i.Nom == "Inactif");
        }

        [Fact]
        public async Task Registre_FiltersByProvinceAndVille()
        {
            var items = await _sut.GetRegistreEnseignantAsync(province: "kinshasa", ville: "kinshasa");

            items.Should().ContainSingle();
            items[0].Nom.Should().Be("Mukendi");
            items[0].Ville.Should().Be("Kinshasa");
        }

        public void Dispose() => _context.Dispose();
    }
}
