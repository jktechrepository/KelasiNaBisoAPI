using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DeviseMonetaireCatalogTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly CurrencyConversionService _conversion;

        public DeviseMonetaireCatalogTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _conversion = new CurrencyConversionService(_context);

            _context.Ecoles.Add(new Ecole
            {
                IdEcole = 1,
                Nom = "École Test",
                CodeDevisePrincipale = "USD",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task EnsureDefaults_SeedsUsdAndCdf_AndExtraPrincipale()
        {
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "EUR");

            var codes = await _context.DevisesMonetaires
                .Where(d => d.IdEcole == 1)
                .Select(d => d.CodeDevise)
                .OrderBy(c => c)
                .ToListAsync();

            codes.Should().BeEquivalentTo(new[] { "CDF", "EUR", "USD" });
        }

        [Fact]
        public async Task EnsureDefaults_IsIdempotent()
        {
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "USD");
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "USD");

            (await _context.DevisesMonetaires.CountAsync(d => d.IdEcole == 1)).Should().Be(2);
        }

        [Fact]
        public async Task ConvertAsync_Fails_WhenSourceNotInCatalog()
        {
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "USD");

            var result = await _conversion.ConvertAsync(1, "EUR", "USD", 10m, DateTime.UtcNow);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("EUR");
        }

        [Fact]
        public async Task ConvertAsync_Fails_WhenDeviseInactive()
        {
            _context.DevisesMonetaires.Add(new DeviseMonetaire
            {
                IdEcole = 1,
                CodeDevise = "USD",
                Libelle = "Dollar",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            _context.DevisesMonetaires.Add(new DeviseMonetaire
            {
                IdEcole = 1,
                CodeDevise = "CDF",
                Libelle = "Franc",
                Statut = false,
                DateCreation = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var result = await _conversion.ConvertAsync(1, "USD", "CDF", 10m, DateTime.UtcNow);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("CDF");
        }

        [Fact]
        public async Task ConvertAsync_SameCurrency_Succeeds_WhenActiveInCatalog()
        {
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "USD");

            var result = await _conversion.ConvertAsync(1, "USD", "USD", 12.5m, DateTime.UtcNow);

            result.Success.Should().BeTrue();
            result.Taux.Should().Be(1m);
            result.MontantConverti.Should().Be(12.5m);
        }

        [Fact]
        public async Task Catalog_DetectsDuplicateCodeForSameEcole()
        {
            await DeviseMonetaireSeedHelper.EnsureDefaultsAsync(_context, 1, "USD");

            var duplicateExists = await _context.DevisesMonetaires
                .AnyAsync(d => d.IdEcole == 1 && d.CodeDevise == "USD");

            duplicateExists.Should().BeTrue();
        }

        [Fact]
        public async Task IsActiveDeviseAsync_ReturnsFalse_WhenMissing()
        {
            (await _conversion.IsActiveDeviseAsync(1, "USD")).Should().BeFalse();
        }

        public void Dispose() => _context.Dispose();
    }
}
