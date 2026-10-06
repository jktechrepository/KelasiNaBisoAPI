using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class TuteurControllerSearchTests
    {
        private static TuteurController CreateController(
            Mock<ITuteurRepository> repository,
            int? idEcole = null,
            string role = UserRoles.ADMIN)
        {
            var claims = new List<Claim> { new(ClaimTypes.Role, role) };
            if (idEcole.HasValue)
                claims.Add(new Claim("IdEcole", idEcole.Value.ToString()));

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            };

            return new TuteurController(
                repository.Object,
                Mock.Of<ICurrentUserService>(),
                Mock.Of<IInscriptionActiveResolver>())
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }

        [Fact]
        public async Task SearchTuteurs_TermeTropCourt_RetourneBadRequest()
        {
            var repository = new Mock<ITuteurRepository>();
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.SearchTuteurs("J");

            result.Should().BeOfType<BadRequestObjectResult>();
            repository.Verify(
                r => r.SearchByNomCompletAsync(It.IsAny<string>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchTuteurs_UtilisateurScolaire_UtiliseSonEcole()
        {
            var repository = new Mock<ITuteurRepository>();
            repository
                .Setup(r => r.SearchByNomCompletAsync("Jean", 10))
                .ReturnsAsync(new[]
                {
                    new TuteurRechercheDto(1, "Jean Dupont", "M", null, null, true)
                });
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.SearchTuteurs("Jean");

            result.Should().BeOfType<OkObjectResult>();
            repository.Verify(r => r.SearchByNomCompletAsync("Jean", 10), Times.Once);
        }

        [Fact]
        public async Task SearchTuteurs_SuperAdmin_UtiliseUneRechercheGlobale()
        {
            var repository = new Mock<ITuteurRepository>();
            repository
                .Setup(r => r.SearchByNomCompletAsync("Jean", null))
                .ReturnsAsync(Array.Empty<TuteurRechercheDto>());
            var controller = CreateController(
                repository,
                role: UserRoles.SUPER_ADMIN);

            var result = await controller.SearchTuteurs("Jean");

            result.Should().BeOfType<OkObjectResult>();
            repository.Verify(r => r.SearchByNomCompletAsync("Jean", null), Times.Once);
        }

        [Fact]
        public async Task GetTuteursByEcolePaged_EcoleDifferente_RefuseSansAppelerLeRepository()
        {
            var repository = new Mock<ITuteurRepository>();
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.GetTuteursByEcolePaged(
                11,
                new PagedRequest { PageNumber = 2, PageSize = 20 });

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            repository.Verify(
                r => r.GetByEcolePagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<PagedRequest>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
