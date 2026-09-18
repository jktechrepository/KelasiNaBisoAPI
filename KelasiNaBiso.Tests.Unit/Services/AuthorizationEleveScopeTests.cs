using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AuthorizationEleveScopeTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly AuthorizationService _service;

        public AuthorizationEleveScopeTests()
        {
            var options = new DbContextOptionsBuilder<KelasiNaBisoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new KelasiNaBisoDbContext(options);

            var role = TestDataBuilder.CreateRole(1, UserRoles.ELEVE, niveau: 6);
            var user = TestDataBuilder.CreateUtilisateur(5, "eleve@test.com");
            user.IdRole = 1;
            user.IdEleve = 99;
            user.IdEcole = 1;
            user.Role = role;

            _context.Roles.Add(role);
            _context.Utilisateurs.Add(user);
            _context.SaveChanges();

            _service = new AuthorizationService(_context, NullLogger<AuthorizationService>.Instance);
        }

        [Fact]
        public async Task GetUserScopeAsync_ForEleve_IncludesOwnEleveId()
        {
            var scope = await _service.GetUserScopeAsync(5);

            scope.RoleName.Should().Be(UserRoles.ELEVE);
            scope.EleveIds.Should().Equal(99);
        }

        public void Dispose() => _context.Dispose();
    }
}
