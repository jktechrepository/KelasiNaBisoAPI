using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Services
{
    /// <summary>
    /// Parent multi-école et réinscription : Inscription comme source de vérité.
    /// </summary>
    public class InscriptionModelIntegrationTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;

        public InscriptionModelIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            SeedData();
        }

        private void SeedData()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            context.Ecoles.AddRange(
                new Ecole { IdEcole = 1, Nom = "Ecole Alpha", Statut = true, DateCreation = DateTime.Now },
                new Ecole { IdEcole = 2, Nom = "Ecole Beta", Statut = true, DateCreation = DateTime.Now });
            context.Directions.AddRange(
                new Direction { IdDirection = 1, IdEcole = 1, NomDirection = "Dir A", Statut = true, DateCreation = DateTime.Now },
                new Direction { IdDirection = 2, IdEcole = 2, NomDirection = "Dir B", Statut = true, DateCreation = DateTime.Now });
            context.Classes.AddRange(
                new Classe { IdClasse = 10, IdDirection = 1, NomClasse = "6e A", Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 11, IdDirection = 1, NomClasse = "7e A", Statut = true, DateCreation = DateTime.Now },
                new Classe { IdClasse = 20, IdDirection = 2, NomClasse = "5e B", Statut = true, DateCreation = DateTime.Now });
            context.AnneeScolaires.AddRange(
                new AnneeScolaire { IdAnneeScolaire = 100, IdEcole = 1, LibelleAnneeScolaire = "2025-2026", DateDebut = new DateTime(2025, 9, 1), DateFin = new DateTime(2026, 6, 30), Statut = true, DateCreation = DateTime.Now },
                new AnneeScolaire { IdAnneeScolaire = 101, IdEcole = 1, LibelleAnneeScolaire = "2026-2027", DateDebut = new DateTime(2026, 9, 1), DateFin = new DateTime(2027, 6, 30), Statut = true, DateCreation = DateTime.Now });
            context.Tuteurs.Add(new Tuteur
            {
                IdTuteur = 1,
                NomComplet = "Parent Multi",
                Genre = "M",
                Telephone = "+243900111222",
                Statut = true,
                DateCreation = DateTime.Now
            });
            context.Eleves.AddRange(
                new Eleve { IdEleve = 1, IdTuteur = 1, Nom = "Enfant1", Postnom = "T", Prenom = "A", NomComplet = "Enfant1 T A", Genre = "M", DateNaissance = new DateTime(2015, 1, 1), Nationalite = "RDC", Statut = true, DateCreation = DateTime.Now },
                new Eleve { IdEleve = 2, IdTuteur = 1, Nom = "Enfant2", Postnom = "T", Prenom = "B", NomComplet = "Enfant2 T B", Genre = "F", DateNaissance = new DateTime(2016, 1, 1), Nationalite = "RDC", Statut = true, DateCreation = DateTime.Now });
            context.Inscriptions.AddRange(
                new Inscription { IdInscription = 1, Type = "Inscription", IdEleve = 1, IdEcole = 1, IdClasse = 10, IdAnneeScolaire = 100, DateInscription = DateTime.Now, StatutInscription = "Confirmé", Statut = true, DateCreation = DateTime.Now },
                new Inscription { IdInscription = 2, Type = "Inscription", IdEleve = 2, IdEcole = 2, IdClasse = 20, IdAnneeScolaire = 100, DateInscription = DateTime.Now, StatutInscription = "Confirmé", Statut = true, DateCreation = DateTime.Now },
                new Inscription { IdInscription = 3, Type = "Réinscription", IdEleve = 1, IdEcole = 1, IdClasse = 11, IdAnneeScolaire = 101, DateInscription = DateTime.Now, StatutInscription = "Confirmé", Statut = true, DateCreation = DateTime.Now });
            context.SaveChanges();
        }

        [Fact]
        public async Task FilterTuteursInEcole_SameTuteur_InTwoSchools_WithoutIdEcole()
        {
            using var scope = _factory.Services.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            var tuteursEcole1 = await resolver.FilterTuteursInEcole(context.Tuteurs, 1).ToListAsync();
            var tuteursEcole2 = await resolver.FilterTuteursInEcole(context.Tuteurs, 2).ToListAsync();

            tuteursEcole1.Should().ContainSingle(t => t.IdTuteur == 1);
            tuteursEcole2.Should().ContainSingle(t => t.IdTuteur == 1);
        }

        [Fact]
        public async Task GetClasseCouranteAsync_ReturnsNewInscription_AfterReinscriptionYearNPlus1()
        {
            using var scope = _factory.Services.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();

            var classeCourante = await resolver.GetClasseCouranteAsync(1, 101);

            classeCourante.Should().Be(11);
        }

        [Fact]
        public async Task FilterElevesInClasse_UsesInscription_NotLegacyIdClasse()
        {
            using var scope = _factory.Services.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            var elevesClasse11 = await resolver.FilterElevesInClasse(context.Eleves, 11).ToListAsync();

            elevesClasse11.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task FilterElevesInEcole_WithYearFilter_ReturnsOnlyMatchingYear()
        {
            using var scope = _factory.Services.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IInscriptionActiveResolver>();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            var elevesN = await resolver.FilterElevesInEcole(context.Eleves, 1, 100).ToListAsync();
            var elevesN1 = await resolver.FilterElevesInEcole(context.Eleves, 1, 101).ToListAsync();

            elevesN.Should().ContainSingle(e => e.IdEleve == 1);
            elevesN1.Should().ContainSingle(e => e.IdEleve == 1);

            var classeN = await resolver.GetClasseCouranteAsync(1, 100);
            var classeN1 = await resolver.GetClasseCouranteAsync(1, 101);
            classeN.Should().Be(10);
            classeN1.Should().Be(11);
        }

        public void Dispose() => _factory.Dispose();
    }
}
