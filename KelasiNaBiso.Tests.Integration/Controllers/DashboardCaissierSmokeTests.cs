using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class DashboardCaissierSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public DashboardCaissierSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            var now = DateTime.Now;
            context.Ecoles.Add(new Ecole
            {
                IdEcole = 1,
                Nom = "Ecole Dashboard Caissier",
                Statut = true,
                DateCreation = now
            });
            context.AnneeScolaires.Add(new AnneeScolaire
            {
                IdAnneeScolaire = 101,
                IdEcole = 1,
                LibelleAnneeScolaire = "2025-2026",
                DateDebut = now.AddMonths(-3),
                DateFin = now.AddMonths(6),
                Statut = true,
                DateCreation = now
            });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetDashboardCaissier_ShouldReturnUnauthorized_WhenAnonymous()
        {
            var response = await _client.GetAsync("/api/Dashboard/caissier?idEcole=1&idAnneeScolaire=101");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDashboardCaissier_ShouldReturnOk_WhenCaissier()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 500);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/caissier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardCaissier_ShouldReturnForbidden_WhenEnseignant()
        {
            var token = CreateToken(UserRoles.ENSEIGNANT, idEcole: 1, idUtilisateur: 501);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/caissier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDashboardCaissier_ShouldReturnForbidden_WhenWrongSchool()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 2, idUtilisateur: 502);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/caissier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetClotureCaissier_ShouldReturnOk_WhenCaissier()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 500);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/caissier/cloture?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardCaissier_ShouldReturnOk_WhenDirecteurWithScopeEcole()
        {
            var token = CreateToken(UserRoles.DIRECTEUR, idEcole: 1, idUtilisateur: 503);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/caissier?idEcole=1&idAnneeScolaire=101&scope=ecole");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private static string CreateToken(string role, int idEcole, int idUtilisateur)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "KelasiNaBiso-Test-SecretKey-Min32Chars-ForHS256!!",
                    ["Jwt:Issuer"] = "KelasiNaBiso",
                    ["Jwt:Audience"] = "KelasiNaBisoUsers",
                    ["Jwt:ExpirationMinutes"] = "60"
                })
                .Build();

            var jwtService = new SimpleJwtService(config);
            var roleEntity = new Role { IdRole = 99, Nom = role, Niveau = 3, Statut = true };
            var utilisateur = new Utilisateur
            {
                IdUtilisateur = idUtilisateur,
                Email = $"{role}@test.com",
                NomUtilisateur = role,
                PostNomUtilisateur = "Test",
                PrenomUtilisateur = "Smoke",
                IdEcole = idEcole,
                Statut = true,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        IdRole = roleEntity.IdRole,
                        IdUtilisateur = idUtilisateur,
                        IsPrimary = true,
                        Statut = true,
                        Role = roleEntity
                    }
                }
            };

            return jwtService.GenerateToken(utilisateur);
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
