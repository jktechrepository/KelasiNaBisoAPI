using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class RoleServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly RoleService _service;

        public RoleServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _service = new RoleService(_context);

            _context.Roles.AddRange(
                new Role { IdRole = 1, Nom = "Super-Admin", Statut = true },
                new Role { IdRole = 2, Nom = "Admin", Statut = true },
                new Role { IdRole = 3, Nom = "Directeur", Statut = true },
                new Role { IdRole = 4, Nom = "Enseignant", Statut = true },
                new Role { IdRole = 5, Nom = "Eleve", Statut = true },
                new Role { IdRole = 6, Nom = "Parent", Statut = true },
                new Role { IdRole = 7, Nom = "RoleInactif", Statut = false },
                new Role { IdRole = 8, Nom = "IT-Support", Statut = true });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_DefaultRole_ExcludesNonAgentRoles()
        {
            var roles = await _service.GetAllAsync("Enseignant");
            var roleNames = roles.Select(role => role.Nom).ToList();

            roleNames.Should().NotContain("Eleve");
            roleNames.Should().NotContain("Parent");
            roleNames.Should().Contain("Enseignant");
            roleNames.Should().NotContain("Super-Admin");
            roleNames.Should().NotContain("Admin");
            roleNames.Should().NotContain("Directeur");
            roleNames.Should().NotContain("IT-Support");
            roleNames.Should().NotContain("RoleInactif");
        }

        [Fact]
        public async Task GetAllAsync_SuperAdmin_IncludesITSupport()
        {
            var roles = await _service.GetAllAsync("Super-Admin");
            var roleNames = roles.Select(role => role.Nom).ToList();

            roleNames.Should().Contain("IT-Support");
            roleNames.Should().NotContain("Eleve");
            roleNames.Should().NotContain("Parent");
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Directeur")]
        [InlineData("IT-Support")]
        [InlineData("Enseignant")]
        public async Task GetAllAsync_NonSuperAdmin_ExcludesITSupport(string nomRole)
        {
            var roles = await _service.GetAllAsync(nomRole);
            var roleNames = roles.Select(role => role.Nom).ToList();

            roleNames.Should().NotContain("IT-Support");
            roleNames.Should().NotContain("Eleve");
            roleNames.Should().NotContain("Parent");
        }

        public void Dispose() => _context.Dispose();
    }
}
