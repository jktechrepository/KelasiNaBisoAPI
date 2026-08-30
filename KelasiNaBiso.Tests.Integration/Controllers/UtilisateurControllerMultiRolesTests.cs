using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    /// <summary>
    /// Tests d'intégration pour les endpoints multi-rôles de UtilisateurController
    /// </summary>
    public class UtilisateurControllerMultiRolesTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly KelasiNaBisoDbContext _context;
        private readonly IServiceScope _scope;

        public UtilisateurControllerMultiRolesTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            _context.Database.EnsureCreated();

            SeedTestData();
        }

        private void SeedTestData()
        {
            var ecole = new Ecole
            {
                IdEcole = 1,
                Nom = "Test Ecole",
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Ecoles.Add(ecole);

            var roleEnseignant = new Role { IdRole = 1, Nom = "Enseignant", Niveau = 4, Statut = true };
            var roleParent = new Role { IdRole = 2, Nom = "Parent", Niveau = 5, Statut = true };
            var roleAdmin = new Role { IdRole = 3, Nom = "Super-Admin", Niveau = 1, Statut = true };
            _context.Roles.AddRange(roleEnseignant, roleParent, roleAdmin);

            var admin = new Utilisateur
            {
                IdUtilisateur = 1,
                Email = "admin@test.com",
                NomUtilisateur = "Admin",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Statut = true,
                IdEcole = 1,
                DateCreation = DateTime.Now
            };
            _context.Utilisateurs.Add(admin);

            var utilisateur = new Utilisateur
            {
                IdUtilisateur = 2,
                Email = "user@test.com",
                NomUtilisateur = "User",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Statut = true,
                IdEcole = 1,
                DateCreation = DateTime.Now
            };
            _context.Utilisateurs.Add(utilisateur);

            var adminUserRole = new UserRole
            {
                IdUtilisateur = 1,
                IdRole = 3,
                IsPrimary = true,
                Statut = true,
                DateAttribution = DateTime.Now,
                Utilisateur = admin,
                Role = roleAdmin
            };
            _context.UserRoles.Add(adminUserRole);

            var userUserRole = new UserRole
            {
                IdUtilisateur = 2,
                IdRole = 1,
                IsPrimary = true,
                Statut = true,
                DateAttribution = DateTime.Now,
                Utilisateur = utilisateur,
                Role = roleEnseignant
            };
            _context.UserRoles.Add(userUserRole);

            _context.SaveChanges();
        }

        private async Task<string> GetAuthTokenAsync(string email, string password)
        {
            var loginRequest = new
            {
                EmailOuTelephone = email,
                MotDePasse = password
            };

            var response = await _client.PostAsJsonAsync("/api/Utilisateur/authentifier", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed: {response.StatusCode} - {errorContent}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = authResponse.GetProperty("accessToken").GetString();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token is null or empty in authentication response");
            }

            return token;
        }

        [Fact]
        public async Task GetUserRoles_ShouldReturnUserRoles_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Utilisateur/2/roles");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<Role>>();
            roles.Should().NotBeNull();
            roles!.Should().HaveCount(1);
            roles.First().Nom.Should().Be("Enseignant");
        }

        [Fact]
        public async Task AddRoleToUser_ShouldAddRole_WhenValidRequest()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync("/api/Utilisateur/2/roles/2?isPrimary=false", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Rôle ajouté avec succès");

            var rolesResponse = await _client.GetAsync("/api/Utilisateur/2/roles");
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<Role>>();
            roles.Should().HaveCount(2);
            roles!.Should().Contain(r => r.Nom == "Enseignant");
            roles.Should().Contain(r => r.Nom == "Parent");
        }

        [Fact]
        public async Task AddRoleToUser_ShouldSetAsPrimary_WhenIsPrimaryIsTrue()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            await _client.PostAsync("/api/Utilisateur/2/roles/2?isPrimary=false", null);

            var response = await _client.PostAsync("/api/Utilisateur/2/roles/2?isPrimary=true", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var rolesResponse = await _client.GetAsync("/api/Utilisateur/2/roles");
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<Role>>();
            roles.Should().HaveCount(2);
        }

        [Fact]
        public async Task RemoveRoleFromUser_ShouldRemoveRole_WhenValidRequest()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            await _client.PostAsync("/api/Utilisateur/2/roles/2?isPrimary=false", null);

            var response = await _client.DeleteAsync("/api/Utilisateur/2/roles/2");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Rôle retiré avec succès");

            var rolesResponse = await _client.GetAsync("/api/Utilisateur/2/roles");
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<Role>>();
            roles.Should().HaveCount(1);
            roles!.Should().NotContain(r => r.Nom == "Parent");
        }

        [Fact]
        public async Task RemoveRoleFromUser_ShouldReturnBadRequest_WhenLastRole()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync("/api/Utilisateur/2/roles/1");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Contain("dernier rôle actif");
        }

        [Fact]
        public async Task SetPrimaryRole_ShouldSetPrimaryRole_WhenValidRequest()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            await _client.PostAsync("/api/Utilisateur/2/roles/2?isPrimary=false", null);

            var response = await _client.PutAsync("/api/Utilisateur/2/roles/2/primary", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Rôle principal défini avec succès");
            result.GetProperty("primaryRole").GetProperty("nom").GetString().Should().Be("Parent");
        }

        [Fact]
        public async Task GetUserRoles_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Utilisateur/2/roles");

            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddRoleToUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync("/api/Utilisateur/999/roles/1?isPrimary=false", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}
