using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Sync;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Sync;
using KelasiNaBiso.Services.Tarif;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class SyncPullServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly SyncPullService _sut;
        private readonly DateTime _now = DateTime.Now;

        public SyncPullServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var anneeRepo = new AnneeScolaireService(_context);
            var idempotency = new SyncIdempotencyService(_context);
            _sut = new SyncPullService(
                _context, anneeRepo, idempotency, new FraisDuCalculator(_context));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Sync"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "Courante", _now.AddMonths(-3), _now.AddMonths(6)));

            _context.Eleves.AddRange(
                TestDataBuilder.CreateEleve(1, null, "Alpha"),
                TestDataBuilder.CreateEleve(2, null, "Beta"),
                TestDataBuilder.CreateEleve(3, null, "Gamma"));

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100),
                TestDataBuilder.CreateInscription(3, 3, 1, 10, 100));

            _context.Frais.Add(TestDataBuilder.CreateFrais(1, 1, 100, "Minerval", PorteeFrais.Direction, 100));
            _context.FraisDirections.Add(TestDataBuilder.CreateFraisDirection(1, 1));

            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 1,
                IdEleve = 1,
                IdFrais = 1,
                Montant = 40,
                Devise = "USD",
                Statut = true,
                StatutPaiement = "Confirmé",
                DateCreation = _now,
                DatePaiement = _now
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetElevesPage_ReturnsConfirmedElevesWithClasse()
        {
            var page = await _sut.GetElevesPageAsync(1, new SyncRequestDto { PageSize = 10 });

            page.Items.Should().HaveCount(3);
            page.HasMore.Should().BeFalse();
            page.Items.Select(e => e.IdEleve).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            page.Items.Should().OnlyContain(e => e.IdClasse == 10 && e.NomClasse == "6e A");
            page.NextSince.Should().NotBeNullOrWhiteSpace();
            page.Snapshot.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GetElevesPage_RespectsCursorAndPageSize()
        {
            var page1 = await _sut.GetElevesPageAsync(1, new SyncRequestDto { PageSize = 2 });
            page1.Items.Should().HaveCount(2);
            page1.HasMore.Should().BeTrue();
            page1.NextCursor.Should().Be("2");

            var page2 = await _sut.GetElevesPageAsync(1, new SyncRequestDto
            {
                PageSize = 2,
                Cursor = page1.NextCursor,
                Snapshot = page1.Snapshot
            });
            page2.Items.Should().ContainSingle(e => e.IdEleve == 3);
            page2.HasMore.Should().BeFalse();
        }

        [Fact]
        public async Task GetFraisDusPage_OnlyOutstanding_ComputesReste()
        {
            var page = await _sut.GetFraisDusPageAsync(1, new SyncFraisDusRequestDto
            {
                PageSize = 50,
                OnlyOutstanding = true
            });

            // Élève 1 : 100 - 40 = 60 ; élèves 2 et 3 : 100 dus
            page.Items.Should().HaveCount(3);
            var e1 = page.Items.Single(x => x.IdEleve == 1);
            e1.MontantTotal.Should().Be(100m);
            e1.MontantPaye.Should().Be(40m);
            e1.MontantDu.Should().Be(60m);
            e1.CodeDevise.Should().Be("USD");
        }

        [Fact]
        public async Task GetFraisDusPage_ExcludesFullyPaid_WhenOnlyOutstanding()
        {
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 2,
                IdEleve = 2,
                IdFrais = 1,
                Montant = 100,
                Devise = "USD",
                Statut = true,
                StatutPaiement = "Confirmé",
                DateCreation = _now,
                DatePaiement = _now
            });
            await _context.SaveChangesAsync();

            var page = await _sut.GetFraisDusPageAsync(1, new SyncFraisDusRequestDto
            {
                OnlyOutstanding = true
            });

            page.Items.Select(x => x.IdEleve).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetBootstrap_PopulatesElevesAndFraisDus()
        {
            var dto = await _sut.GetBootstrapAsync(1);

            dto.IdEcole.Should().Be(1);
            dto.Eleves.Should().HaveCount(3);
            dto.FraisDus.Should().HaveCount(3);
            dto.Watermark.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void TryParseSince_ReadsWatermarkFormat()
        {
            var ok = SyncPullHelpers.TryParseSince("2026-09-22T08:00:00.0000000Z_638123", out var dt);
            ok.Should().BeTrue();
            dt.Year.Should().Be(2026);
            dt.Month.Should().Be(9);
        }

        public void Dispose() => _context.Dispose();
    }
}
