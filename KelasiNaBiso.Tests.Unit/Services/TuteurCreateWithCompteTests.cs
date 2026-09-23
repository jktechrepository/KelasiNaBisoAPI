using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class TuteurCreateWithCompteTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly TuteurService _sut;

        public TuteurCreateWithCompteTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Roles.Add(new Role { IdRole = 10, Nom = "Parent", Statut = true, DateCreation = DateTime.Now });
            _context.SaveChanges();

            var userRepoMock = new Mock<IUtilisateurRepository>();
            userRepoMock
                .Setup(r => r.AddRoleToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            var compte = new TuteurCompteService(
                _context,
                userRepoMock.Object,
                Mock.Of<IEmailService>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<ISignalRNotificationService>(),
                new InscriptionActiveResolver(_context),
                NullLogger<TuteurCompteService>.Instance);

            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));
            _sut = new TuteurService(_context, resolver, scope, compte);
        }

        [Fact]
        public async Task CreateWithCompte_CreeTuteurEtCompteParent()
        {
            var dto = new CreateTuteurDto
            {
                NomComplet = "Marie Dupont",
                Genre = "F",
                Telephone = "+243810000001",
                Email = "marie.dupont@test.cd"
            };

            var result = await _sut.CreateWithCompteAsync(dto, idEcole: 1);

            result.Tuteur.IdTuteur.Should().BeGreaterThan(0);
            result.Tuteur.NomComplet.Should().Be("Marie Dupont");
            result.CompteUtilisateur.Should().NotBeNull();
            result.CompteUtilisateur!.Role.Should().Be("Parent");
            result.CompteUtilisateur.DefaultUsername.Should().NotBeNullOrWhiteSpace();
            result.CompteUtilisateur.MotDePasseParDefaut.Should().Be("123456");
            result.CompteUtilisateur.IdTuteur.Should().Be(result.Tuteur.IdTuteur);

            var user = await _context.Utilisateurs.SingleAsync(u => u.IdTuteur == result.Tuteur.IdTuteur);
            user.IdEcole.Should().Be(1);
            user.DoitChangerMotDePasse.Should().BeTrue();
            user.Email.Should().Be("marie.dupont@test.cd");
        }

        [Fact]
        public async Task CreateWithCompte_ReutiliseUtilisateurExistantParTelephone()
        {
            _context.Utilisateurs.Add(new Utilisateur
            {
                IdUtilisateur = 50,
                NomUtilisateur = "Existait Deja",
                Email = "autre@test.cd",
                Telephone = "+243810000099",
                DefaultUsername = "ExistaitDeja1",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("old"),
                Statut = true,
                IdEcole = 1,
                IdRole = 10,
                DateCreation = DateTime.Now
            });
            _context.UserRoles.Add(new UserRole
            {
                IdUserRole = 1,
                IdUtilisateur = 50,
                IdRole = 10,
                IsPrimary = true,
                Statut = true,
                DateAttribution = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Already has Parent role — should link IdTuteur, empty password in response
            var dto = new CreateTuteurDto
            {
                NomComplet = "Nouveau Nom",
                Genre = "M",
                Telephone = "+243810000099"
            };

            var result = await _sut.CreateWithCompteAsync(dto, 1);

            result.CompteUtilisateur.Should().NotBeNull();
            result.CompteUtilisateur!.IdUtilisateur.Should().Be(50);
            result.CompteUtilisateur.MotDePasseParDefaut.Should().BeEmpty();

            var user = await _context.Utilisateurs.FindAsync(50);
            user!.IdTuteur.Should().Be(result.Tuteur.IdTuteur);
        }

        [Fact]
        public async Task CreateWithCompte_EcoleInconnue_Throws()
        {
            var dto = new CreateTuteurDto
            {
                NomComplet = "X",
                Genre = "M",
                Telephone = "+243810000002"
            };

            var act = () => _sut.CreateWithCompteAsync(dto, idEcole: 999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        public void Dispose() => _context.Dispose();
    }
}
