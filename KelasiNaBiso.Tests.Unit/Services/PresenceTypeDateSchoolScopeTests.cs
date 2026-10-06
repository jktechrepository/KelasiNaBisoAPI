using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Notifications;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public sealed class PresenceTypeDateSchoolScopeTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PresenceService _service;

        public PresenceTypeDateSchoolScopeTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(
                _context,
                resolver,
                new AnneeScolaireService(_context));

            _service = new PresenceService(
                _context,
                NullLogger<PresenceService>.Instance,
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                scope);

            Seed();
        }

        [Fact]
        public async Task GetByTypePersonneAndDateAsync_StudentsAreScopedToSchoolYearCoveringDate()
        {
            var currentYearDate = new DateTime(2025, 9, 10);
            var presences = await _service.GetByTypePersonneAndDateAsync(
                "ELEVE",
                currentYearDate,
                idEcole: 1);

            presences.Select(p => p.IdEleve.GetValueOrDefault()).Should().Equal(2);

            var previousYearDate = new DateTime(2025, 6, 10);
            var historicalPresences = await _service.GetByTypePersonneAndDateAsync(
                "ELEVE",
                previousYearDate,
                idEcole: 1);

            historicalPresences
                .Select(p => p.IdEleve.GetValueOrDefault())
                .Should()
                .BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetByTypePersonneAndDateAsync_AgentsAreScopedByTheirSchool()
        {
            var presences = await _service.GetByTypePersonneAndDateAsync(
                "AGENT",
                new DateTime(2025, 9, 10),
                idEcole: 1);

            presences.Select(p => p.IdAgent.GetValueOrDefault()).Should().Equal(11);
        }

        private void Seed()
        {
            _context.AnneeScolaires.AddRange(
                TestDataBuilder.CreateAnneeScolaire(
                    100, 1, "2024-2025", new DateTime(2024, 9, 1), new DateTime(2025, 6, 30)),
                TestDataBuilder.CreateAnneeScolaire(
                    101, 1, "2025-2026", new DateTime(2025, 9, 1), new DateTime(2026, 6, 30)),
                TestDataBuilder.CreateAnneeScolaire(
                    201, 2, "2025-2026", new DateTime(2025, 9, 1), new DateTime(2026, 6, 30)));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, null, "Transferred"),
                TestDataBuilder.CreateEleve(2, null, "Current school"),
                TestDataBuilder.CreateEleve(3, null, "Previous year only"),
                TestDataBuilder.CreateEleve(4, null, "Other school"),
                TestDataBuilder.CreateEleve(5, null, "Unconfirmed enrollment"));
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 1, 2, 20, 201),
                TestDataBuilder.CreateInscription(3, 2, 1, 10, 101),
                TestDataBuilder.CreateInscription(4, 3, 1, 10, 100),
                TestDataBuilder.CreateInscription(5, 4, 2, 20, 201),
                TestDataBuilder.CreateInscription(6, 5, 1, 10, 101, statutInscription: "En attente"));

            _context.Agents.AddRange(
                TestDataBuilder.CreateAgent(11, "School one", idEcole: 1),
                TestDataBuilder.CreateAgent(22, "School two", idEcole: 2));

            _context.Presences.AddRange(
                CreatePresence(1, "ELEVE", idEleve: 1, date: new DateTime(2025, 6, 10)),
                CreatePresence(2, "ELEVE", idEleve: 3, date: new DateTime(2025, 6, 10)),
                CreatePresence(3, "ELEVE", idEleve: 1, date: new DateTime(2025, 9, 10)),
                CreatePresence(4, "ELEVE", idEleve: 2, date: new DateTime(2025, 9, 10)),
                CreatePresence(5, "ELEVE", idEleve: 3, date: new DateTime(2025, 9, 10)),
                CreatePresence(6, "ELEVE", idEleve: 4, date: new DateTime(2025, 9, 10)),
                CreatePresence(7, "AGENT", idAgent: 11, date: new DateTime(2025, 9, 10)),
                CreatePresence(8, "AGENT", idAgent: 22, date: new DateTime(2025, 9, 10)),
                CreatePresence(9, "ELEVE", idEleve: 5, date: new DateTime(2025, 9, 10)));

            _context.SaveChanges();
        }

        private static Presence CreatePresence(
            int id,
            string type,
            DateTime date,
            int? idEleve = null,
            int? idAgent = null) =>
            new()
            {
                IdPresence = id,
                TypePresence = type,
                IdEleve = idEleve,
                IdAgent = idAgent,
                DateDuJour = date,
                HeureArrivee = new TimeSpan(7, 30, 0),
                Statut = true
            };

        public void Dispose() => _context.Dispose();
    }
}
