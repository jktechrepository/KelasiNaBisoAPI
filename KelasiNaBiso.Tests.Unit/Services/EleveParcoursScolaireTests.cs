using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveParcoursScolaireTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _sut;
        private readonly DateTime _now = DateTime.Now;

        public EleveParcoursScolaireTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _sut = new EleveService(
                _context,
                Mock.Of<IInscriptionRepository>(),
                resolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole A"));
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(2, "Ecole B"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Directions.Add(TestDataBuilder.CreateDirection(2, 2));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "5e A", idDirection: 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(20, "6e B", idDirection: 2));

            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2024-2025",
                debut: _now.AddYears(-1),
                fin: _now.AddMonths(-3)));
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                200, 2, "2025-2026",
                debut: _now.AddMonths(-2),
                fin: _now.AddMonths(10)));

            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent Un"));
            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(2, "Autre Parent"));

            var eleve = TestDataBuilder.CreateEleve(1, 1, "Mukendi");
            eleve.Matricule = "MAT-001";
            _context.Eleves.Add(eleve);

            var autre = TestDataBuilder.CreateEleve(2, 2, "Autre");
            autre.Matricule = "MAT-002";
            _context.Eleves.Add(autre);

            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                1, 1, 1, 10, 100, dateInscription: _now.AddYears(-1)));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                2, 1, 2, 20, 200, dateInscription: _now.AddMonths(-1)));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(
                3, 1, 1, 10, 100, statutInscription: "En attente",
                dateInscription: _now.AddMonths(-6)));

            _context.Frais.Add(TestDataBuilder.CreateFrais(1, 2, 200, "Minerval"));
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 1,
                IdEleve = 1,
                IdFrais = 1,
                Montant = 50,
                Devise = "USD",
                DatePaiement = _now.AddDays(-5),
                Statut = true,
                StatutPaiement = "Confirmé",
                ReferencePaiemenet = "REF1"
            });

            _context.Presences.Add(new Presence
            {
                IdPresence = 1,
                IdEleve = 1,
                DateDuJour = _now.AddDays(-3),
                HeureArrivee = TimeSpan.FromHours(7),
                IsPresent = true,
                Statut = true,
                TypePresence = "ELEVE"
            });

            _context.SaveChanges();
        }

        private static ParcoursScolaireCallerContext SuperAdmin() => new()
        {
            Role = UserRoles.SUPER_ADMIN,
            IsSuperAdmin = true
        };

        [Fact]
        public async Task Parcours_UnknownMatricule_ReturnsNotFound()
        {
            var result = await _sut.GetParcoursScolaireByMatriculeAsync("UNKNOWN", SuperAdmin());
            result.Status.Should().Be(ParcoursScolaireAccessStatus.NotFound);
        }

        [Fact]
        public async Task Parcours_SuperAdmin_ReturnsConfirmedMultiEcoleFullDossier()
        {
            var result = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", SuperAdmin());

            result.Status.Should().Be(ParcoursScolaireAccessStatus.Ok);
            result.Data!.Eleve.Matricule.Should().Be("MAT-001");
            result.Data.Etapes.Should().HaveCount(2);
            result.Data.Etapes[0].NomEcole.Should().Be("Ecole A");
            result.Data.Etapes[1].NomEcole.Should().Be("Ecole B");
            result.Data.Etapes[1].Paiements.Should().ContainSingle();
            result.Data.Etapes[1].Presences.Should().ContainSingle();
        }

        [Fact]
        public async Task Parcours_Directeur_SameEcole_Allowed()
        {
            var result = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.DIRECTEUR,
                EcoleId = 1
            });

            result.Status.Should().Be(ParcoursScolaireAccessStatus.Ok);
            result.Data!.Etapes.Should().HaveCount(2);
        }

        [Fact]
        public async Task Parcours_Directeur_OtherEcoleOnly_Forbidden()
        {
            // Élève MAT-002 n'a aucune inscription → d'abord créer inscription école 2 only then directeur école 1
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(10, 2, 2, 20, 200));
            await _context.SaveChangesAsync();

            var result = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-002", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.DIRECTEUR,
                EcoleId = 1
            });

            result.Status.Should().Be(ParcoursScolaireAccessStatus.Forbidden);
        }

        [Fact]
        public async Task Parcours_SousDirecteur_SameGateAsDirecteur()
        {
            var result = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.SOUS_DIRECTEUR,
                EcoleId = 2
            });

            result.Status.Should().Be(ParcoursScolaireAccessStatus.Ok);
        }

        [Fact]
        public async Task Parcours_Eleve_Own_Ok_Other_Forbidden()
        {
            var own = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.ELEVE,
                EleveId = 1
            });
            own.Status.Should().Be(ParcoursScolaireAccessStatus.Ok);

            var other = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.ELEVE,
                EleveId = 2
            });
            other.Status.Should().Be(ParcoursScolaireAccessStatus.Forbidden);
        }

        [Fact]
        public async Task Parcours_Parent_LinkedChild_Ok_Other_Forbidden()
        {
            var ok = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.PARENT,
                TuteurId = 1
            });
            ok.Status.Should().Be(ParcoursScolaireAccessStatus.Ok);

            var forbidden = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.PARENT,
                TuteurId = 2
            });
            forbidden.Status.Should().Be(ParcoursScolaireAccessStatus.Forbidden);
        }

        [Fact]
        public async Task Parcours_UnauthorizedRole_Forbidden()
        {
            var result = await _sut.GetParcoursScolaireByMatriculeAsync("MAT-001", new ParcoursScolaireCallerContext
            {
                Role = UserRoles.CAISSIER,
                EcoleId = 1
            });
            result.Status.Should().Be(ParcoursScolaireAccessStatus.Forbidden);
        }

        public void Dispose() => _context.Dispose();
    }
}
