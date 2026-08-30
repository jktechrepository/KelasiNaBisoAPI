using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AnneeScolaireScopeTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly InscriptionService _inscriptionService;
        private readonly ClasseService _classeService;
        private readonly PaiementService _paiementService;
        private readonly PresenceService _presenceService;
        private readonly DateTime _now = DateTime.Now;

        public AnneeScolaireScopeTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            _scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));

            _inscriptionService = new InscriptionService(
                _context,
                new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
                new UsernameGeneratorService(_context, NullLogger<UsernameGeneratorService>.Instance),
                Mock.Of<KelasiNaBisoAPI.Services.Repositories.IEmailService>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<ISignalRNotificationService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<InscriptionService>.Instance,
                resolver,
                _scope);

            _classeService = new ClasseService(_context, resolver, _scope);
            _paiementService = new PaiementService(
                _context,
                NullLogger<PaiementService>.Instance,
                Mock.Of<ICacheService>(),
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                _scope);

            _presenceService = new PresenceService(
                _context,
                NullLogger<PresenceService>.Instance,
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                _scope);

            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));

            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    _now.AddYears(-1), _now.AddMonths(-6), statut: true),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6), statut: true));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, null, "Alice"),
                TestDataBuilder.CreateEleve(2, null, "Bob"));

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 101),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100));

            _context.Frais.AddRange(
                new Frais
                {
                    IdFrais = 1,
                    LibelleFrais = "Frais N-1",
                    Montant = 100,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 100,
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Frais
                {
                    IdFrais = 2,
                    LibelleFrais = "Frais N",
                    Montant = 120,
                    Devise = "USD",
                    IdDirection = 1,
                    IdAnneeScolaire = 101,
                    Statut = true,
                    DateCreation = DateTime.Now
                });

            _context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 1,
                    IdEleve = 1,
                    IdFrais = 2,
                    Montant = 120,
                    DatePaiement = _now,
                    Statut = true
                },
                new Paiement
                {
                    IdPaiement = 2,
                    IdEleve = 2,
                    IdFrais = 1,
                    Montant = 100,
                    DatePaiement = _now.AddMonths(-8),
                    Statut = true
                });

            _context.Presences.AddRange(
                new Presence
                {
                    IdPresence = 1,
                    IdEleve = 1,
                    TypePresence = "ELEVE",
                    DateDuJour = _now.Date,
                    HeureArrivee = TimeSpan.FromHours(7),
                    Statut = true
                },
                new Presence
                {
                    IdPresence = 2,
                    IdEleve = 2,
                    TypePresence = "ELEVE",
                    DateDuJour = _now.Date,
                    HeureArrivee = TimeSpan.FromHours(7),
                    Statut = true
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Inscription_GetByEcole_DefaultsToCurrentYear()
        {
            var result = await _inscriptionService.GetByEcoleAsync(1);

            result.IdAnneeScolaire.Should().Be(101);
            result.Data.Should().HaveCount(1);
            result.Data.Single().IdEleve.Should().Be(1);
        }

        [Fact]
        public async Task Inscription_GetByEcole_OrderedByDateDesc()
        {
            _context.Eleves.Add(TestDataBuilder.CreateEleve(3, null, "Charlie"));
            _context.Inscriptions.Add(new Inscription
            {
                IdInscription = 3,
                IdEleve = 1,
                IdEcole = 1,
                IdClasse = 10,
                IdAnneeScolaire = 101,
                DateInscription = _now.AddDays(-10),
                Statut = true,
                StatutInscription = InscriptionActiveRules.StatutConfirme,
                Type = "Inscription"
            });
            _context.Inscriptions.Add(new Inscription
            {
                IdInscription = 4,
                IdEleve = 3,
                IdEcole = 1,
                IdClasse = 10,
                IdAnneeScolaire = 101,
                DateInscription = _now,
                Statut = true,
                StatutInscription = InscriptionActiveRules.StatutConfirme,
                Type = "Inscription"
            });
            await _context.SaveChangesAsync();

            var result = await _inscriptionService.GetByEcoleAsync(1, 101);
            result.Data.Select(i => i.IdInscription).First().Should().Be(4);
        }

        [Fact]
        public async Task Classe_GetEleves_ExcludesPreviousYearOnly()
        {
            var result = await _classeService.GetElevesAsync(10);

            result.IdAnneeScolaire.Should().Be(101);
            result.Data.Select(e => e.IdEleve).Should().Contain(1).And.NotContain(2);
        }

        [Fact]
        public async Task Paiement_GetByEcole_FiltersByAnneeViaFrais()
        {
            var result = await _paiementService.GetByEcoleAsync(1);

            result.IdAnneeScolaire.Should().Be(101);
            result.Data.Should().HaveCount(1);
            result.Data.Single().IdPaiement.Should().Be(1);
        }

        [Fact]
        public async Task Presence_GetByEcole_FiltersElevesByAnnee()
        {
            var result = await _presenceService.GetAllAsync(1);

            result.IdAnneeScolaire.Should().Be(101);
            result.Data.Should().HaveCount(1);
            result.Data.Single().IdEleve.Should().Be(1);
        }

        public void Dispose() => _context.Dispose();
    }
}
