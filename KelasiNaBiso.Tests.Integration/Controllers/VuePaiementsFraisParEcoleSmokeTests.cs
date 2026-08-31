using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class VuePaiementsFraisParEcoleSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;

        public VuePaiementsFraisParEcoleSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            context.VuePaiementsFraisParEcole.AddRange(
                new VuePaiementsFraisParEcoleDTO
                {
                    IdPaiement = 1,
                    IdEleve = 10,
                    Matricule = "MAT-2026-001",
                    NomCompletFormate = "Jean KABONGO MULUMBA",
                    NomCompletOriginal = "Jean KABONGO MULUMBA",
                    Montant = 50,
                    Devise = "USD",
                    MontantFrais = 100,
                    DeviseFrais = "USD",
                    LibelleFrais = "Minerval",
                    IdFrais = 1,
                    IdEcole = 1,
                    ReferenceTransaction = "TX-001",
                    DatePaiement = DateTime.Now
                },
                new VuePaiementsFraisParEcoleDTO
                {
                    IdPaiement = 2,
                    IdEleve = 20,
                    Matricule = "MAT-2026-002",
                    NomCompletFormate = "Marie TSHILOMBO",
                    NomCompletOriginal = "Marie TSHILOMBO",
                    Montant = 30,
                    Devise = "USD",
                    IdEcole = 1,
                    DatePaiement = DateTime.Now
                });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByEleveMatriculeAsync_ReturnsMatchingRows_WithNomCompletFormate()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveMatriculeAsync("MAT-2026-001")).ToList();

            result.Should().ContainSingle();
            result[0].NomCompletFormate.Should().Be("Jean KABONGO MULUMBA");
            result[0].ReferenceTransaction.Should().Be("TX-001");
            result[0].Matricule.Should().Contain("MAT-2026-001");
        }

        [Fact]
        public async Task GetByEleveMatriculeAsync_DoesNotReturnOtherMatricules()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveMatriculeAsync("MAT-2026-002")).ToList();

            result.Should().ContainSingle();
            result.Should().NotContain(r => r.Matricule != null && r.Matricule.Contains("MAT-2026-001"));
        }

        public void Dispose() => _factory.Dispose();
    }
}
