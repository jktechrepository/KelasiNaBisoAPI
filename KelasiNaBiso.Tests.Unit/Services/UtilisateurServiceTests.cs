using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KelasiNaBiso.Tests.Unit.Services
{
    /// <summary>
    /// Tests unitaires pour UtilisateurService - Multi-rôles
    /// </summary>
    public class UtilisateurServiceTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly UtilisateurService _utilisateurService;

        public UtilisateurServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _utilisateurService = new UtilisateurService(_context);
        }

        [Fact]
        public async Task AddRoleToUserAsync_ShouldAddRole_WhenRoleDoesNotExist()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.AddRoleToUserAsync(1, 1, assignedByUserId: 1, isPrimary: true);

            // Assert
            result.Should().BeTrue();
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 1);
            userRole.Should().NotBeNull();
            userRole!.Statut.Should().BeTrue();
            userRole.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public async Task AddRoleToUserAsync_ShouldReactivateRole_WhenRoleExistsButInactive()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant");
            var userRole = TestDataBuilder.CreateUserRole(1, 1, isPrimary: false, statut: false);

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.AddRoleToUserAsync(1, 1, assignedByUserId: 1, isPrimary: true);

            // Assert
            result.Should().BeTrue();
            var updatedUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 1);
            updatedUserRole.Should().NotBeNull();
            updatedUserRole!.Statut.Should().BeTrue();
            updatedUserRole.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public async Task AddRoleToUserAsync_ShouldSetAsPrimaryAndUnsetOthers_WhenIsPrimaryIsTrue()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant");
            var role2 = TestDataBuilder.CreateRole(2, "Parent");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2);
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true),  // Enseignant (principal)
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: false)  // Parent
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.AddRoleToUserAsync(1, 2, assignedByUserId: 1, isPrimary: true);

            // Assert
            result.Should().BeTrue();
            var enseignantRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 1);
            var parentRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 2);

            enseignantRole!.IsPrimary.Should().BeFalse();
            parentRole!.IsPrimary.Should().BeTrue();
        }

        [Fact]
        public async Task AddRoleToUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var role = TestDataBuilder.CreateRole(1, "Enseignant");
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.AddRoleToUserAsync(999, 1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddRoleToUserAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.AddRoleToUserAsync(1, 999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_ShouldSoftDeleteRole_WhenRoleExists()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant");
            var role2 = TestDataBuilder.CreateRole(2, "Parent");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.AddRange(role1, role2);
            _context.UserRoles.AddRange(
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true),
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: false)
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.RemoveRoleFromUserAsync(1, 2);

            // Assert
            result.Should().BeTrue();
            var removedRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 2);
            removedRole.Should().NotBeNull();
            removedRole!.Statut.Should().BeFalse();
            removedRole.IsPrimary.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.RemoveRoleFromUserAsync(1, 999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_ShouldReturnFalse_WhenUserHasOnlyOneRole()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant");

            _context.Utilisateurs.Add(utilisateur);
            _context.Roles.Add(role);
            _context.UserRoles.Add(TestDataBuilder.CreateUserRole(1, 1, isPrimary: true));
            await _context.SaveChangesAsync();

            // Act
            var result = await _utilisateurService.RemoveRoleFromUserAsync(1, 1);

            // Assert
            result.Should().BeFalse(); // Ne peut pas retirer le dernier rôle
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.IdUtilisateur == 1 && ur.IdRole == 1);
            userRole!.Statut.Should().BeTrue(); // Toujours actif
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

