using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Paiement;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DashboardAnneeScolaireTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DashboardController _controller;
        private readonly DateTime _now = DateTime.Now;

        public DashboardAnneeScolaireTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, anneeRepo);

            var cacheMock = new Mock<ICacheService>();
            cacheMock
                .Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<DashboardPresenceDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns<string, Func<Task<DashboardPresenceDto>>, TimeSpan?>((_, factory, _) => factory());
            cacheMock
                .Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<DashboardPaiementDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns<string, Func<Task<DashboardPaiementDto>>, TimeSpan?>((_, factory, _) => factory());

            var presenceReporting = new PresenceReportingService(
                _context, cacheMock.Object, resolver, scope);

            var paiementService = new PaiementService(
                _context,
                NullLogger<PaiementService>.Instance,
                cacheMock.Object,
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                scope);

            _controller = new DashboardController(
                presenceReporting,
                paiementService,
                _context,
                NullLogger<DashboardController>.Instance,
                Mock.Of<ICurrentUserService>(),
                scope,
                resolver);

            SeedCurrentYearScenario();
        }

        private void SeedCurrentYearScenario()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));

            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    _now.AddYears(-1), _now.AddMonths(-6)),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6)));

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

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetDashboardGlobal_DefaultYear_CountsOnlyCurrentYearStudents()
        {
            var actionResult = await _controller.GetDashboardGlobal(1);

            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = ok.Value.Should().BeOfType<DashboardGlobalDto>().Subject;

            dto.IdAnneeScolaire.Should().Be(101);
            dto.LibelleAnneeScolaire.Should().Be("2025-2026");
            dto.Statistiques.NombreEleves.Should().Be(1);
            dto.RepartitionEleves.TotalEleves.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardGlobal_ExplicitPastYear_CountsOnlyThatYear()
        {
            var actionResult = await _controller.GetDashboardGlobal(1, idAnneeScolaire: 100);

            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = ok.Value.Should().BeOfType<DashboardGlobalDto>().Subject;

            dto.IdAnneeScolaire.Should().Be(100);
            dto.Statistiques.NombreEleves.Should().Be(1);
            dto.RepartitionEleves.TotalEleves.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardGlobal_VacationGap_DefaultsToUpcomingYearB()
        {
            using var vacationContext = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(vacationContext);
            var scope = new EleveAnneeScopeHelper(
                vacationContext, resolver, new AnneeScolaireService(vacationContext));

            var finA = _now.AddDays(-30);
            var debutB = _now.AddDays(30);

            vacationContext.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Vacances"));
            vacationContext.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            vacationContext.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            vacationContext.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025",
                    finA.AddMonths(-10), finA),
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    debutB, debutB.AddMonths(10)));
            vacationContext.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            vacationContext.Inscriptions.Add(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            vacationContext.Frais.Add(new Frais
            {
                IdFrais = 1,
                LibelleFrais = "Frais B",
                Montant = 100,
                Devise = "USD",
                IdDirection = 1,
                IdAnneeScolaire = 101,
                Statut = true,
                DateCreation = DateTime.Now
            });
            vacationContext.SaveChanges();

            var cacheMock = new Mock<ICacheService>();
            cacheMock
                .Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<DashboardPresenceDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns<string, Func<Task<DashboardPresenceDto>>, TimeSpan?>((_, factory, _) => factory());
            cacheMock
                .Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<DashboardPaiementDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns<string, Func<Task<DashboardPaiementDto>>, TimeSpan?>((_, factory, _) => factory());

            var controller = new DashboardController(
                new PresenceReportingService(vacationContext, cacheMock.Object, resolver, scope),
                new PaiementService(
                    vacationContext,
                    NullLogger<PaiementService>.Instance,
                    cacheMock.Object,
                    Mock.Of<INotificationDispatcher>(),
                    Mock.Of<INotificationJobQueue>(),
                    Mock.Of<IDashboardHubService>(),
                    resolver,
                    scope),
                vacationContext,
                NullLogger<DashboardController>.Instance,
                Mock.Of<ICurrentUserService>(),
                scope,
                resolver);

            var actionResult = await controller.GetDashboardGlobal(1);

            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = ok.Value.Should().BeOfType<DashboardGlobalDto>().Subject;

            dto.IdAnneeScolaire.Should().Be(101);
            dto.Statistiques.NombreEleves.Should().Be(1);
        }

        public void Dispose() => _context.Dispose();
    }
}
