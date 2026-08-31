using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PermissionSeederCaissierTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;

        public PermissionSeederCaissierTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task EnsureCaissierPermissionsAsync_ShouldGrantGuichetPermissionsOnly()
        {
            var caissierRole = TestDataBuilder.CreateRole(10, UserRoles.CAISSIER, niveau: 3);
            _context.Roles.Add(caissierRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 1, Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Statut = true },
                new Permission { IdPermission = 2, Nom = "Paiement.Update", Categorie = "Paiement", Action = "Update", Statut = true },
                new Permission { IdPermission = 3, Nom = "Frais.Read", Categorie = "Frais", Action = "Read", Statut = true },
                new Permission { IdPermission = 4, Nom = "Frais.Create", Categorie = "Frais", Action = "Create", Statut = true },
                new Permission { IdPermission = 5, Nom = "Eleve.ReadAll", Categorie = "Eleve", Action = "ReadAll", Statut = true }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureCaissierPermissionsAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == caissierRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("Paiement.Create");
            assigned.Should().Contain("Frais.Read");
            assigned.Should().Contain("Eleve.ReadAll");
            assigned.Should().NotContain("Paiement.Update");
            assigned.Should().NotContain("Frais.Create");
        }

        [Fact]
        public async Task EnsureCaissierPermissionsAsync_ShouldBeIdempotent()
        {
            var caissierRole = TestDataBuilder.CreateRole(11, UserRoles.CAISSIER, niveau: 3);
            _context.Roles.Add(caissierRole);
            _context.Permissions.Add(
                new Permission { IdPermission = 20, Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Statut = true });
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureCaissierPermissionsAsync(_context);
            await PermissionSeeder.EnsureCaissierPermissionsAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == caissierRole.IdRole);
            count.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
