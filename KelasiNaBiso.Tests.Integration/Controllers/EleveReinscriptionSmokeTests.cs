using System.Net;
using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class EleveReinscriptionSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly DateTime _now = DateTime.Now;

        public EleveReinscriptionSmokeTests()
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
            context.Eleves.Add(new Eleve
            {
                IdEleve = 2,
                IdTuteur = 1,
                Nom = "D",
                Postnom = "E",
                Prenom = "F",
                NomComplet = "D E F",
                Matricule = "MAT-SMOKE-002",
                Genre = "F",
                DateNaissance = new DateTime(2016, 1, 1),
                Nationalite = "RDC",
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.Inscriptions.Add(new Inscription
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
            context.SaveChanges();
        }

        [Fact]
        public async Task GetReinscription_Http_ShouldReturnUnauthorized_WhenAnonymous()
        {
            var response = await _client.GetAsync("/api/Eleve/reinscription?matricule=MAT-SMOKE-002");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetReinscriptionByMatricule_ViaService_DefaultsToPreviousYear()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEleveRepository>();

            var result = await repo.GetReinscriptionPrefillByMatriculeAsync(1, "MAT-SMOKE-002");

            result.Should().NotBeNull();
            result!.IdEleveExistant.Should().Be(2);
            result.IdAnneeScolaireReference.Should().Be(99);
            result.Type.Should().Be("Réinscription");
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
