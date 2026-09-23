using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class EleveAffecterTuteurTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveService _sut;
        private readonly DateTime _now = DateTime.Now;

        public EleveAffecterTuteurTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _sut = new EleveService(
                _context,
                Mock.Of<IInscriptionRepository>(),
                resolver,
                scope,
                NullLogger<EleveService>.Instance);
            Seed();
        }

        private void Seed()
        {
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole A"));
            _context.Tuteurs.AddRange(
                TestDataBuilder.CreateTuteur(1, "Parent Un", telephone: "+243810000001"),
                TestDataBuilder.CreateTuteur(2, "Parent Deux", telephone: "+243810000002"),
                TestDataBuilder.CreateTuteur(3, "Parent Inactif", telephone: "+243810000003", statut: false));
            _context.Eleves.Add(TestDataBuilder.CreateEleve(10, 1, "Eleve Test"));
            _context.SaveChanges();
        }

        [Fact]
        public async Task AffecterTuteur_RemplaceIdTuteur()
        {
            var result = await _sut.AffecterTuteurAsync(10, 2);

            result.IdEleve.Should().Be(10);
            result.IdTuteur.Should().Be(2);
            result.IdTuteurPrecedent.Should().Be(1);
            result.NomCompletTuteur.Should().Be("Parent Deux");

            var eleve = await _context.Eleves.FindAsync(10);
            eleve!.IdTuteur.Should().Be(2);
        }

        [Fact]
        public async Task AffecterTuteur_TuteurInexistant_Throws()
        {
            var act = () => _sut.AffecterTuteurAsync(10, 999);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Tuteur*");
        }

        [Fact]
        public async Task AffecterTuteur_TuteurInactif_Throws()
        {
            var act = () => _sut.AffecterTuteurAsync(10, 3);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Tuteur*");
        }

        [Fact]
        public async Task AffecterTuteur_EleveInexistant_Throws()
        {
            var act = () => _sut.AffecterTuteurAsync(999, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Élève*");
        }

        public void Dispose() => _context.Dispose();
    }
}
