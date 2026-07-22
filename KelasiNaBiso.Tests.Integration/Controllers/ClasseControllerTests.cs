using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Pagination;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using BCrypt.Net;
using AspNetCoreRateLimit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    /// <summary>
    /// Tests d'intégration pour ClasseController - Pagination
    /// </summary>
    public class ClasseControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly KelasiNaBisoDbContext _context;
        private readonly IServiceScope _scope;

        public ClasseControllerTests(WebApplicationFactory<Program> factory)
        {
            var dbName = "TestDb_Classe_" + Guid.NewGuid().ToString();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remplacer le DbContext par InMemory
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<KelasiNaBisoDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<KelasiNaBisoDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });

                    // Désactiver le rate limiting pour les tests en configurant les options
                    services.Configure<IpRateLimitOptions>(options =>
                    {
                        options.EnableEndpointRateLimiting = false;
                        options.GeneralRules = new List<RateLimitRule>();
                        options.IpWhitelist = new List<string> { "127.0.0.1", "::1", "localhost" };
                        options.RealIpHeader = null; // Désactiver la détection d'IP réelle
                    });
                    
                    // Configurer les politiques pour éviter les erreurs
                    services.Configure<IpRateLimitPolicies>(options =>
                    {
                        options.IpRules = new List<IpRateLimitPolicy>();
                    });
                });
            });

            _client = _factory.CreateClient();

            // Créer un scope pour accéder au DbContext
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            _context.Database.EnsureCreated();

            // Initialiser les données de test
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Créer une école
            var ecole = new Ecole
            {
                IdEcole = 1,
                Nom = "Test Ecole",
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Ecoles.Add(ecole);

            // Créer une direction
            var direction = new Direction
            {
                IdDirection = 1,
                IdEcole = 1,
                NomDirection = "Direction Test",
                Statut = true
            };
            _context.Directions.Add(direction);

            // Créer une section
            var section = new Section
            {
                IdSection = 1,
                NomSection = "Section Test",
                IdEcole = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Sections.Add(section);

            // Créer une option
            var option = new Option
            {
                IdOption = 1,
                NomOption = "Option Test",
                IdSection = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            _context.Options.Add(option);

            // Créer des classes
            var classes = new List<Classe>
            {
                new Classe { IdClasse = 1, NomClasse = "5ème A", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 2, NomClasse = "5ème B", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 3, NomClasse = "6ème A", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 4, NomClasse = "6ème B", IdDirection = 1, IdSection = 1, IdOption = 1, Statut = false, DateCreation = DateTime.Now }, // Inactive
            };
            _context.Classes.AddRange(classes);

            // Créer un utilisateur admin pour l'authentification
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

            // Créer un rôle Admin
            var roleAdmin = new Role
            {
                IdRole = 1,
                Nom = "Admin",
                Niveau = 1,
                Statut = true
            };
            _context.Roles.Add(roleAdmin);

            // Associer le rôle à l'utilisateur
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
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.TotalRecords.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            // Arrange - Ne pas ajouter le token

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldFilterBySearchTerm_WhenSearchTermIsProvided()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SearchTerm=5ème");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.Data.All(c => c.NomClasse.Contains("5ème")).Should().BeTrue();
            result.TotalRecords.Should().Be(2); // 5ème A et 5ème B
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnOnlyActiveClasses_WhenIncludeInactiveIsFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&IncludeInactive=false");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.Data.All(c => c.Statut == true).Should().BeTrue();
            result.TotalRecords.Should().Be(3); // Seulement les classes actives
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnAllClasses_WhenIncludeInactiveIsTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&IncludeInactive=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.TotalRecords.Should().Be(4); // Toutes les classes (actives + inactives)
        }

        [Fact]
        public async Task GetClassesPaged_ShouldSortByNomClasseAscending_WhenSortDescendingIsFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SortBy=NomClasse&SortDescending=false");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            
            var classes = result.Data.ToList();
            if (classes.Count >= 2)
            {
                // Vérifier que les classes sont triées par ordre croissant
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
            // Arrange - Ajouter plus de classes
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

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=2&PageSize=5");

            // Assert
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
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Recherche qui ne correspond à rien
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=10&SearchTerm=ClasseInexistante12345");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Classe>>();
            result.Should().NotBeNull();
            result!.Data.Should().BeEmpty();
            result.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnBadRequest_WhenInvalidPageNumber()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=0&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetClassesPaged_ShouldReturnBadRequest_WhenInvalidPageSize()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/Classe/paged?PageNumber=1&PageSize=0");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
        }
    }
}

