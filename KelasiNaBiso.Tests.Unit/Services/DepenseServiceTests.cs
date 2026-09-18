using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Depense;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class DepenseServiceTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly DepenseService _depenseService;
        private readonly CategorieDepenseService _categorieService;

        public DepenseServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var currency = new CurrencyConversionService(_context);
            _depenseService = new DepenseService(_context, currency);
            _categorieService = new CategorieDepenseService(_context);
            Seed();
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            ecole.CodeDevisePrincipale = "USD";
            _context.Ecoles.Add(ecole);

            _context.DevisesMonetaires.AddRange(
                new DeviseMonetaire
                {
                    IdDeviseMonetaire = 1,
                    IdEcole = 1,
                    CodeDevise = "USD",
                    Libelle = "Dollar",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                },
                new DeviseMonetaire
                {
                    IdDeviseMonetaire = 2,
                    IdEcole = 1,
                    CodeDevise = "CDF",
                    Libelle = "Franc congolais",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });

            _context.TauxChanges.Add(new TauxChange
            {
                IdTauxChange = 1,
                IdEcole = 1,
                CodeDeviseSource = "CDF",
                CodeDeviseCible = "USD",
                Taux = 0.0004m,
                DateEffet = DateTime.UtcNow.AddDays(-10),
                Statut = true,
                DateCreation = DateTime.UtcNow
            });

            _context.CategoriesDepense.Add(new CategorieDepense
            {
                IdCategorieDepense = 1,
                IdEcole = 1,
                NomCategorie = "Fournitures",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task Create_SetsValideeAndSnapshot_SameDevise()
        {
            var dto = new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "Cahiers",
                Montant = 100m,
                CodeDeviseMontant = "USD",
                ModePaiement = "Espèces",
                DateDepense = DateTime.UtcNow
            };

            var created = await _depenseService.CreateAsync(dto, idUtilisateurCreateur: 5);

            created.Statut.Should().Be(DepenseStatuts.Validee);
            created.CodeDevisePrincipale.Should().Be("USD");
            created.TauxVersDevisePrincipale.Should().Be(1m);
            created.MontantDevisePrincipale.Should().Be(100m);
            created.NomCategorie.Should().Be("Fournitures");
        }

        [Fact]
        public async Task Create_ConvertsToPrincipale_WhenCdf()
        {
            var created = await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "Loyer",
                Montant = 250000m,
                CodeDeviseMontant = "CDF"
            }, null);

            created.Statut.Should().Be(DepenseStatuts.Validee);
            created.CodeDevisePrincipale.Should().Be("USD");
            created.TauxVersDevisePrincipale.Should().Be(0.0004m);
            created.MontantDevisePrincipale.Should().Be(100m);
        }

        [Fact]
        public async Task GetMois_DefaultValidee_IncludesCreated()
        {
            await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "A",
                Montant = 50m,
                CodeDeviseMontant = "USD",
                DateDepense = DateTime.UtcNow
            }, null);

            var mois = DateTime.UtcNow.Month;
            var annee = DateTime.UtcNow.Year;
            var report = await _depenseService.GetMoisAsync(1, mois, annee, statut: null);

            report.Depenses.Should().HaveCount(1);
            report.SyntheseDepense.MontantTotal.Should().Be(50m);
            report.SyntheseDepense.NombreValidees.Should().Be(1);
        }

        [Fact]
        public async Task Annuler_ExcludesFromDefaultMoisTotals()
        {
            var created = await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "A",
                Montant = 80m,
                CodeDeviseMontant = "USD",
                DateDepense = DateTime.UtcNow
            }, null);

            await _depenseService.AnnulerAsync(created.IdDepense, "Erreur saisie", 1);

            var report = await _depenseService.GetMoisAsync(
                1, DateTime.UtcNow.Month, DateTime.UtcNow.Year, statut: null);

            report.Depenses.Should().BeEmpty();
            report.SyntheseDepense.MontantTotal.Should().Be(0);

            var annulees = await _depenseService.GetMoisAsync(
                1, DateTime.UtcNow.Month, DateTime.UtcNow.Year, statut: DepenseStatuts.Annulee);
            annulees.Depenses.Should().HaveCount(1);
        }

        [Fact]
        public async Task Update_DoesNotChangeMontant()
        {
            var created = await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "Old",
                Montant = 42m,
                CodeDeviseMontant = "USD"
            }, null);

            var updated = await _depenseService.UpdateAsync(created.IdDepense, new UpdateDepenseDto
            {
                Libelle = "New label",
                Beneficiaire = "Fournisseur"
            });

            updated.Libelle.Should().Be("New label");
            updated.Beneficiaire.Should().Be("Fournisseur");
            updated.Montant.Should().Be(42m);
            updated.MontantDevisePrincipale.Should().Be(42m);
        }

        [Fact]
        public async Task SoftDelete_HidesFromGetById()
        {
            var created = await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "X",
                Montant = 10m,
                CodeDeviseMontant = "USD"
            }, null);

            await _depenseService.SoftDeleteAsync(created.IdDepense);

            var got = await _depenseService.GetByIdAsync(created.IdDepense);
            got.Should().BeNull();
        }

        [Fact]
        public async Task Categorie_CreateAndListByEcole()
        {
            var cat = await _categorieService.CreateAsync(new CreateCategorieDepenseDto
            {
                IdEcole = 1,
                NomCategorie = "Transport",
                Description = "Carburant"
            });

            var list = await _categorieService.GetByEcoleAsync(1);
            list.Should().Contain(c => c.IdCategorieDepense == cat.IdCategorieDepense);
        }

        [Fact]
        public async Task GetPaged_FiltersBySearch()
        {
            await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "Achat craie",
                Montant = 5m,
                CodeDeviseMontant = "USD"
            }, null);
            await _depenseService.CreateAsync(new CreateDepenseDto
            {
                IdEcole = 1,
                IdCategorieDepense = 1,
                Libelle = "Loyer bureau",
                Montant = 200m,
                CodeDeviseMontant = "USD"
            }, null);

            var page = await _depenseService.GetPagedAsync(
                1, null, null, null, null,
                new PagedRequest { PageNumber = 1, PageSize = 20, SearchTerm = "craie" });

            page.Data.Should().HaveCount(1);
            page.Data[0].Libelle.Should().Contain("craie");
        }

        public void Dispose() => _context.Dispose();
    }
}
