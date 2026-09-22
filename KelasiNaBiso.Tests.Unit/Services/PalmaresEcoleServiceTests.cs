using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PalmaresEcoleServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly PalmaresEcoleService _sut;
        private readonly DateTime _now = DateTime.Now;

        public PalmaresEcoleServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _sut = new PalmaresEcoleService(_context, scope, new PeriodeCotationResolver(_context));
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Palmares"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1, "Secondaire"));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(11, "6e B", idDirection: 1));

            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(
                100, 1, "2025-2026",
                debut: _now.AddMonths(-3),
                fin: _now.AddMonths(9)));

            _context.PeriodesCotation.AddRange(
                new PeriodeCotation { IdPeriode = 1, Code = "T1", Libelle = "Trimestre 1", Ordre = 1, Statut = true },
                new PeriodeCotation { IdPeriode = 2, Code = "T2", Libelle = "Trimestre 2", Ordre = 2, Statut = true });

            _context.Tuteurs.Add(TestDataBuilder.CreateTuteur(1, "Parent"));
            var e1 = TestDataBuilder.CreateEleve(1, 1, "Alpha");
            e1.Matricule = "M1";
            var e2 = TestDataBuilder.CreateEleve(2, 1, "Beta");
            e2.Matricule = "M2";
            var e3 = TestDataBuilder.CreateEleve(3, 1, "Gamma");
            e3.Matricule = "M3";
            _context.Eleves.AddRange(e1, e2, e3);

            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 2, 1, 10, 100),
                TestDataBuilder.CreateInscription(3, 3, 1, 11, 100));

            // T1 : moyennes 18, 16, 16 → rangs 1, 2, 2
            _context.BulletinsFiges.AddRange(
                new BulletinFige
                {
                    IdBulletinFige = 1,
                    IdEleve = 1,
                    IdAnneeScolaire = 100,
                    IdPeriode = 1,
                    MoyenneGenerale = 18,
                    Decision = "Passe",
                    PayloadJson = "{}",
                    DateValidation = _now.AddMonths(-2)
                },
                new BulletinFige
                {
                    IdBulletinFige = 2,
                    IdEleve = 2,
                    IdAnneeScolaire = 100,
                    IdPeriode = 1,
                    MoyenneGenerale = 16,
                    PayloadJson = "{}",
                    DateValidation = _now.AddMonths(-2)
                },
                new BulletinFige
                {
                    IdBulletinFige = 3,
                    IdEleve = 3,
                    IdAnneeScolaire = 100,
                    IdPeriode = 1,
                    MoyenneGenerale = 16,
                    PayloadJson = "{}",
                    DateValidation = _now.AddMonths(-2)
                },
                // T2 : seule Beta figée → P1 doit choisir T2
                new BulletinFige
                {
                    IdBulletinFige = 4,
                    IdEleve = 2,
                    IdAnneeScolaire = 100,
                    IdPeriode = 2,
                    MoyenneGenerale = 19,
                    PayloadJson = "{}",
                    DateValidation = _now.AddDays(-1)
                });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Palmares_WithPeriode_CompetitionRanks_AndLimit()
        {
            var dto = await _sut.GetPalmaresEcoleAsync(
                idEcole: 1,
                idAnneeScolaire: 100,
                idPeriode: 1,
                limit: 10);

            dto.CodePeriode.Should().Be("T1");
            dto.EffectifPrisEnCompte.Should().Be(3);
            dto.Lignes.Should().HaveCount(3);
            dto.Lignes[0].Matricule.Should().Be("M1");
            dto.Lignes[0].Rang.Should().Be(1);
            dto.Lignes[0].MoyenneGenerale.Should().Be(18);
            dto.Lignes[1].Rang.Should().Be(2);
            dto.Lignes[2].Rang.Should().Be(2);
        }

        [Fact]
        public async Task Palmares_WithoutPeriode_UsesLatestFigePeriod_P1()
        {
            var dto = await _sut.GetPalmaresEcoleAsync(
                idEcole: 1,
                idAnneeScolaire: 100);

            dto.CodePeriode.Should().Be("T2");
            dto.Lignes.Should().ContainSingle();
            dto.Lignes[0].Matricule.Should().Be("M2");
            dto.Lignes[0].MoyenneGenerale.Should().Be(19);
        }

        [Fact]
        public async Task Palmares_FilterByClasse()
        {
            var dto = await _sut.GetPalmaresEcoleAsync(
                idEcole: 1,
                idAnneeScolaire: 100,
                idPeriode: 1,
                idClasse: 11);

            dto.IdClasse.Should().Be(11);
            dto.Lignes.Should().ContainSingle();
            dto.Lignes[0].Matricule.Should().Be("M3");
        }

        [Fact]
        public async Task Palmares_RespectsLimit()
        {
            var dto = await _sut.GetPalmaresEcoleAsync(
                idEcole: 1,
                idAnneeScolaire: 100,
                idPeriode: 1,
                limit: 1);

            dto.EffectifPrisEnCompte.Should().Be(3);
            dto.Lignes.Should().ContainSingle();
            dto.Lignes[0].Rang.Should().Be(1);
        }

        [Fact]
        public void AssignCompetitionRanks_ExAequo()
        {
            var lignes = new List<PalmaresEcoleLigneDto>
            {
                new() { NomComplet = "A", MoyenneGenerale = 10 },
                new() { NomComplet = "B", MoyenneGenerale = 12 },
                new() { NomComplet = "C", MoyenneGenerale = 12 },
                new() { NomComplet = "D", MoyenneGenerale = null }
            };

            PalmaresEcoleService.AssignCompetitionRanks(lignes);

            lignes.Single(l => l.NomComplet == "B").Rang.Should().Be(1);
            lignes.Single(l => l.NomComplet == "C").Rang.Should().Be(1);
            lignes.Single(l => l.NomComplet == "A").Rang.Should().Be(3);
            lignes.Single(l => l.NomComplet == "D").Rang.Should().BeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
