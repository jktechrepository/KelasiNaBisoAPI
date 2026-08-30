using System.Net;
using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class EleveAnneeScolaireSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly DateTime _now = DateTime.Now;

        public EleveAnneeScolaireSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            context.Ecoles.Add(new Ecole { IdEcole = 1, Nom = "Ecole Smoke", Statut = true, DateCreation = DateTime.Now });
            context.Directions.Add(new Direction { IdDirection = 1, IdEcole = 1, NomDirection = "Dir", Statut = true, DateCreation = DateTime.Now });
            context.Classes.Add(new Classe { IdClasse = 10, IdDirection = 1, NomClasse = "6e", Statut = true, DateCreation = DateTime.Now });
            context.AnneeScolaires.AddRange(
                new AnneeScolaire
                {
                    IdAnneeScolaire = 100,
                    IdEcole = 1,
                    LibelleAnneeScolaire = "Courante",
                    DateDebut = _now.AddMonths(-2),
                    DateFin = _now.AddMonths(6),
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new AnneeScolaire
                {
                    IdAnneeScolaire = 99,
                    IdEcole = 1,
                    LibelleAnneeScolaire = "Past",
                    DateDebut = _now.AddYears(-2),
                    DateFin = _now.AddYears(-1),
                    Statut = true,
                    DateCreation = DateTime.Now
                });
            context.Tuteurs.Add(new Tuteur
            {
                IdTuteur = 1,
                NomComplet = "Parent",
                Genre = "M",
                Telephone = "+243900000001",
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.Eleves.AddRange(
                new Eleve
                {
                    IdEleve = 1,
                    IdTuteur = 1,
                    Nom = "A",
                    Postnom = "B",
                    Prenom = "C",
                    NomComplet = "A B C",
                    Genre = "M",
                    DateNaissance = new DateTime(2015, 1, 1),
                    Nationalite = "RDC",
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Eleve
                {
                    IdEleve = 2,
                    IdTuteur = 1,
                    Nom = "D",
                    Postnom = "E",
                    Prenom = "F",
                    NomComplet = "D E F",
                    Genre = "F",
                    DateNaissance = new DateTime(2016, 1, 1),
                    Nationalite = "RDC",
                    Statut = true,
                    DateCreation = DateTime.Now
                });
            context.Inscriptions.AddRange(
                new Inscription
                {
                    IdInscription = 1,
                    Type = "Inscription",
                    IdEleve = 1,
                    IdEcole = 1,
                    IdClasse = 10,
                    IdAnneeScolaire = 100,
                    DateInscription = DateTime.Now,
                    StatutInscription = "Confirmé",
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Inscription
                {
                    IdInscription = 2,
                    Type = "Inscription",
                    IdEleve = 2,
                    IdEcole = 1,
                    IdClasse = 10,
                    IdAnneeScolaire = 99,
                    DateInscription = DateTime.Now,
                    StatutInscription = "Confirmé",
                    Statut = true,
                    DateCreation = DateTime.Now
                });
            context.V_Eleves.AddRange(
                new V_Eleve
                {
                    IdEleve = 1,
                    NomComplet = "A B C",
                    IdClasse = 10,
                    IdEcole = 1,
                    DateNaissance = new DateTime(2015, 1, 1),
                    Nationalite = "RDC",
                    DateCreation = DateTime.Now
                },
                new V_Eleve
                {
                    IdEleve = 2,
                    NomComplet = "D E F",
                    IdClasse = 10,
                    IdEcole = 1,
                    DateNaissance = new DateTime(2016, 1, 1),
                    Nationalite = "RDC",
                    DateCreation = DateTime.Now
                });
            context.EleveParEcole.AddRange(
                new EleveParEcoleDTO
                {
                    IdEleve = 1,
                    NomCompletEleve = "A B C",
                    IdClasse = 10,
                    IdEcole = 1
                },
                new EleveParEcoleDTO
                {
                    IdEleve = 2,
                    NomCompletEleve = "D E F",
                    IdClasse = 10,
                    IdEcole = 1
                });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByEcole_ViaService_DefaultsToCurrentYear()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEleveRepository>();

            var result = await repo.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetByEcole_ViaService_WithPastAnnee_ReturnsPastOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEleveRepository>();

            var result = await repo.GetByEcoleAsync(1, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task VEleve_GetByEcole_DefaultsToCurrentYear()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IV_EleveRepository>();

            var result = await repo.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task VEleve_GetByEcole_WithPastAnnee_ReturnsPastOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IV_EleveRepository>();

            var result = await repo.GetByEcoleAsync(1, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task EleveParEcole_GetByEcole_DefaultsToCurrentYear()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEleveParEcoleRepository>();

            var result = await repo.GetByEcoleAsync(1, null);

            result.IdAnneeScolaire.Should().Be(100);
            result.Data.Should().ContainSingle(e => e.IdEleve == 1);
            result.Data.Should().NotContain(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task EleveParEcole_GetByEcole_WithPastAnnee_ReturnsPastOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEleveParEcoleRepository>();

            var result = await repo.GetByEcoleAsync(1, 99);

            result.IdAnneeScolaire.Should().Be(99);
            result.Data.Should().ContainSingle(e => e.IdEleve == 2);
        }

        [Fact]
        public async Task GetElevesByEcole_Http_ShouldReturnUnauthorized_WhenAnonymous()
        {
            var response = await _client.GetAsync("/api/Eleve/ecole/1");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
