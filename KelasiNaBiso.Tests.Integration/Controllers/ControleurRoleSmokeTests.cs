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
    public class ControleurRoleSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private const int ControleurUserId = 600;

        public ControleurRoleSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            if (!context.Ecoles.Any(e => e.IdEcole == 1))
            {
                context.Ecoles.Add(new Ecole
                {
                    IdEcole = 1,
                    Nom = "Ecole Controleur Smoke",
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                context.SaveChanges();
            }

            PermissionSeeder.SeedPermissionsAsync(context).GetAwaiter().GetResult();

            var controleurRole = context.Roles.First(r => r.Nom == UserRoles.CONTROLEUR);

            if (!context.Utilisateurs.Any(u => u.IdUtilisateur == ControleurUserId))
            {
                context.Utilisateurs.Add(new Utilisateur
                {
                    IdUtilisateur = ControleurUserId,
                    Email = "controleur@test.com",
                    NomUtilisateur = "Controleur",
                    PostNomUtilisateur = "Test",
                    PrenomUtilisateur = "Smoke",
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                    IdEcole = 1,
                    Statut = true,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            IdRole = controleurRole.IdRole,
                            IdUtilisateur = ControleurUserId,
                            IsPrimary = true,
                            Statut = true,
                            Role = controleurRole
                        }
                    }
                });
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task CreatePaiement_ShouldReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/Paiement", new
            {
                idEleve = 1,
                idFrais = 1,
                montant = 50m,
                modePaiement = "Cash",
                statutPaiement = "Confirme"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetWalletMouvements_ShouldReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Ecole/1/paiement-mobile/wallet/mouvements");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetFraisByEcole_ShouldNotReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Frais/ecole/1");

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreatePresence_ShouldNotReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/Presence", new
            {
                idEleve = 1,
                isPresent = true,
                dateDuJour = DateTime.Today.ToString("yyyy-MM-dd")
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetEleveBySerialNumber_ShouldNotReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Eleve/serial-number/SN-TEST");

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdatePresence_ShouldReturnForbidden_WhenControleur()
        {
            var token = CreateControleurToken(idEcole: 1);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PutAsJsonAsync("/api/Presence/1", new
            {
                idPresence = 1,
                isPresent = true,
                heureArrivee = DateTime.Now
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private string CreateControleurToken(int idEcole)
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
            var roleEntity = new Role { IdRole = 88, Nom = UserRoles.CONTROLEUR, Niveau = 4, Statut = true };
            var utilisateur = new Utilisateur
            {
                IdUtilisateur = ControleurUserId,
                Email = "controleur@test.com",
                NomUtilisateur = "Controleur",
                PostNomUtilisateur = "Test",
                PrenomUtilisateur = "Smoke",
                IdEcole = idEcole,
                Statut = true,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        IdRole = roleEntity.IdRole,
                        IdUtilisateur = ControleurUserId,
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
