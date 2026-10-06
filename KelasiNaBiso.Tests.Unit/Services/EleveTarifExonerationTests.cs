using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Tarif;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveTarifExonerationTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveTarifService _service;

        public EleveTarifExonerationTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(10, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(20, "6e A", idDirection: 10));

            var annee = TestDataBuilder.CreateAnneeScolaire(
                30,
                1,
                "2025-2026",
                DateTime.UtcNow.Date.AddDays(-10),
                DateTime.UtcNow.Date.AddDays(10));
            _context.AnneeScolaires.Add(annee);

            var eleveExonere = TestDataBuilder.CreateEleve(40, null, "Eleve Exonere");
            var eleveSansRegle = TestDataBuilder.CreateEleve(41, null, "Eleve Sans Regle");
            _context.Eleves.AddRange(eleveExonere, eleveSansRegle);
            _context.Inscriptions.AddRange(
                new Inscription
                {
                    Type = "Inscription",
                    IdEleve = 40,
                    IdEcole = 1,
                    IdClasse = 20,
                    IdAnneeScolaire = 30,
                    DateInscription = DateTime.UtcNow,
                    StatutInscription = "Confirmé",
                    Statut = true
                },
                new Inscription
                {
                    Type = "Inscription",
                    IdEleve = 41,
                    IdEcole = 1,
                    IdClasse = 20,
                    IdAnneeScolaire = 30,
                    DateInscription = DateTime.UtcNow,
                    StatutInscription = "Confirmé",
                    Statut = true
                });

            _context.CategoriesEleveTarif.AddRange(
                new CategorieEleveTarif
                {
                    IdCategorieEleveTarif = 50,
                    IdEcole = 1,
                    Code = "BOURSIER",
                    Libelle = "Boursier",
                    Statut = true
                },
                new CategorieEleveTarif
                {
                    IdCategorieEleveTarif = 51,
                    IdEcole = 1,
                    Code = "NORMAL",
                    Libelle = "Normal",
                    Statut = true
                });
            _context.AffectationsEleveCategorieTarif.AddRange(
                new AffectationEleveCategorieTarif
                {
                    IdEleve = 40,
                    IdCategorieEleveTarif = 50,
                    IdAnneeScolaire = 30,
                    DateDebut = DateTime.UtcNow.AddDays(-1)
                },
                new AffectationEleveCategorieTarif
                {
                    IdEleve = 41,
                    IdCategorieEleveTarif = 51,
                    IdAnneeScolaire = 30,
                    DateDebut = DateTime.UtcNow.AddDays(-1)
                });
            _context.ReglesExonerationFrais.Add(new RegleExonerationFrais
            {
                IdEcole = 1,
                IdAnneeScolaire = 30,
                IdCategorieEleveTarif = 50,
                IdFrais = 60,
                TypeRegle = "Totale",
                Statut = true
            });
            _context.SaveChanges();

            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _service = new EleveTarifService(_context, scope);
        }

        [Fact]
        public async Task GetElevesExoneresByClasse_RetourneSeulementLesElevesAvecRegleActive()
        {
            var result = await _service.GetElevesExoneresByClasseAsync(20);

            result.IdEcole.Should().Be(1);
            result.IdAnneeScolaire.Should().Be(30);
            result.Eleves.Should().ContainSingle();
            result.Eleves[0].IdEleve.Should().Be(40);
            result.Eleves[0].CodeCategorie.Should().Be("BOURSIER");
        }

        public void Dispose() => _context.Dispose();
    }
}
