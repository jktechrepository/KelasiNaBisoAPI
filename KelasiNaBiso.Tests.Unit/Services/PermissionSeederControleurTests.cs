using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PermissionSeederControleurTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;

        public PermissionSeederControleurTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task EnsureControleurPermissionsAsync_ShouldGrantControlPermissionsOnly()
        {
            var controleurRole = TestDataBuilder.CreateRole(20, UserRoles.CONTROLEUR, niveau: 4);
            _context.Roles.Add(controleurRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 1, Nom = "Presence.Create", Categorie = "Presence", Action = "Create", Statut = true },
                new Permission { IdPermission = 2, Nom = "Presence.Update", Categorie = "Presence", Action = "Update", Statut = true },
                new Permission { IdPermission = 3, Nom = "Frais.Read", Categorie = "Frais", Action = "Read", Statut = true },
                new Permission { IdPermission = 4, Nom = "Frais.Create", Categorie = "Frais", Action = "Create", Statut = true },
                new Permission { IdPermission = 5, Nom = "Paiement.Read", Categorie = "Paiement", Action = "Read", Statut = true },
                new Permission { IdPermission = 6, Nom = "Paiement.Create", Categorie = "Paiement", Action = "Create", Statut = true },
                new Permission { IdPermission = 7, Nom = "Eleve.ReadAll", Categorie = "Eleve", Action = "ReadAll", Statut = true }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureControleurPermissionsAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == controleurRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("Presence.Create");
            assigned.Should().Contain("Frais.Read");
            assigned.Should().Contain("Paiement.Read");
            assigned.Should().Contain("Eleve.ReadAll");
            assigned.Should().NotContain("Presence.Update");
            assigned.Should().NotContain("Frais.Create");
            assigned.Should().NotContain("Paiement.Create");
        }

        [Fact]
        public async Task EnsureControleurPermissionsAsync_ShouldBeIdempotent()
        {
            var controleurRole = TestDataBuilder.CreateRole(21, UserRoles.CONTROLEUR, niveau: 4);
            _context.Roles.Add(controleurRole);
            _context.Permissions.Add(
                new Permission { IdPermission = 30, Nom = "Presence.Read", Categorie = "Presence", Action = "Read", Statut = true });
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureControleurPermissionsAsync(_context);
            await PermissionSeeder.EnsureControleurPermissionsAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == controleurRole.IdRole);
            count.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
