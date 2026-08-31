using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class InscriptionActiveResolverTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly InscriptionActiveResolver _resolver;

        public InscriptionActiveResolverTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _resolver = new InscriptionActiveResolver(_context);
            Seed();
        }

        private void Seed()
        {
            var ecole1 = TestDataBuilder.CreateEcole(1, "Ecole A");
            var ecole2 = TestDataBuilder.CreateEcole(2, "Ecole B");
            var dir1 = TestDataBuilder.CreateDirection(1, 1);
            var dir2 = TestDataBuilder.CreateDirection(2, 2);
            var classe1 = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);
            var classe2 = TestDataBuilder.CreateClasse(20, "5e B", idDirection: 2);
            var annee1 = TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025", new DateTime(2024, 9, 1), new DateTime(2025, 6, 30));
            var annee2 = TestDataBuilder.CreateAnneeScolaire(200, 2, "2024-2025", new DateTime(2024, 9, 1), new DateTime(2025, 6, 30));
            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent Dupont", telephone: "+243900000001");
            var eleve = TestDataBuilder.CreateEleve(1, 1);

            _context.Ecoles.AddRange(ecole1, ecole2);
            _context.Directions.AddRange(dir1, dir2);
            _context.Classes.AddRange(classe1, classe2);
            _context.AnneeScolaires.AddRange(annee1, annee2);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.Add(eleve);
            _context.Inscriptions.AddRange(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100),
                TestDataBuilder.CreateInscription(2, 1, 2, 20, 200, dateInscription: DateTime.Now.AddDays(1)));
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetInscriptionActiveAsync_ReturnsLatestConfirmed_WhenMultipleYears()
        {
            var inscription = await _resolver.GetInscriptionActiveAsync(1);

            inscription.Should().NotBeNull();
            inscription!.IdEcole.Should().Be(2);
            inscription.IdClasse.Should().Be(20);
        }

        [Fact]
        public async Task GetClasseCouranteAsync_UsesInscription_NotLegacyIdClasse()
        {
            var idClasse = await _resolver.GetClasseCouranteAsync(1);
            idClasse.Should().Be(20);
        }

        [Fact]
        public async Task FilterTuteursInEcole_FindsTuteurViaChildInscription_NotIdEcole()
        {
            var tuteursEcole2 = await _resolver.FilterTuteursInEcole(_context.Tuteurs, 2).ToListAsync();
            tuteursEcole2.Should().ContainSingle(t => t.IdTuteur == 1);
        }

        [Fact]
        public async Task ParentClasses_ResolvedViaInscription_NotEleveIdClasse()
        {
            // Miroir de DevoirADomicileHub : classes parents via Inscription, pas Eleves.IdClasse
            var classesEnfants = await _context.Inscriptions
                .AsNoTracking()
                .Where(i =>
                    i.Statut == true
                    && i.Eleve != null
                    && i.Eleve.IdTuteur == 1
                    && i.Eleve.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .Select(i => i.IdClasse)
                .Distinct()
                .ToListAsync();

            classesEnfants.Should().BeEquivalentTo(new[] { 10, 20 });
        }

        [Fact]
        public void IsActiveConfirmed_RejectsEnAttente()
        {
            InscriptionActiveRules.IsActiveConfirmed(new Inscription { Statut = true, StatutInscription = "En attente" })
                .Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("En attente")]
        [InlineData("EN_ATTENTE")]
        [InlineData("en_attente")]
        [InlineData(" En Attente ")]
        public void NormalizeStatutInscriptionForCreate_MapsPendingToConfirme(string? input)
        {
            InscriptionActiveRules.NormalizeStatutInscriptionForCreate(input)
                .Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Theory]
        [InlineData("Annulé")]
        [InlineData("Confirmé")]
        [InlineData("Confirme")]
        public void NormalizeStatutInscriptionForCreate_KeepsExplicitNonPending(string input)
        {
            InscriptionActiveRules.NormalizeStatutInscriptionForCreate(input)
                .Should().Be(input);
        }

        [Theory]
        [InlineData("Brouillon")]
        [InlineData("PENDING")]
        [InlineData("draft")]
        public void NormalizeStatutInscriptionForCreate_UnknownValue_MapsToConfirme(string input)
        {
            InscriptionActiveRules.NormalizeStatutInscriptionForCreate(input)
                .Should().Be(InscriptionActiveRules.StatutConfirme);
        }

        [Fact]
        public async Task FilterElevesInEcole_WithAnnee_FiltersToThatYearOnly()
        {
            var elevesAnnee100 = await _resolver.FilterElevesInEcole(_context.Eleves, 1, 100).ToListAsync();
            var elevesAnnee200 = await _resolver.FilterElevesInEcole(_context.Eleves, 2, 200).ToListAsync();

            elevesAnnee100.Should().ContainSingle(e => e.IdEleve == 1);
            elevesAnnee200.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task FilterElevesInDirection_WithAnnee_FiltersToClassesInThatDirection()
        {
            var elevesDir1Annee100 = await _resolver.FilterElevesInDirection(_context.Eleves, 1, 100).ToListAsync();
            var elevesDir2Annee200 = await _resolver.FilterElevesInDirection(_context.Eleves, 2, 200).ToListAsync();

            elevesDir1Annee100.Should().ContainSingle(e => e.IdEleve == 1);
            elevesDir2Annee200.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task FilterElevesInEcole_WithoutAnnee_FindsViaAnyActiveInscription()
        {
            var elevesEcole1 = await _resolver.FilterElevesInEcole(_context.Eleves, 1).ToListAsync();
            elevesEcole1.Should().ContainSingle(e => e.IdEleve == 1);
        }

        [Fact]
        public async Task GetClasseCouranteAsync_WithAnnee_ReturnsInscriptionForThatYear()
        {
            var classe = await _resolver.GetClasseCouranteAsync(1, 100);
            classe.Should().Be(10);
        }

        public void Dispose() => _context.Dispose();
    }
}
