using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.DevoirADomicile;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Hubs;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class DevoirADomicileControllerDateFilterTests : IDisposable
    {
        private readonly Data.KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly DateTime _jour = new(2026, 3, 10);

        public DevoirADomicileControllerDateFilterTests()
        {
            _context = TestDbContextFactory.CreateInMemoryContext();
            _context.Ecoles.Add(TestDataBuilder.CreateEcole(1, "Ecole Test"));
            _context.Directions.Add(TestDataBuilder.CreateDirection(1, 1));
            _context.Classes.Add(TestDataBuilder.CreateClasse(10, "6e A", idDirection: 1));
            _context.AnneeScolaires.Add(
                TestDataBuilder.CreateAnneeScolaire(101, 1, "2025-2026",
                    _jour.AddMonths(-3), _jour.AddMonths(6)));
            _context.SaveChanges();

            var inscriptionResolver = new InscriptionActiveResolver(_context);
            var anneeRepo = new AnneeScolaireService(_context);
            _scope = new EleveAnneeScopeHelper(_context, inscriptionResolver, anneeRepo);
        }

        private DevoirADomicileController CreateController(
            Mock<IDevoirADomicileRepository> repo,
            Mock<ICurrentUserService> currentUser,
            Mock<IInscriptionActiveResolver>? inscriptionResolver = null)
        {
            return new DevoirADomicileController(
                repo.Object,
                Mock.Of<IFileStorageService>(),
                Mock.Of<IAntivirusService>(),
                currentUser.Object,
                Mock.Of<IHubContext<DevoirADomicileHub>>(),
                Mock.Of<IFirebaseNotificationService>(),
                Mock.Of<IEleveRepository>(),
                _context,
                NullLogger<DevoirADomicileController>.Instance,
                Mock.Of<ISmsNotificationService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<IServiceScopeFactory>(),
                inscriptionResolver?.Object ?? Mock.Of<IInscriptionActiveResolver>(),
                _scope);
        }

        private static PagedResult<DevoirADomicile> EmptyPaged() =>
            new(new List<DevoirADomicile>(), 0, 1, 15);

        [Fact]
        public async Task MesDevoirs_TransmetDateDebutEtDateFinAuRepo()
        {
            var repo = new Mock<IDevoirADomicileRepository>();
            repo.Setup(r => r.GetAllPagedAsync(
                    It.IsAny<PagedRequest>(), 101, null, null, _jour, _jour))
                .ReturnsAsync(EmptyPaged());
            repo.Setup(r => r.GetDevoirIdsTelechargesParUtilisateurAsync(
                    It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new HashSet<int>());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(c => c.UserRole).Returns(UserRoles.SUPER_ADMIN);
            currentUser.Setup(c => c.UserId).Returns(1);

            var controller = CreateController(repo, currentUser);

            var result = await controller.MesDevoirs(
                null, null, null, 101, _jour, _jour);

            result.Result.Should().BeOfType<OkObjectResult>();
            repo.Verify(
                r => r.GetAllPagedAsync(It.IsAny<PagedRequest>(), 101, null, null, _jour, _jour),
                Times.Once);
        }

        [Fact]
        public async Task GetMesDevoirsEleve_TransmetDateDebutEtDateFinAuRepo()
        {
            var repo = new Mock<IDevoirADomicileRepository>();
            repo.Setup(r => r.GetByClassePagedAsync(10, It.IsAny<PagedRequest>(), 101, _jour, _jour))
                .ReturnsAsync(EmptyPaged());
            repo.Setup(r => r.GetDevoirIdsTelechargesParUtilisateurAsync(
                    It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new HashSet<int>());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(c => c.EleveId).Returns(1);
            currentUser.Setup(c => c.UserId).Returns(5);

            var inscriptionResolver = new Mock<IInscriptionActiveResolver>();
            inscriptionResolver
                .Setup(r => r.GetInscriptionActiveAsync(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Inscription
                {
                    IdEleve = 1,
                    IdClasse = 10,
                    IdAnneeScolaire = 101,
                    IdEcole = 1
                });

            var controller = CreateController(repo, currentUser, inscriptionResolver);

            var result = await controller.GetMesDevoirsEleve(null, null, _jour, _jour);

            result.Should().BeOfType<OkObjectResult>();
            repo.Verify(
                r => r.GetByClassePagedAsync(10, It.IsAny<PagedRequest>(), 101, _jour, _jour),
                Times.Once);
        }

        [Fact]
        public async Task GetDevoirsByClasse_TransmetDateDebutEtDateFinAuRepo()
        {
            var repo = new Mock<IDevoirADomicileRepository>();
            repo.Setup(r => r.UserPeutAccederAClasseAsync(1, 10)).ReturnsAsync(true);
            repo.Setup(r => r.GetByClasseAsync(10, 101, _jour, _jour))
                .ReturnsAsync(Array.Empty<DevoirADomicile>());
            repo.Setup(r => r.GetDevoirIdsTelechargesParUtilisateurAsync(
                    It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new HashSet<int>());

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(c => c.UserId).Returns(1);

            var controller = CreateController(repo, currentUser);

            var result = await controller.GetDevoirsByClasse(10, null, 101, _jour, _jour);

            result.Result.Should().BeOfType<OkObjectResult>();
            repo.Verify(r => r.GetByClasseAsync(10, 101, _jour, _jour), Times.Once);
        }

        [Fact]
        public async Task GetDevoirsByTuteur_TransmetDateDebutEtDateFinAuRepo()
        {
            var repo = new Mock<IDevoirADomicileRepository>();
            repo.Setup(r => r.GetByTuteurPagedAsync(3, It.IsAny<PagedRequest>(), null, _jour, _jour))
                .ReturnsAsync(new PagedResult<DevoirADomicilePourTuteurDto>(
                    new List<DevoirADomicilePourTuteurDto>(), 0, 1, 15));

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(c => c.UserRole).Returns(UserRoles.ADMIN);

            var controller = CreateController(repo, currentUser);

            var result = await controller.GetDevoirsByTuteur(3, null, null, _jour, _jour);

            result.Should().BeOfType<OkObjectResult>();
            repo.Verify(
                r => r.GetByTuteurPagedAsync(3, It.IsAny<PagedRequest>(), null, _jour, _jour),
                Times.Once);
        }

        [Fact]
        public async Task GetDevoirsByClasse_DateDebutPosterieure_RetourneBadRequest()
        {
            var repo = new Mock<IDevoirADomicileRepository>();
            repo.Setup(r => r.UserPeutAccederAClasseAsync(1, 10)).ReturnsAsync(true);
            repo.Setup(r => r.GetByClasseAsync(10, 101, _jour.AddDays(2), _jour))
                .ThrowsAsync(new InvalidOperationException(
                    "dateDebut ne peut pas être postérieure à dateFin."));

            var currentUser = new Mock<ICurrentUserService>();
            currentUser.Setup(c => c.UserId).Returns(1);

            var controller = CreateController(repo, currentUser);

            var result = await controller.GetDevoirsByClasse(10, null, 101, _jour.AddDays(2), _jour);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        public void Dispose() => _context.Dispose();
    }
}
