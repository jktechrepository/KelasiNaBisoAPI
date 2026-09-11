using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PermissionSeederDirecteurTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;

        public PermissionSeederDirecteurTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task EnsureDirecteurPermissionsAsync_ShouldRemovePaiementCreateUpdateDelete()
        {
            var directeurRole = TestDataBuilder.CreateRole(30, "Directeur", niveau: 2);
            _context.Roles.Add(directeurRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 1, Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Statut = true },
                new Permission { IdPermission = 2, Nom = "Paiement.Update", Categorie = "Paiement", Action = "Update", Statut = true },
                new Permission { IdPermission = 3, Nom = "Paiement.Delete", Categorie = "Paiement", Action = "Delete", Statut = true },
                new Permission { IdPermission = 4, Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Statut = true },
                new Permission { IdPermission = 5, Nom = "Paiement.Validate", Categorie = "Paiement", Action = "Validate", Statut = true }
            );
            _context.RolePermissions.AddRange(
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 1, DateAttribution = DateTime.UtcNow },
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 2, DateAttribution = DateTime.UtcNow },
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 3, DateAttribution = DateTime.UtcNow },
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 4, DateAttribution = DateTime.UtcNow },
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 5, DateAttribution = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureDirecteurPermissionsAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == directeurRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("Paiement.Read");
            assigned.Should().Contain("Paiement.Validate");
            assigned.Should().NotContain("Paiement.Create");
            assigned.Should().NotContain("Paiement.Update");
            assigned.Should().NotContain("Paiement.Delete");
        }

        [Fact]
        public async Task EnsureDirecteurPermissionsAsync_ShouldBeIdempotent()
        {
            var directeurRole = TestDataBuilder.CreateRole(31, "Directeur", niveau: 2);
            _context.Roles.Add(directeurRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 10, Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Statut = true },
                new Permission { IdPermission = 11, Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Statut = true }
            );
            _context.RolePermissions.AddRange(
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 10, DateAttribution = DateTime.UtcNow },
                new RolePermission { IdRole = directeurRole.IdRole, IdPermission = 11, DateAttribution = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureDirecteurPermissionsAsync(_context);
            await PermissionSeeder.EnsureDirecteurPermissionsAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == directeurRole.IdRole);
            count.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
