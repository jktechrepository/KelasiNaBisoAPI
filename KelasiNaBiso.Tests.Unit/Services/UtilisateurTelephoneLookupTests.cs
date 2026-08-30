using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class UtilisateurTelephoneLookupTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly UtilisateurService _service;

        public UtilisateurTelephoneLookupTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _service = new UtilisateurService(_context);
            Seed();
        }

        private void Seed()
        {
            var user = TestDataBuilder.CreateUtilisateur(1, "user@test.com", "User", idEcole: 1);
            user.Telephone = "+243819932032";
            user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Secret123!");
            _context.Utilisateurs.Add(user);

            var userSpaced = TestDataBuilder.CreateUtilisateur(2, "spaced@test.com", "Spaced", idEcole: 1);
            userSpaced.Telephone = "+243 815 421 689";
            userSpaced.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Secret123!");
            _context.Utilisateurs.Add(userSpaced);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByTelephoneAsync_WithSpaces_FindsNormalizedStoredNumber()
        {
            var result = await _service.GetByTelephoneAsync("+243 819 932 032");

            result.Should().ContainSingle(u => u.IdUtilisateur == 1);
        }

        [Fact]
        public async Task GetByTelephoneAsync_NormalizedInput_FindsSpacedStoredNumber()
        {
            var result = await _service.GetByTelephoneAsync("+243815421689");

            result.Should().ContainSingle(u => u.IdUtilisateur == 2);
        }

        public void Dispose() => _context.Dispose();
    }
}
