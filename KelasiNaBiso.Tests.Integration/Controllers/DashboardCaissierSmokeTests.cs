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
                CodeDevisePrincipale = "CDF",
                Statut = true,
                DateCreation = now
            });
            context.Ecoles.Add(new Ecole
            {
                IdEcole = 2,
                Nom = "Ecole Parent Multi",
                CodeDevisePrincipale = "USD",
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
            context.AnneeScolaires.Add(new AnneeScolaire
            {
                IdAnneeScolaire = 201,
                IdEcole = 2,
                LibelleAnneeScolaire = "2025-2026",
                DateDebut = now.AddMonths(-3),
                DateFin = now.AddMonths(6),
                Statut = true,
                DateCreation = now
            });
            context.Directions.Add(new Direction
            {
                IdDirection = 2,
                IdEcole = 2,
                NomDirection = "Dir Multi",
                Statut = true
            });
            context.Classes.Add(new Classe
            {
                IdClasse = 20,
                NomClasse = "6e B",
                IdDirection = 2,
                Statut = true,
                DateCreation = now
            });
            context.Tuteurs.Add(new Tuteur
            {
                IdTuteur = 11,
                NomComplet = "Parent Multi",
                Genre = "M",
                Telephone = "+243900000011",
                Statut = true,
                DateCreation = now
            });
            context.Eleves.Add(new Eleve
            {
                IdEleve = 110,
                Nom = "Enfant",
                Postnom = "Multi",
                Prenom = "Jean",
                NomComplet = "Enfant Multi Jean",
                Genre = "M",
                DateNaissance = new DateTime(2015, 1, 1),
                Nationalite = "RDC",
                IdTuteur = 11,
                Statut = true,
                DateCreation = now
            });
            context.Inscriptions.Add(new Inscription
            {
                IdInscription = 110,
                IdEleve = 110,
                IdEcole = 2,
                IdClasse = 20,
                IdAnneeScolaire = 201,
                DateInscription = now,
                Statut = true,
                StatutInscription = "Confirmé",
                Type = "Inscription"
            });
            context.DevisesMonetaires.AddRange(
                new DeviseMonetaire
                {
                    IdEcole = 1,
                    CodeDevise = "USD",
                    Libelle = "Dollar américain",
                    Symbole = "$",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                },
                new DeviseMonetaire
                {
                    IdEcole = 1,
                    CodeDevise = "CDF",
                    Libelle = "Franc congolais",
                    Symbole = "FC",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
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
        public async Task GetDashboardCaissierAlias_ShouldReturnOk_WhenCaissier()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 506);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordCaissier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardPaiement_ShouldReturnConfiguredSchoolCurrency_WhenSchoolHasCustomCurrency()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 516);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/paiement?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("\"devisePrincipale\":\"CDF\"");
        }

        [Fact]
        public async Task GetEcoleDevisePrincipale_ShouldReturnConfiguredCurrency_WhenSchoolExists()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 517);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Ecole/1/devise-principale");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("\"codeDevisePrincipale\":\"CDF\"");
        }

        [Fact]
        public async Task GetDevisePreview_ShouldReturnConvertedAmount_WhenRateExists()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
                context.TauxChanges.Add(new TauxChange
                {
                    IdEcole = 1,
                    CodeDeviseSource = "USD",
                    CodeDeviseCible = "CDF",
                    Taux = 2800m,
                    DateEffet = DateTime.UtcNow.AddDays(-1),
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 518);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/Devise/preview-conversion?idEcole=1&codeDeviseSource=USD&montant=25&dateReference={DateTime.UtcNow:O}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("\"codeDevisePrincipale\":\"CDF\"");
            content.Should().Contain("\"taux\":2800");
            content.Should().Contain("\"montantConverti\":70000");
        }

        [Fact]
        public async Task CreatePaiement_ShouldConvertAmountToSchoolPrincipalCurrency_WhenSourceCurrencyDiffers()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
                var caissierRole = context.Roles.First(r => r.Nom == UserRoles.CAISSIER);
                context.Utilisateurs.Add(new Utilisateur
                {
                    IdUtilisateur = 519,
                    Email = "caissier-multidevise@test.com",
                    NomUtilisateur = "Caissier",
                    PostNomUtilisateur = "MultiDevise",
                    PrenomUtilisateur = "Test",
                    IdEcole = 1,
                    Statut = true,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            IdRole = caissierRole.IdRole,
                            IdUtilisateur = 519,
                            IsPrimary = true,
                            Statut = true,
                            Role = caissierRole
                        }
                    }
                });
                context.Frais.Add(new Frais
                {
                    IdFrais = 9001,
                    LibelleFrais = "Frais MultiDevise",
                    Montant = 25,
                    Devise = "USD",
                    IdEcole = 1,
                    IdAnneeScolaire = 101,
                    Portee = PorteeFrais.Direction,
                    DateCreation = DateTime.Now,
                    Statut = true
                });
                context.TauxChanges.Add(new TauxChange
                {
                    IdEcole = 1,
                    CodeDeviseSource = "USD",
                    CodeDeviseCible = "CDF",
                    Taux = 2800m,
                    DateEffet = DateTime.UtcNow.AddDays(-1),
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 519);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/Paiement", new
            {
                idFrais = 9001,
                montant = 25,
                devise = "USD",
                modePaiement = "Cash",
                statutPaiement = "Confirme"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var scopeAfter = _factory.Services.CreateScope();
            var contextAfter = scopeAfter.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();
            var paiement = contextAfter.Paiements.Single(p => p.IdFrais == 9001);

            paiement.CodeDevisePaiement.Should().Be("USD");
            paiement.CodeDevisePrincipale.Should().Be("CDF");
            paiement.TauxVersDevisePrincipale.Should().Be(2800m);
            paiement.MontantPayeDevisePrincipale.Should().Be(70000m);
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

        [Fact]
        public async Task GetDashboardDirecteur_ShouldReturnOk_WhenDirecteur()
        {
            var token = CreateToken(UserRoles.DIRECTEUR, idEcole: 1, idUtilisateur: 514);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/directeur?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardDirecteurAlias_ShouldReturnOk_WhenDirecteur()
        {
            var token = CreateToken(UserRoles.DIRECTEUR, idEcole: 1, idUtilisateur: 515);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordDirecteur?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardFinancier_ShouldReturnOk_WhenFinancier()
        {
            var token = CreateToken(UserRoles.FINANCIER, idEcole: 1, idUtilisateur: 504);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/financier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardFinancierAlias_ShouldReturnOk_WhenFinancier()
        {
            var token = CreateToken(UserRoles.FINANCIER, idEcole: 1, idUtilisateur: 507);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordFinancier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardFinancier_ShouldReturnForbidden_WhenCaissier()
        {
            var token = CreateToken(UserRoles.CAISSIER, idEcole: 1, idUtilisateur: 505);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/financier?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDashboardTuteur_ShouldReturnOk_WhenParent()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 508, idTuteur: 11);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/tuteur?idEcole=1&libelleAnneeScolaire=2025-2026");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardTuteurAlias_ShouldReturnOk_WhenParent()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 509, idTuteur: 12);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordTuteur?idEcole=1&libelleAnneeScolaire=2025-2026");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardTuteur_DefaultLibelle_ShouldReturnOk_WhenParent()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 510, idTuteur: 13);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/tuteur?idEcole=1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardTuteur_MissingIdEcole_ShouldReturnOk_MultiEcoles()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 520, idTuteur: 11);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordTuteur?libelleAnneeScolaire=2025-2026");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardTuteur_MissingIdEcole_DefaultYear_ShouldReturnOk()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 522, idTuteur: 11);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/tuteur");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardTuteur_ParentJwtOtherSchool_WithChildInTarget_ShouldReturnOk()
        {
            var token = CreateToken(UserRoles.PARENT, idEcole: 1, idUtilisateur: 521, idTuteur: 11);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync(
                "/api/Dashboard/tuteur?idEcole=2&libelleAnneeScolaire=2025-2026");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardControleur_ShouldReturnOk_WhenControleur()
        {
            var token = CreateToken(UserRoles.CONTROLEUR, idEcole: 1, idUtilisateur: 512);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/controleur?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardControleurAlias_ShouldReturnOk_WhenControleur()
        {
            var token = CreateToken(UserRoles.CONTROLEUR, idEcole: 1, idUtilisateur: 513);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordControleur?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardEnseignant_ShouldReturnOk_WhenEnseignant()
        {
            var token = CreateToken(UserRoles.ENSEIGNANT, idEcole: 1, idUtilisateur: 510, idAgent: 21);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/enseignant?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDashboardEnseignantAlias_ShouldReturnOk_WhenEnseignant()
        {
            var token = CreateToken(UserRoles.ENSEIGNANT, idEcole: 1, idUtilisateur: 511, idAgent: 22);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Dashboard/DashbordEnseignant?idEcole=1&idAnneeScolaire=101");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private static string CreateToken(
            string role,
            int idEcole,
            int idUtilisateur,
            int? idTuteur = null,
            int? idAgent = null)
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
                IdTuteur = idTuteur,
                IdAgent = idAgent,
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

            return jwtService.GenerateToken(utilisateur, idAgent, idTuteur);
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
