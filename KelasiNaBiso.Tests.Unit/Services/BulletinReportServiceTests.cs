using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Reporting;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class BulletinReportServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly BulletinReportService _reportService;
        private readonly DateTime _now = DateTime.Now;

        public BulletinReportServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var bulletinService = new BulletinService(_context, resolver, scopeFactory.Object);

            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            _reportService = new BulletinReportService(
                bulletinService,
                env.Object,
                NullLogger<BulletinReportService>.Instance);

            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2025-2026", _now.AddMonths(-3), _now.AddMonths(6)));

            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));

            _context.Cours.Add(new Cours
            {
                IdCours = 50,
                NomCours = "Math",
                IdClasse = 10,
                Statut = true,
                DateCreation = DateTime.Now
            });

            _context.Evaluations.Add(new Evaluation
            {
                IdEvaluation = 1,
                TypeEvaluation = "Interro",
                TitreEvaluation = "Math 1",
                Periode = "Trimestre 1",
                Coefficient = 2,
                IdCours = 50,
                IdClasse = 10,
                Statut = true,
                DateCreation = DateTime.Now
            });

            _context.Notes.Add(new Note
            {
                IdNote = 1,
                NoteObtenue = 14,
                Appreciation = "",
                DateEvaluation = _now,
                IdProfesseur = 1,
                IdEleve = 1,
                IdEvaluation = 1,
                IdAnneeScolaire = 100,
                Statut = true
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetElevePdf_ReturnsValidPdfBytes()
        {
            if (!FastReportPdfTestEnvironment.IsAvailable)
                return;

            var pdf = await _reportService.GetElevePdfAsync(1, 100, "Trimestre 1");

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
            Encoding.ASCII.GetString(pdf, 0, 4).Should().Be("%PDF");
        }

        [Fact]
        public async Task GetElevePdf_UnknownEleve_ThrowsKeyNotFound()
        {
            var act = () => _reportService.GetElevePdfAsync(999, 100, "Trimestre 1");

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        public void Dispose() => _context.Dispose();
    }
}
