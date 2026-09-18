using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KelasiNaBiso.Tests.Unit.Services
{
    /// <summary>
    /// Tests unitaires pour SimpleJwtService - Multi-rôles
    /// </summary>
    public class SimpleJwtServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly SimpleJwtService _jwtService;

        public SimpleJwtServiceTests()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestSecretKeyForJWTTokenGeneration123456789" },
                { "Jwt:Issuer", "KelasiNaBiso" },
                { "Jwt:Audience", "KelasiNaBisoUsers" },
                { "Jwt:ExpirationMinutes", "1440" }
            });
            _configuration = configBuilder.Build();
            _jwtService = new SimpleJwtService(_configuration);
        }

        [Fact]
        public void GenerateToken_ShouldIncludeAllRoles_WhenUserHasMultipleRoles()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role1 = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);
            var role2 = TestDataBuilder.CreateRole(2, "Parent", niveau: 5);

            utilisateur.UserRoles = new List<UserRole>
            {
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true),
                TestDataBuilder.CreateUserRole(1, 2, isPrimary: false)
            };
            utilisateur.UserRoles.First().Role = role1;
            utilisateur.UserRoles.Last().Role = role2;

            // Act
            var token = _jwtService.GenerateToken(utilisateur);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            // Vérifier que les deux rôles sont dans le token
            var roleClaims = jsonToken.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).ToList();
            roleClaims.Should().HaveCount(2);
            roleClaims.Should().Contain(c => c.Value == "Enseignant");
            roleClaims.Should().Contain(c => c.Value == "Parent");

            // Vérifier le claim primaryRole
            var primaryRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "primaryRole");
            primaryRoleClaim.Should().NotBeNull();
            primaryRoleClaim!.Value.Should().Be("Enseignant");

            // Vérifier le claim roles (JSON)
            var rolesClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "roles");
            rolesClaim.Should().NotBeNull();
            rolesClaim!.Value.Should().Contain("Enseignant");
            rolesClaim.Value.Should().Contain("Parent");
        }

        [Fact]
        public void GenerateToken_ShouldIncludePrimaryRole_WhenUserHasOneRole()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);

            utilisateur.UserRoles = new List<UserRole>
            {
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true)
            };
            utilisateur.UserRoles.First().Role = role;

            // Act
            var token = _jwtService.GenerateToken(utilisateur);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var roleClaims = jsonToken.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).ToList();
            roleClaims.Should().HaveCount(1);
            roleClaims.First().Value.Should().Be("Enseignant");

            var primaryRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "primaryRole");
            primaryRoleClaim.Should().NotBeNull();
            primaryRoleClaim!.Value.Should().Be("Enseignant");
        }

        [Fact]
        public void GenerateToken_ShouldBeValid_WhenValidated()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant");
            utilisateur.UserRoles = new List<UserRole>
            {
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true)
            };
            utilisateur.UserRoles.First().Role = role;

            // Act
            var token = _jwtService.GenerateToken(utilisateur);
            var isValid = _jwtService.ValidateToken(token);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_WhenTokenIsInvalid()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var isValid = _jwtService.ValidateToken(invalidToken);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void GenerateToken_ShouldIncludeUserId_WhenTokenGenerated()
        {
            // Arrange
            var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com");
            var role = TestDataBuilder.CreateRole(1, "Enseignant");
            utilisateur.UserRoles = new List<UserRole>
            {
                TestDataBuilder.CreateUserRole(1, 1, isPrimary: true)
            };
            utilisateur.UserRoles.First().Role = role;

            // Act
            var token = _jwtService.GenerateToken(utilisateur);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
            userIdClaim.Should().NotBeNull();
            userIdClaim!.Value.Should().Be("1");
        }

        [Fact]
        public void GenerateToken_ShouldIncludeEleveId_WhenUtilisateurHasIdEleve()
        {
            var utilisateur = TestDataBuilder.CreateUtilisateur(10, "eleve@test.com");
            utilisateur.IdEleve = 42;
            var role = TestDataBuilder.CreateRole(6, "Eleve", niveau: 6);
            utilisateur.UserRoles = new List<UserRole>
            {
                TestDataBuilder.CreateUserRole(10, 6, isPrimary: true)
            };
            utilisateur.UserRoles.First().Role = role;

            var token = _jwtService.GenerateToken(utilisateur);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var eleveIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "EleveId");
            eleveIdClaim.Should().NotBeNull();
            eleveIdClaim!.Value.Should().Be("42");

            var roleClaims = jsonToken.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).ToList();
            roleClaims.Should().Contain(c => c.Value == "Eleve");
        }
    }
}

