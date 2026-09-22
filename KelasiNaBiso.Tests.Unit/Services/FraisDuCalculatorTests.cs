using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Tarif;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class FraisDuCalculatorTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly FraisDuCalculator _sut;
        private readonly DateTime _now = DateTime.UtcNow;

        public FraisDuCalculatorTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _sut = new FraisDuCalculator(_context);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(100, 1, "2025-2026", _now.AddMonths(-3), _now.AddMonths(6)));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(1, null, "Dupont"));
            _context.Frais.Add(TestDataBuilder.CreateFrais(10, 1, 100, "Minerval", PorteeFrais.Direction, 200));

            _context.CategoriesEleveTarif.Add(new CategorieEleveTarif
            {
                IdCategorieEleveTarif = 1,
                IdEcole = 1,
                Code = "BOURSIER",
                Libelle = "Boursier",
                Statut = true,
                DateCreation = _now
            });

            _context.AffectationsEleveCategorieTarif.Add(new AffectationEleveCategorieTarif
            {
                IdAffectationEleveCategorieTarif = 1,
                IdEleve = 1,
                IdCategorieEleveTarif = 1,
                IdAnneeScolaire = 100,
                DateDebut = _now.AddMonths(-1),
                DateFin = null,
                DateCreation = _now
            });

            _context.SaveChanges();
        }

        [Theory]
        [InlineData(TypeRegleExonerationFrais.Totale, 50, 0)]
        [InlineData(TypeRegleExonerationFrais.Pourcentage, 25, 150)] // 200 * 0.75
        [InlineData(TypeRegleExonerationFrais.MontantReduction, 40, 160)]
        [InlineData(TypeRegleExonerationFrais.MontantDuFixe, 75, 75)]
        public void Apply_FourTypes(string type, decimal valeur, decimal expected)
        {
            _sut.Apply(200m, type, valeur).Should().Be(expected);
        }

        [Fact]
        public void Apply_SansRegle_RetourneCatalogue()
        {
            _sut.Apply(200m, null, 0).Should().Be(200m);
        }

        [Fact]
        public async Task GetMontantDuEffectif_SansRegle_EgalCatalogue()
        {
            var du = await _sut.GetMontantDuEffectifAsync(1, 10);
            du.Should().Be(200m);
        }

        [Fact]
        public async Task GetMontantDuEffectif_Pourcentage()
        {
            _context.ReglesExonerationFrais.Add(new RegleExonerationFrais
            {
                IdRegleExonerationFrais = 1,
                IdEcole = 1,
                IdAnneeScolaire = 100,
                IdCategorieEleveTarif = 1,
                IdFrais = 10,
                TypeRegle = TypeRegleExonerationFrais.Pourcentage,
                Valeur = 50,
                Statut = true,
                DateCreation = _now
            });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailAsync(1, 10);
            detail.Should().NotBeNull();
            detail!.MontantCatalogue.Should().Be(200m);
            detail.MontantDuEffectif.Should().Be(100m);
            detail.MontantReduction.Should().Be(100m);
            detail.CodeCategorie.Should().Be("BOURSIER");
            detail.TypeRegle.Should().Be(TypeRegleExonerationFrais.Pourcentage);
        }

        [Fact]
        public async Task ChangementCategorie_NeModifiePasPaiements()
        {
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 1,
                IdEleve = 1,
                IdFrais = 10,
                Montant = 50,
                Devise = "USD",
                Statut = true,
                StatutPaiement = "Confirmé",
                DateCreation = _now,
                DatePaiement = _now
            });
            _context.ReglesExonerationFrais.Add(new RegleExonerationFrais
            {
                IdRegleExonerationFrais = 1,
                IdEcole = 1,
                IdAnneeScolaire = 100,
                IdCategorieEleveTarif = 1,
                IdFrais = 10,
                TypeRegle = TypeRegleExonerationFrais.MontantDuFixe,
                Valeur = 80,
                Statut = true,
                DateCreation = _now
            });
            await _context.SaveChangesAsync();

            var avant = await _context.Paiements.FindAsync(1);
            avant!.Montant.Should().Be(50);

            // Clôturer et réaffecter sans règle → dû = catalogue, paiements inchangés
            var aff = await _context.AffectationsEleveCategorieTarif.FindAsync(1);
            aff!.DateFin = _now;
            await _context.SaveChangesAsync();

            var apres = await _context.Paiements.FindAsync(1);
            apres!.Montant.Should().Be(50);

            var du = await _sut.GetMontantDuEffectifAsync(1, 10);
            du.Should().Be(200m); // plus de catégorie active
        }

        [Fact]
        public async Task GetDetail_ResteUtiliseDuEffectif()
        {
            _context.ReglesExonerationFrais.Add(new RegleExonerationFrais
            {
                IdRegleExonerationFrais = 1,
                IdEcole = 1,
                IdAnneeScolaire = 100,
                IdCategorieEleveTarif = 1,
                IdFrais = 10,
                TypeRegle = TypeRegleExonerationFrais.MontantDuFixe,
                Valeur = 100,
                Statut = true,
                DateCreation = _now
            });
            _context.Paiements.Add(new Paiement
            {
                IdPaiement = 2,
                IdEleve = 1,
                IdFrais = 10,
                Montant = 40,
                Devise = "USD",
                Statut = true,
                StatutPaiement = "Confirmé",
                DateCreation = _now,
                DatePaiement = _now
            });
            await _context.SaveChangesAsync();

            var detail = await _sut.GetDetailAsync(1, 10);
            detail!.ResteAPayer.Should().Be(60m); // 100 - 40
            detail.MontantPaye.Should().Be(40m);
        }

        public void Dispose() => _context.Dispose();
    }
}
