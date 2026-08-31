using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    public class CaissierRoleSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CaissierRoleSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            context.Ecoles.Add(new Ecole
            {
                IdEcole = 1,
                Nom = "Ecole Caissier Smoke",
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetWalletMouvements_ShouldReturnForbidden_WhenCaissier()
        {
            var token = CreateCaissierToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Ecole/1/paiement-mobile/wallet/mouvements");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetOverview_ShouldNotReturnForbidden_WhenCaissier()
        {
            var token = CreateCaissierToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Ecole/1/paiement-mobile");

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PayInFraisScolaire_ShouldNotReturnForbidden_WhenCaissier()
        {
            var token = CreateCaissierToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/MokoAfrika/payin/frais-scolaire", new
            {
                idEleve = 1,
                montant = 100m,
                methode = "airtel",
                telephonePayeur = "+243900000099"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        private static string CreateCaissierToken(int idEcole)
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
            var role = new Role { IdRole = 99, Nom = UserRoles.CAISSIER, Niveau = 3, Statut = true };
            var utilisateur = new Utilisateur
            {
                IdUtilisateur = 500,
                Email = "caissier@test.com",
                NomUtilisateur = "Caissier",
                PostNomUtilisateur = "Test",
                PrenomUtilisateur = "Smoke",
                IdEcole = idEcole,
                Statut = true,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        IdRole = role.IdRole,
                        IdUtilisateur = 500,
                        IsPrimary = true,
                        Statut = true,
                        Role = role
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
