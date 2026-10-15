using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PermissionSeederFinancierInscriptionTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;

        public PermissionSeederFinancierInscriptionTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task EnsureFinancierInscriptionUpdateAsync_ShouldGrantInscriptionUpdate()
        {
            var financierRole = TestDataBuilder.CreateRole(20, UserRoles.FINANCIER, niveau: 3);
            _context.Roles.Add(financierRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 1, Nom = "Inscription.Read", Categorie = "Inscription", Action = "Read", Statut = true },
                new Permission { IdPermission = 2, Nom = "Inscription.Update", Categorie = "Inscription", Action = "Update", Statut = true },
                new Permission { IdPermission = 3, Nom = "Inscription.Create", Categorie = "Inscription", Action = "Create", Statut = true }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureFinancierInscriptionUpdateAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == financierRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("Inscription.Update");
            assigned.Should().NotContain("Inscription.Create");
            assigned.Should().NotContain("Inscription.Read");
        }

        [Fact]
        public async Task EnsureFinancierInscriptionUpdateAsync_ShouldBeIdempotent()
        {
            var financierRole = TestDataBuilder.CreateRole(21, UserRoles.FINANCIER, niveau: 3);
            _context.Roles.Add(financierRole);
            _context.Permissions.Add(
                new Permission { IdPermission = 10, Nom = "Inscription.Update", Categorie = "Inscription", Action = "Update", Statut = true });
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureFinancierInscriptionUpdateAsync(_context);
            await PermissionSeeder.EnsureFinancierInscriptionUpdateAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == financierRole.IdRole);
            count.Should().Be(1);
        }

        public void Dispose() => _context.Dispose();
    }
}
