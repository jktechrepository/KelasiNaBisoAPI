using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KelasiNaBiso.Tests.Unit.Services
{
    /// <summary>
    /// Tests unitaires pour PermissionService - Multi-rôles
    /// </summary>
    public class PermissionServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly Mock<ILogger<PermissionService>> _loggerMock;
        private readonly PermissionService _permissionService;

        public PermissionServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _loggerMock = new Mock<ILogger<PermissionService>>();
            _permissionService = new PermissionService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetUserRolesAsync_ShouldReturnActiveRoles_WhenUserHasMultipleRoles()
        {
            // Arrange
            var ecole = TestDataBuilder.CreateEcole(1, "Test Ecole");
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com", idEcole: 1);
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);
            var role2 = TestDataBuilder.CreateRole(2, "Parent", niveau: 5);
            var role3 = TestDataBuilder.CreateRole(3, "Admin", niveau: 1);

            _context.Ecoles.Add(ecole);
            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2, role3);
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true),  // Enseignant (actif)
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: false), // Parent (actif)
                TestDataBuilder.CreateUserRole(1, 3, isPrimary: false, statut: false) // Admin (inactif)
            );
            await _context.SaveChangesAsync();

            // Act
            var roles = await _permissionService.GetUserRolesAsync(1);

            // Assert
            roles.Should().HaveCount(2);
            roles.Should().Contain(r => r.Nom == "Enseignant");
            roles.Should().Contain(r => r.Nom == "Parent");
            roles.Should().NotContain(r => r.Nom == "Admin");
        }

        [Fact]
        public async Task GetUserRolesAsync_ShouldReturnEmpty_WhenUserHasNoRoles()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Act
            var roles = await _permissionService.GetUserRolesAsync(1);

            // Assert
            roles.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserPrimaryRoleAsync_ShouldReturnPrimaryRole_WhenExists()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);
            var role2 = TestDataBuilder.CreateRole(2, "Parent", niveau: 5);

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2);
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: false), // Enseignant
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: true)   // Parent (principal)
            );
            await _context.SaveChangesAsync();

            // Act
            var primaryRole = await _permissionService.GetUserPrimaryRoleAsync(1);

            // Assert
            primaryRole.Should().NotBeNull();
            primaryRole!.Nom.Should().Be("Parent");
        }

        [Fact]
        public async Task GetUserPrimaryRoleAsync_ShouldReturnHighestLevelRole_WhenNoPrimarySet()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);
            var role2 = TestDataBuilder.CreateRole(2, "Admin", niveau: 1); // Plus haut niveau

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2);
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: false), // Enseignant
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: false)  // Admin (niveau plus élevé)
            );
            await _context.SaveChangesAsync();

            // Act
            var primaryRole = await _permissionService.GetUserPrimaryRoleAsync(1);

            // Assert
            primaryRole.Should().NotBeNull();
            primaryRole!.Nom.Should().Be("Admin"); // Niveau le plus bas = plus élevé
        }

        [Fact]
        public async Task GetUserPrimaryRoleAsync_ShouldReturnNull_WhenUserHasNoActiveRoles()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Act
            var primaryRole = await _permissionService.GetUserPrimaryRoleAsync(1);

            // Assert
            primaryRole.Should().BeNull();
        }

        [Fact]
        public async Task GetEffectiveUserPermissionsAsync_ShouldReturnUnionOfAllRolesPermissions()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant");
            var role2 = TestDataBuilder.CreateRole(2, "Parent");

            var perm1 = TestDataBuilder.CreatePermission(1, "Note.Create", "Note");
            var perm2 = TestDataBuilder.CreatePermission(2, "Note.Read", "Note");
            var perm3 = TestDataBuilder.CreatePermission(3, "Eleve.ReadChildren", "Eleve");
            var perm4 = TestDataBuilder.CreatePermission(4, "Paiement.Read", "Paiement");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2);
            _context.Permissions.AddRange(perm1, perm2, perm3, perm4);
            _context.RolePermissions.AddRange(
                TestDataBuilder.CreateRolePermission(1, 1), // Enseignant: Note.Create
                TestDataBuilder.CreateRolePermission(1, 2), // Enseignant: Note.Read
                TestDataBuilder.CreateRolePermission(2, 3), // Parent: Eleve.ReadChildren
                TestDataBuilder.CreateRolePermission(2, 4)  // Parent: Paiement.Read
            );
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1),
                TestDataBuilder.CreateUserRole(1, 2)
            );
            await _context.SaveChangesAsync();

            // Act
            var permissions = await _permissionService.GetEffectiveUserPermissionsAsync(1);

            // Assert
            permissions.Should().HaveCount(4);
            permissions.Should().Contain("Note.Create");
            permissions.Should().Contain("Note.Read");
            permissions.Should().Contain("Eleve.ReadChildren");
            permissions.Should().Contain("Paiement.Read");
        }

        [Fact]
        public async Task UserHasPermissionAsync_ShouldReturnTrue_WhenUserHasPermissionInAnyRole()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant");
            var perm1 = TestDataBuilder.CreatePermission(1, "Note.Create", "Note");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.Add(role1);
            _context.Permissions.Add(perm1);
            _context.RolePermissions.Add(TestDataBuilder.CreateRolePermission(1, 1));
            _context.UserRoles.Add(TestDataBuilder.CreateUserRole(1, 1));
            await _context.SaveChangesAsync();

            // Act
            var hasPermission = await _permissionService.UserHasPermissionAsync(1, "Note.Create");

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasPermissionAsync_ShouldReturnFalse_WhenUserDoesNotHavePermission()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant");
            var perm1 = TestDataBuilder.CreatePermission(1, "Note.Create", "Note");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.Add(role1);
            _context.Permissions.Add(perm1);
            _context.UserRoles.Add(TestDataBuilder.CreateUserRole(1, 1));
            // Pas de RolePermission = pas de permission
            await _context.SaveChangesAsync();

            // Act
            var hasPermission = await _permissionService.UserHasPermissionAsync(1, "Note.Create");

            // Assert
            hasPermission.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

