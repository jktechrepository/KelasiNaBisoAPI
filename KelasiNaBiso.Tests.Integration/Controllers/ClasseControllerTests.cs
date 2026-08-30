using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    /// <summary>
    /// Tests d'intégration pour ClasseController - Pagination
    /// </summary>
    public class ClasseControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly KelasiNaBisoDbContext _context;
        private readonly IServiceScope _scope;

        public ClasseControllerTests()
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

            var direction = new Direction
            {
                IdDirection = 1,
                IdEcole = 1,
                NomDirection = "Direction Test",
                Statut = true
            };
            _context.Directions.Add(direction);

            var section = new Section
            {
                IdSection = 1,
                NomSection = "Section Test",
                IdEcole = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Sections.Add(section);

            var option = new Option
            {
                IdOption = 1,
                NomOption = "Option Test",
                IdSection = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Options.Add(option);

            var classes = new List<Classe>
            {
                new Classe { IdClasse = 1, NomClasse = "5ème A", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 2, NomClasse = "5ème B", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 3, NomClasse = "6ème A", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 4, NomClasse = "6ème B", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = false, DateCreation = DateTime.Now },
            };
            _context.Classes.AddRange(classes);

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

            var roleAdmin = new Role
            {
                IdRole = 1,
                Nom = "Admin",
                Niveau = 1,
                Statut = true
            };
            _context.Roles.Add(roleAdmin);

            var userRole = new UserRole
            {
                IdUtilisateur = 1,
                IdRole = 1,
                IsPrimary = true,
                Statut = true,
                DateAttribution = DateTime.Now
            };
            _context.UserRoles.Add(userRole);

            _context.SaveChanges();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                EmailOuTelephone = "admin@test.com",
                MotDePasse = "Admin123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Utilisateur/authentifier", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var token = jsonDoc.RootElement.GetProperty("accessToken").GetString();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token is null or empty in authentication response");
            }

            return token;
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnPagedResult_WhenAuthenticated()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.TotalRecords.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldFilterBySearchTerm_WhenSearchTermIsProvided()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SearchTerm=5ème");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.Data.All(c => c.NomClasse.Contains("5ème")).Should().BeTrue();
            result.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnOnlyActiveClasses_WhenIncludeInactiveIsFalse()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&IncludeInactive=false");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.Data.All(c => c.Statut == true).Should().BeTrue();
            result.TotalRecords.Should().Be(3);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnAllClasses_WhenIncludeInactiveIsTrue()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&IncludeInactive=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.TotalRecords.Should().Be(4);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldSortByNomClasseAscending_WhenSortDescendingIsFalse()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SortBy=NomClasse&SortDescending=false");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();

            var classes = result.Data.ToList();
            if (classes.Count >= 2)
            {
                for (int i = 0; i < classes.Count - 1; i++)
                {
                    string.Compare(classes[i].NomClasse, classes[i + 1].NomClasse, StringComparison.OrdinalIgnoreCase)
                        .Should().BeLessThanOrEqualTo(0);
                }
            }
        }

        [Fact]
        public async Task GetClassesPaged_ShouldPaginateCorrectly_WhenMultiplePages()
        {
            for (int i = 5; i <= 15; i++)
            {
                _context.Classes.Add(new Classe
                {
                    IdClasse = i,
                    NomClasse = $"Classe {i}",
                    IdDirection = 1,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();

            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=2&PageSize=5");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().HaveCount(5);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.HasPrevious.Should().BeTrue();
            result.HasNext.Should().BeTrue();
            result.FirstRowOnPage.Should().Be(6);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnEmptyResult_WhenNoClassesMatch()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SearchTerm=ClasseInexistante12345");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().BeEmpty();
            result.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldClampPageNumber_WhenInvalidPageNumber()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=0&PageSize=10");

            // PagedRequest corrige PageNumber < 1 vers 1 (pas de 400)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldClampPageSize_WhenInvalidPageSize()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=0");

            // PagedRequest corrige PageSize < 1 vers 1 (pas de 400)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.PageSize.Should().Be(1);
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}
