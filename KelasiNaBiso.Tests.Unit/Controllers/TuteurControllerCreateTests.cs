using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class TuteurControllerCreateTests
    {
        private static TuteurController CreateController(
            Mock<ITuteurRepository> repository,
            int? idEcole = null,
            string role = UserRoles.ADMIN)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, role)
            };

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

        private static CreateTuteurDto CreateDto() => new()
        {
            NomComplet = "Marie Dupont",
            Genre = "F",
            Telephone = "+243810000001"
        };

        [Fact]
        public async Task CreateTuteur_SansQuery_UtiliseIdEcoleDuToken()
        {
            var repository = new Mock<ITuteurRepository>();
            repository
                .Setup(r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), 10))
                .ReturnsAsync(new CreateTuteurResultDto
                {
                    Tuteur = new Tuteur { IdTuteur = 1, NomComplet = "Marie Dupont" }
                });
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.CreateTuteur(null, CreateDto());

            result.Should().BeOfType<CreatedAtActionResult>();
            repository.Verify(
                r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), 10),
                Times.Once);
        }

        [Fact]
        public async Task CreateTuteur_SansQueryEtSansClaimEcole_RetourneBadRequest()
        {
            var repository = new Mock<ITuteurRepository>();
            var controller = CreateController(repository);

            var result = await controller.CreateTuteur(null, CreateDto());

            result.Should().BeOfType<BadRequestObjectResult>();
            repository.Verify(
                r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateTuteur_QueryDuneAutreEcole_RetourneForbidden()
        {
            var repository = new Mock<ITuteurRepository>();
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.CreateTuteur(20, CreateDto());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            repository.Verify(
                r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateTuteur_QueryValide_PreserveLeComportementExistant()
        {
            var repository = new Mock<ITuteurRepository>();
            repository
                .Setup(r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), 10))
                .ReturnsAsync(new CreateTuteurResultDto
                {
                    Tuteur = new Tuteur { IdTuteur = 1, NomComplet = "Marie Dupont" }
                });
            var controller = CreateController(repository, idEcole: 10);

            var result = await controller.CreateTuteur(10, CreateDto());

            result.Should().BeOfType<CreatedAtActionResult>();
            repository.Verify(
                r => r.CreateWithCompteAsync(It.IsAny<CreateTuteurDto>(), 10),
                Times.Once);
        }
    }
}
