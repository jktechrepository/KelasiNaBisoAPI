using FluentAssertions;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class UtilisateurEmailLookupTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly UtilisateurService _service;

        public UtilisateurEmailLookupTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _service = new UtilisateurService(_context);

            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole"));
            _context.Utilisateurs.AddRange(
                TestDataBuilder.CreateUtilisateur(1, "actif@test.com", idEcole: 1, statut: true),
                TestDataBuilder.CreateUtilisateur(2, "inactif@test.com", idEcole: 1, statut: false),
                TestDataBuilder.CreateUtilisateur(3, "  MixedCase@Test.COM  ".Trim(), idEcole: 1, statut: true));
            // Fix MixedCase email stored as mixed
            var u3 = _context.Utilisateurs.Local.First(u => u.IdUtilisateur == 3);
            u3.Email = "MixedCase@Test.COM";
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEmailAsync_ActiveOnly_IgnoresInactive()
        {
            var actif = await _service.GetByEmailAsync("actif@test.com");
            var inactif = await _service.GetByEmailAsync("inactif@test.com");

            actif.Should().NotBeNull();
            actif!.IdUtilisateur.Should().Be(1);
            inactif.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAnyStatusAsync_FindsInactive()
        {
            var inactif = await _service.GetByEmailAnyStatusAsync("inactif@test.com");

            inactif.Should().NotBeNull();
            inactif!.IdUtilisateur.Should().Be(2);
            inactif.Statut.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmailAnyStatusAsync_IsCaseInsensitiveAndTrimmed()
        {
            var found = await _service.GetByEmailAnyStatusAsync("  mixedcase@test.com  ");

            found.Should().NotBeNull();
            found!.IdUtilisateur.Should().Be(3);
        }

        [Fact]
        public async Task GetByEmailAsync_Unknown_ReturnsNull()
        {
            var missing = await _service.GetByEmailAsync("ghost@test.com");
            missing.Should().BeNull();
        }

        public void Dispose() => _context.Dispose();
    }
}
