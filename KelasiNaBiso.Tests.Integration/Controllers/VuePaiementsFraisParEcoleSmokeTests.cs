using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class VuePaiementsFraisParEcoleSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly DateTime _now = DateTime.Now;

        public VuePaiementsFraisParEcoleSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            Seed();
        }

        private void Seed()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KelasiNaBisoDbContext>();

            context.Ecoles.Add(new Ecole
            {
                IdEcole = 1,
                Nom = "Ecole Paiements Vue",
                Statut = true,
                DateCreation = _now
            });

            context.AnneeScolaires.AddRange(
                new AnneeScolaire
                {
                    IdAnneeScolaire = 100,
                    IdEcole = 1,
                    LibelleAnneeScolaire = "2024-2025",
                    DateDebut = _now.AddYears(-1).AddMonths(-3),
                    DateFin = _now.AddMonths(-4),
                    Statut = true,
                    DateCreation = _now
                },
                new AnneeScolaire
                {
                    IdAnneeScolaire = 101,
                    IdEcole = 1,
                    LibelleAnneeScolaire = "2025-2026",
                    DateDebut = _now.AddMonths(-3),
                    DateFin = _now.AddMonths(6),
                    Statut = true,
                    DateCreation = _now
                });

            context.Frais.AddRange(
                new Frais
                {
                    IdFrais = 1,
                    LibelleFrais = "Minerval courant",
                    Montant = 100,
                    Devise = "USD",
                    IdEcole = 1,
                    IdAnneeScolaire = 101,
                    Portee = PorteeFrais.Direction,
                    Statut = true,
                    DateCreation = _now
                },
                new Frais
                {
                    IdFrais = 2,
                    LibelleFrais = "Minerval precedent",
                    Montant = 80,
                    Devise = "USD",
                    IdEcole = 1,
                    IdAnneeScolaire = 100,
                    Portee = PorteeFrais.Direction,
                    Statut = true,
                    DateCreation = _now
                });

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
                    LibelleFrais = "Minerval courant",
                    IdFrais = 1,
                    IdEcole = 1,
                    ReferenceTransaction = "TX-001",
                    DatePaiement = _now,
                    StatutPaiement = "Confirme"
                },
                new VuePaiementsFraisParEcoleDTO
                {
                    IdPaiement = 3,
                    IdEleve = 10,
                    Matricule = "MAT-2026-001",
                    NomCompletFormate = "Jean KABONGO MULUMBA",
                    NomCompletOriginal = "Jean KABONGO MULUMBA",
                    Montant = 40,
                    Devise = "USD",
                    MontantFrais = 80,
                    DeviseFrais = "USD",
                    LibelleFrais = "Minerval precedent",
                    IdFrais = 2,
                    IdEcole = 1,
                    ReferenceTransaction = "TX-PREV",
                    DatePaiement = _now.AddMonths(-6),
                    StatutPaiement = "Confirme"
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
                    MontantFrais = 100,
                    DeviseFrais = "USD",
                    LibelleFrais = "Minerval courant",
                    IdFrais = 1,
                    IdEcole = 1,
                    DatePaiement = _now,
                    StatutPaiement = "Confirme"
                },
                new VuePaiementsFraisParEcoleDTO
                {
                    IdPaiement = 4,
                    IdEleve = 10,
                    Matricule = "MAT-2026-001",
                    NomCompletFormate = "Jean KABONGO MULUMBA",
                    NomCompletOriginal = "Jean KABONGO MULUMBA",
                    Montant = 20,
                    Devise = "USD",
                    MontantFrais = 100,
                    DeviseFrais = "USD",
                    LibelleFrais = "Minerval courant",
                    IdFrais = 1,
                    IdEcole = 1,
                    ReferenceTransaction = "TX-002",
                    DatePaiement = _now.AddDays(-1),
                    StatutPaiement = "Confirme"
                },
                new VuePaiementsFraisParEcoleDTO
                {
                    IdPaiement = 5,
                    IdEleve = 10,
                    Matricule = "MAT-2026-001",
                    NomCompletFormate = "Jean KABONGO MULUMBA",
                    NomCompletOriginal = "Jean KABONGO MULUMBA",
                    Montant = 15,
                    Devise = "USD",
                    MontantFrais = 100,
                    DeviseFrais = "USD",
                    LibelleFrais = "Minerval courant",
                    IdFrais = 1,
                    IdEcole = 1,
                    ReferenceTransaction = "TX-WAIT",
                    DatePaiement = _now.AddHours(-2),
                    StatutPaiement = "En attente"
                });

            context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 1,
                    IdEleve = 10,
                    IdFrais = 1,
                    Montant = 50,
                    Devise = "USD",
                    DatePaiement = _now,
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "REF1"
                },
                new Paiement
                {
                    IdPaiement = 4,
                    IdEleve = 10,
                    IdFrais = 1,
                    Montant = 20,
                    Devise = "USD",
                    DatePaiement = _now.AddDays(-1),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "REF4"
                },
                new Paiement
                {
                    IdPaiement = 5,
                    IdEleve = 10,
                    IdFrais = 1,
                    Montant = 15,
                    Devise = "USD",
                    DatePaiement = _now.AddHours(-2),
                    Statut = true,
                    StatutPaiement = "En attente",
                    ReferencePaiemenet = "REF5"
                },
                new Paiement
                {
                    IdPaiement = 6,
                    IdEleve = 10,
                    IdFrais = 1,
                    Montant = 10,
                    Devise = "USD",
                    DatePaiement = _now.AddHours(-3),
                    Statut = true,
                    StatutPaiement = "Echoue",
                    ReferencePaiemenet = "REF6"
                });
            context.SaveChanges();
        }

        [Fact]
        public async Task GetByEleveMatriculeAsync_Default_ReturnsCurrentYearOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveMatriculeAsync("MAT-2026-001")).ToList();

            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r.IdFrais == 1);
            result.Should().Contain(r => r.IdPaiement == 1 && r.ReferenceTransaction == "TX-001");
            result.Should().NotContain(r => r.IdPaiement == 3);
        }

        [Fact]
        public async Task GetByEleveMatriculeAsync_ExplicitAnnee_ReturnsThatYearOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveMatriculeAsync("MAT-2026-001", idAnneeScolaire: 100)).ToList();

            result.Should().ContainSingle();
            result[0].IdPaiement.Should().Be(3);
            result[0].IdFrais.Should().Be(2);
            result[0].ReferenceTransaction.Should().Be("TX-PREV");
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

        [Fact]
        public async Task GetByEleveAsync_Default_ReturnsCurrentYearOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveAsync(10)).ToList();

            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r.IdFrais == 1);
            result.Should().NotContain(r => r.IdPaiement == 3);
        }

        [Fact]
        public async Task GetByEleveAsync_ExplicitAnnee_ReturnsThatYearOnly()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveAsync(10, idAnneeScolaire: 100)).ToList();

            result.Should().ContainSingle();
            result[0].IdPaiement.Should().Be(3);
            result[0].IdFrais.Should().Be(2);
        }

        [Fact]
        public async Task GetByEleveAsync_EnrichesResteExcludingPendingAndFailed()
        {
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IVuePaiementsFraisParEcoleRepository>();

            var result = (await repo.GetByEleveAsync(10, idAnneeScolaire: 101)).ToList();

            result.Should().OnlyContain(r =>
                r.TotalPayeSurFrais == 70m
                && r.ResteAPayer == 30m
                && r.CodeDeviseReste == "USD");
        }

        public void Dispose() => _factory.Dispose();
    }
}
