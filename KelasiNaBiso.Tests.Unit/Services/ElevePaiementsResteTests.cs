using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class ElevePaiementsResteTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _service;
        private readonly DateTime _now = DateTime.Now;

        public ElevePaiementsResteTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new EleveService(
                _context,
                Mock.Of<IInscriptionRepository>(),
                resolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _now.AddMonths(-3), _now.AddMonths(6), statut: true));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Alice"));
            _context.Inscriptions.Add(TestDataBuilder.CreateInscription(1, 1, 1, 10, 101));
            _context.Frais.Add(new Frais
            {
                IdFrais = 101,
                LibelleFrais = "Minerval",
                Montant = 100,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 101,
                Portee = PorteeFrais.Direction,
                Statut = true,
                DateCreation = _now
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetPaiementsAsync_PartialConfirmed_ExcludesPendingAndFailed()
        {
            _context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 1,
                    IdEleve = 1,
                    IdFrais = 101,
                    Montant = 40,
                    Devise = "USD",
                    DatePaiement = _now.AddDays(-2),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "REF1"
                },
                new Paiement
                {
                    IdPaiement = 2,
                    IdEleve = 1,
                    IdFrais = 101,
                    Montant = 30,
                    Devise = "USD",
                    DatePaiement = _now.AddDays(-1),
                    Statut = true,
                    StatutPaiement = "Confirmé",
                    ReferencePaiemenet = "REF2"
                },
                new Paiement
                {
                    IdPaiement = 3,
                    IdEleve = 1,
                    IdFrais = 101,
                    Montant = 20,
                    Devise = "USD",
                    DatePaiement = _now,
                    Statut = true,
                    StatutPaiement = "En attente",
                    ReferencePaiemenet = "REF3"
                },
                new Paiement
                {
                    IdPaiement = 4,
                    IdEleve = 1,
                    IdFrais = 101,
                    Montant = 15,
                    Devise = "USD",
                    DatePaiement = _now.AddHours(-1),
                    Statut = true,
                    StatutPaiement = "Echoue",
                    ReferencePaiemenet = "REF4"
                });
            await _context.SaveChangesAsync();

            var result = (await _service.GetPaiementsAsync(1)).ToList();

            result.Should().HaveCount(4);
            result.Should().OnlyContain(d =>
                d.LibelleFrais == "Minerval"
                && d.MontantFrais == 100
                && d.DeviseFrais == "USD"
                && d.TotalPayeSurFrais == 70m
                && d.ResteAPayer == 30m
                && d.CodeDeviseReste == "USD");
        }

        [Fact]
        public async Task GetPaiementsAsync_WithLibelleAnnee_FiltersAndKeepsReste()
        {
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(99, 1, "2024-2025",
                    _now.AddYears(-2), _now.AddYears(-1), statut: true));
            _context.Frais.Add(new Frais
            {
                IdFrais = 99,
                LibelleFrais = "Ancien",
                Montant = 50,
                Devise = "USD",
                IdEcole = 1,
                IdAnneeScolaire = 99,
                Portee = PorteeFrais.Direction,
                Statut = true,
                DateCreation = _now
            });
            _context.Paiements.AddRange(
                new Paiement
                {
                    IdPaiement = 10,
                    IdEleve = 1,
                    IdFrais = 101,
                    Montant = 25,
                    DatePaiement = _now,
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "CUR"
                },
                new Paiement
                {
                    IdPaiement = 11,
                    IdEleve = 1,
                    IdFrais = 99,
                    Montant = 50,
                    DatePaiement = _now.AddYears(-1),
                    Statut = true,
                    StatutPaiement = "Confirme",
                    ReferencePaiemenet = "OLD"
                });
            await _context.SaveChangesAsync();

            var result = (await _service.GetPaiementsAsync(1, "2025-2026")).ToList();

            result.Should().ContainSingle();
            result[0].IdFrais.Should().Be(101);
            result[0].ResteAPayer.Should().Be(75m);
            result[0].TotalPayeSurFrais.Should().Be(25m);
        }

        public void Dispose() => _context.Dispose();
    }
}
