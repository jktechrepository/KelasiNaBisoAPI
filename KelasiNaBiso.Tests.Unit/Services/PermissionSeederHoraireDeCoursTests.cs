using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PermissionSeederHoraireDeCoursTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;

        public PermissionSeederHoraireDeCoursTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public async Task EnsureHoraireDeCoursPermissionsAsync_ShouldGrantReadOnlyToEnseignant()
        {
            var enseignantRole = TestDataBuilder.CreateRole(4, UserRoles.ENSEIGNANT, niveau: 4);
            _context.Roles.Add(enseignantRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 1, Nom = "HoraireDeCours.Create", Categorie = "HoraireDeCours", Action = "Create", Statut = true },
                new Permission { IdPermission = 2, Nom = "HoraireDeCours.Read", Categorie = "HoraireDeCours", Action = "Read", Statut = true },
                new Permission { IdPermission = 3, Nom = "HoraireDeCours.Update", Categorie = "HoraireDeCours", Action = "Update", Statut = true },
                new Permission { IdPermission = 4, Nom = "HoraireDeCours.Delete", Categorie = "HoraireDeCours", Action = "Delete", Statut = true }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == enseignantRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("HoraireDeCours.Read");
            assigned.Should().NotContain("HoraireDeCours.Create");
            assigned.Should().NotContain("HoraireDeCours.Update");
            assigned.Should().NotContain("HoraireDeCours.Delete");
        }

        [Fact]
        public async Task EnsureHoraireDeCoursPermissionsAsync_ShouldBeIdempotentForEnseignant()
        {
            var enseignantRole = TestDataBuilder.CreateRole(5, UserRoles.ENSEIGNANT, niveau: 4);
            _context.Roles.Add(enseignantRole);
            _context.Permissions.Add(
                new Permission { IdPermission = 20, Nom = "HoraireDeCours.Read", Categorie = "HoraireDeCours", Action = "Read", Statut = true });
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);
            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == enseignantRole.IdRole);
            count.Should().Be(1);
        }

        [Fact]
        public async Task EnsureHoraireDeCoursPermissionsAsync_ShouldGrantReadOnlyToParent()
        {
            var parentRole = TestDataBuilder.CreateRole(6, UserRoles.PARENT, niveau: 5);
            _context.Roles.Add(parentRole);
            _context.Permissions.AddRange(
                new Permission { IdPermission = 31, Nom = "HoraireDeCours.Create", Categorie = "HoraireDeCours", Action = "Create", Statut = true },
                new Permission { IdPermission = 32, Nom = "HoraireDeCours.Read", Categorie = "HoraireDeCours", Action = "Read", Statut = true },
                new Permission { IdPermission = 33, Nom = "HoraireDeCours.Update", Categorie = "HoraireDeCours", Action = "Update", Statut = true },
                new Permission { IdPermission = 34, Nom = "HoraireDeCours.Delete", Categorie = "HoraireDeCours", Action = "Delete", Statut = true }
            );
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);

            var assigned = await _context.RolePermissions
                .Where(rp => rp.IdRole == parentRole.IdRole)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission!.Nom)
                .ToListAsync();

            assigned.Should().Contain("HoraireDeCours.Read");
            assigned.Should().NotContain("HoraireDeCours.Create");
            assigned.Should().NotContain("HoraireDeCours.Update");
            assigned.Should().NotContain("HoraireDeCours.Delete");
        }

        [Fact]
        public async Task EnsureHoraireDeCoursPermissionsAsync_ShouldBeIdempotentForParent()
        {
            var parentRole = TestDataBuilder.CreateRole(7, UserRoles.PARENT, niveau: 5);
            _context.Roles.Add(parentRole);
            _context.Permissions.Add(
                new Permission { IdPermission = 40, Nom = "HoraireDeCours.Read", Categorie = "HoraireDeCours", Action = "Read", Statut = true });
            await _context.SaveChangesAsync();

            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);
            await PermissionSeeder.EnsureHoraireDeCoursPermissionsAsync(_context);

            var count = await _context.RolePermissions.CountAsync(rp => rp.IdRole == parentRole.IdRole);
            count.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
