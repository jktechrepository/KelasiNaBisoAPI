using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class InscriptionCreateStatutTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly InscriptionService _service;

        public InscriptionCreateStatutTests()
        {
            var options = new DbContextOptionsBuilder<KelasiNaBisoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new KelasiNaBisoDbContext(options);
            Seed();
            _service = CreateService();
        }

        private InscriptionService CreateService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:KelasiConnection"] = "Server=localhost;Database=test;"
                })
                .Build();

            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));

            return new InscriptionService(
                _context,
                config,
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<ISignalRNotificationService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<InscriptionService>.Instance,
                resolver,
                scope,
                new EleveCompteService(_context, Mock.Of<ISmsNotificationService>(), NullLogger<EleveCompteService>.Instance));
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1);
            var classeA = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);
            var classeB = TestDataBuilder.CreateClasse(11, "5e B", idDirection: 1);
            var annee100 = TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025");
            var annee101 = TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026");
            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent Test", telephone: "+243900000099");
            var eleve = TestDataBuilder.CreateEleve(1, 1, "Eleve Existant");

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.AddRange(classeA, classeB);
            _context.AnneeScolaires.AddRange(annee100, annee101);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.Add(eleve);
            _context.Inscriptions.Add(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateInscriptionAsync_WhenClientSendsEnAttente_PersistsConfirme()
        {
            var dto = BuildReinscriptionDto(statutInscription: "En attente");

            var result = await _service.CreateInscriptionAsync(dto);

            result.Success.Should().BeTrue();
            var inscription = await _context.Inscriptions
                .AsNoTracking()
                .OrderByDescending(i => i.IdInscription)
                .FirstAsync();
            inscription.StatutInscription.Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Fact]
        public async Task CreateInscriptionAsync_WhenClientSendsEnAttenteUnderscore_PersistsConfirme()
        {
            var dto = BuildReinscriptionDto(statutInscription: "EN_ATTENTE");

            var result = await _service.CreateInscriptionAsync(dto);

            result.Success.Should().BeTrue();
            var inscription = await _context.Inscriptions
                .AsNoTracking()
                .OrderByDescending(i => i.IdInscription)
                .FirstAsync();
            inscription.StatutInscription.Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Fact]
        public async Task CreateInscriptionAsync_WhenClientSendsAnnule_PersistsAnnule()
        {
            var dto = BuildReinscriptionDto(statutInscription: "Annulé");

            var result = await _service.CreateInscriptionAsync(dto);

            result.Success.Should().BeTrue();
            var inscription = await _context.Inscriptions
                .AsNoTracking()
                .OrderByDescending(i => i.IdInscription)
                .FirstAsync();
            inscription.StatutInscription.Should().Be("Annulé");
        }

        [Fact]
        public async Task CreateInscriptionAsync_WhenClientSendsUnknownStatus_PersistsConfirme()
        {
            var dto = BuildReinscriptionDto(statutInscription: "Brouillon");

            var result = await _service.CreateInscriptionAsync(dto);

            result.Success.Should().BeTrue();
            var inscription = await _context.Inscriptions
                .AsNoTracking()
                .OrderByDescending(i => i.IdInscription)
                .FirstAsync();
            inscription.StatutInscription.Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Fact]
        public async Task UpdateAsync_WhenStatutInscriptionEnAttente_PersistsConfirme()
        {
            var pending = await _context.Inscriptions.FindAsync(1);
            pending!.StatutInscription = "EN_ATTENTE";
            await _context.SaveChangesAsync();

            pending.StatutInscription = "En attente";
            pending.IdClasse = 11;

            var updated = await _service.UpdateAsync(pending);

            updated.Should().NotBeNull();
            updated!.StatutInscription.Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Fact]
        public async Task UpdateAsync_WhenStatutInscriptionAnnule_KeepsAnnule()
        {
            var inscription = await _context.Inscriptions.FindAsync(1);
            inscription!.StatutInscription = "Annulé";
            inscription.IdClasse = 10;

            var updated = await _service.UpdateAsync(inscription);

            updated.Should().NotBeNull();
            updated!.StatutInscription.Should().Be("Annulé");
        }

        private static CreateInscriptionDto BuildReinscriptionDto(string statutInscription) =>
            new()
            {
                Type = "Réinscription",
                IdEcole = 1,
                IdClasse = 11,
                IdAnneeScolaire = 101,
                DateInscription = DateTime.UtcNow,
                StatutInscription = statutInscription,
                IdEleveExistant = 1,
                NomEleve = "Eleve",
                PostnomEleve = "Existant",
                PrenomEleve = "Test",
                GenreEleve = "M",
                DateNaissanceEleve = new DateTime(2015, 1, 1),
                LieuNaissanceEleve = "Kinshasa",
                NationaliteEleve = "RDC",
                NomCompletTuteur = "Parent Test",
                GenreTuteur = "M",
                TelephoneTuteur = "+243900000099",
                MatriculeEleve = "MAT-TEST-001"
            };

        public void Dispose() => _context.Dispose();
    }
}
