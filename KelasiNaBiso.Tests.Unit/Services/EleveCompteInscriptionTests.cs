using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    /// <summary>
    /// Compte Élève auto à l'inscription : username = matricule, IdEleve, rôle Eleve.
    /// </summary>
    public class EleveCompteInscriptionTests : IDisposable
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly InscriptionService _service;
        private const string Matricule = "MAT-ELEVE-42";

        public EleveCompteInscriptionTests()
        {
            var options = new DbContextOptionsBuilder<KelasiNaBisoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new KelasiNaBisoDbContext(options);
            Seed();
            _service = CreateService();
        }

        private InscriptionService CreateService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:KelasiConnection"] = "Server=localhost;Database=test;"
                })
                .Build();

            var resolver = new InscriptionActiveResolver(_context);
            var scope = new EleveAnneeScopeHelper(_context, resolver, new AnneeScolaireService(_context));

            return new InscriptionService(
                _context,
                config,
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<ISignalRNotificationService>(),
                Mock.Of<IUtilisateurRepository>(),
                NullLogger<InscriptionService>.Instance,
                resolver,
                scope,
                new EleveCompteService(_context, Mock.Of<ISmsNotificationService>(), NullLogger<EleveCompteService>.Instance));
        }

        private void Seed()
        {
            var ecole = TestDataBuilder.CreateEcole(1, "Ecole Test");
            var direction = TestDataBuilder.CreateDirection(1, 1);
            var classeA = TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1);
            var classeB = TestDataBuilder.CreateClasse(11, "5e B", idDirection: 1);
            var annee100 = TestDataBuilder.CreateAnneeScolaire(100, 1, "2024-2025");
            var annee101 = TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026");
            var tuteur = TestDataBuilder.CreateTuteur(1, "Parent Test", telephone: "+243900000099");
            var eleve = TestDataBuilder.CreateEleve(1, 1, "Eleve Existant");
            eleve.Matricule = Matricule;

            _context.Ecoles.Add(ecole);
            _context.Directions.Add(direction);
            _context.Classes.AddRange(classeA, classeB);
            _context.AnneeScolaires.AddRange(annee100, annee101);
            _context.Tuteurs.Add(tuteur);
            _context.Eleves.Add(eleve);
            _context.Inscriptions.Add(
                TestDataBuilder.CreateInscription(1, 1, 1, 10, 100));
            _context.SaveChanges();
        }

        private static CreateInscriptionDto BuildReinscriptionDto() =>
            new()
            {
                Type = "Réinscription",
                IdEcole = 1,
                IdClasse = 11,
                IdAnneeScolaire = 101,
                DateInscription = DateTime.UtcNow,
                StatutInscription = "Confirmé",
                IdEleveExistant = 1,
                NomEleve = "Eleve",
                PostnomEleve = "Existant",
                PrenomEleve = "Test",
                GenreEleve = "M",
                DateNaissanceEleve = new DateTime(2015, 1, 1),
                LieuNaissanceEleve = "Kinshasa",
                NationaliteEleve = "RDC",
                NomCompletTuteur = "Parent Test",
                GenreTuteur = "M",
                TelephoneTuteur = "+243900000099",
                MatriculeEleve = Matricule
            };

        [Fact]
        public void UserRoles_Eleve_EqualsAsciiEleve_NotAccented()
        {
            UserRoles.ELEVE.Should().Be("Eleve");
            UserRoles.ELEVE.Should().NotBe("Élève");
        }

        [Fact]
        public async Task CreateInscriptionAsync_CreatesEleveUser_WithMatriculeUsernameAndIdEleve()
        {
            var result = await _service.CreateInscriptionAsync(BuildReinscriptionDto());

            result.Success.Should().BeTrue();

            var user = await _context.Utilisateurs
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.IdEleve == 1);

            user.Should().NotBeNull();
            user!.DefaultUsername.Should().Be(Matricule);
            user.IdEleve.Should().Be(1);
            user.DoitChangerMotDePasse.Should().BeTrue();
            user.IdEcole.Should().Be(1);
            BCrypt.Net.BCrypt.Verify("123456", user.MotDePasseHash).Should().BeTrue();

            var roleName = user.UserRoles?.FirstOrDefault(ur => ur.Statut == true)?.Role?.Nom
                           ?? user.Role?.Nom;
            roleName.Should().Be(UserRoles.ELEVE);
        }

        [Fact]
        public async Task CreateInscriptionAsync_SecondReinscription_IsIdempotent_OneUserPerEleve()
        {
            var first = await _service.CreateInscriptionAsync(BuildReinscriptionDto());
            first.Success.Should().BeTrue();

            var userIdAfterFirst = await _context.Utilisateurs
                .Where(u => u.IdEleve == 1)
                .Select(u => u.IdUtilisateur)
                .SingleAsync();

            // Nouvelle année pour une 2e réinscription
            _context.AnneeScolaires.Add(TestDataBuilder.CreateAnneeScolaire(102, 1, "2026-2027"));
            await _context.SaveChangesAsync();

            var dto = BuildReinscriptionDto();
            dto.IdAnneeScolaire = 102;
            dto.IdClasse = 10;

            var second = await _service.CreateInscriptionAsync(dto);
            second.Success.Should().BeTrue();

            var users = await _context.Utilisateurs.Where(u => u.IdEleve == 1).ToListAsync();
            users.Should().HaveCount(1);
            users[0].IdUtilisateur.Should().Be(userIdAfterFirst);
            users[0].DefaultUsername.Should().Be(Matricule);
        }

        public void Dispose() => _context.Dispose();
    }
}
