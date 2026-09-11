using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
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
    public class PaiementElevePagedResteTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PaiementService _service;
        private readonly DateTime _now = DateTime.Now;

        public PaiementElevePagedResteTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new PaiementService(
                _context,
                NullLogger<PaiementService>.Instance,
                Mock.Of<ICacheService>(),
                Mock.Of<INotificationDispatcher>(),
                Mock.Of<INotificationJobQueue>(),
                Mock.Of<IDashboardHubService>(),
                resolver,
                scope);
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
        public async Task GetByElevePaged_PartialConfirmed_ExcludesPendingAndFailed()
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

            var result = await _service.GetByElevePagedAsync(
                1,
                new PagedRequest { PageNumber = 1, PageSize = 50 },
                101);

            result.Data.Data.Should().HaveCount(4);
            result.Data.Data.Should().OnlyContain(d =>
                d.LibelleFrais == "Minerval"
                && d.MontantFrais == 100
                && d.DeviseFrais == "USD"
                && d.TotalPayeSurFrais == 70m
                && d.ResteAPayer == 30m
                && d.CodeDeviseReste == "USD");
        }

        [Fact]
        public async Task GetByEleveAsync_PartialConfirmed_ExcludesPendingAndFailed()
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

            var result = await _service.GetByEleveAsync(1, 101);

            result.IdAnneeScolaire.Should().Be(101);
            result.Data.Should().HaveCount(4);
            result.Data.Should().OnlyContain(d =>
                d.LibelleFrais == "Minerval"
                && d.MontantFrais == 100
                && d.DeviseFrais == "USD"
                && d.TotalPayeSurFrais == 70m
                && d.ResteAPayer == 30m
                && d.CodeDeviseReste == "USD");
        }

        [Fact]
        public void FromEntity_ComputesResteFromFraisAndTotal()
        {
            var paiement = new Paiement
            {
                IdPaiement = 1,
                IdFrais = 101,
                Montant = 40,
                StatutPaiement = "Confirme"
            };

            var dto = PaiementElevePagedItemDto.FromEntity(
                paiement,
                "Minerval",
                100,
                "USD",
                70m);

            dto.ResteAPayer.Should().Be(30m);
            dto.TotalPayeSurFrais.Should().Be(70m);
            dto.CodeDeviseReste.Should().Be("USD");
        }

        public void Dispose() => _context.Dispose();
    }
}
