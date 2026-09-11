using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class FraisParentMultiEcoleAccessTests
    {
        private static ControllerBase CreateControllerBase(
            string role,
            int? idEcoleJwt,
            int? idTuteur = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, role)
            };
            if (idEcoleJwt.HasValue)
                claims.Add(new Claim("IdEcole", idEcoleJwt.Value.ToString()));
            if (idTuteur.HasValue)
                claims.Add(new Claim("IdTuteur", idTuteur.Value.ToString()));

            var identity = new ClaimsIdentity(claims, authenticationType: "Test");
            var controller = new FraisController(Mock.Of<IFraisRepository>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
                    }
                }
            };
            return controller;
        }

        [Fact]
        public async Task ForbidIfWrongSchoolAsync_Parent_WithChildInTargetEcole_Allows()
        {
            var resolver = new Mock<IInscriptionActiveResolver>();
            resolver.Setup(r => r.IsTuteurInEcoleAsync(50, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = CreateControllerBase(UserRoles.PARENT, idEcoleJwt: 10, idTuteur: 50);

            var deny = await controller.ForbidIfWrongSchoolAsync(20, resolver.Object);

            deny.Should().BeNull();
        }

        [Fact]
        public async Task ForbidIfWrongSchoolAsync_Parent_WithoutChildInTargetEcole_Forbids()
        {
            var resolver = new Mock<IInscriptionActiveResolver>();
            resolver.Setup(r => r.IsTuteurInEcoleAsync(50, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = CreateControllerBase(UserRoles.PARENT, idEcoleJwt: 10, idTuteur: 50);

            var deny = await controller.ForbidIfWrongSchoolAsync(20, resolver.Object);

            deny.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task ForbidIfWrongSchoolAsync_Parent_SameJwtEcole_AllowsWithoutResolverCall()
        {
            var resolver = new Mock<IInscriptionActiveResolver>();
            var controller = CreateControllerBase(UserRoles.PARENT, idEcoleJwt: 10, idTuteur: 50);

            var deny = await controller.ForbidIfWrongSchoolAsync(10, resolver.Object);

            deny.Should().BeNull();
            resolver.Verify(r => r.IsTuteurInEcoleAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ForbidIfWrongSchoolAsync_Admin_WrongEcole_Forbids()
        {
            var controller = CreateControllerBase(UserRoles.ADMIN, idEcoleJwt: 10);

            var deny = await controller.ForbidIfWrongSchoolAsync(20, Mock.Of<IInscriptionActiveResolver>());

            deny.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task ForbidIfWrongSchoolAsync_SuperAdmin_AllowsAnyEcole()
        {
            var controller = CreateControllerBase(UserRoles.SUPER_ADMIN, idEcoleJwt: 10);

            var deny = await controller.ForbidIfWrongSchoolAsync(99, Mock.Of<IInscriptionActiveResolver>());

            deny.Should().BeNull();
        }

        [Fact]
        public async Task GetFraisByEcoleAndLibelle_ParentLinkedToOtherEcole_ReturnsOk()
        {
            var repo = new Mock<IFraisRepository>();
            var listResult = new ElevesAnneeScopedResult<IEnumerable<FraisDto>>
            {
                IdEcole = 20,
                IdAnneeScolaire = 1,
                Data = new List<FraisDto> { new() { IdFrais = 1, IdEcole = 20, LibelleFrais = "Minerval" } }
            };
            repo.Setup(r => r.GetByEcoleAsync(20, null, null)).ReturnsAsync(listResult);

            var resolver = new Mock<IInscriptionActiveResolver>();
            resolver.Setup(r => r.IsTuteurInEcoleAsync(50, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, UserRoles.PARENT),
                new Claim("IdEcole", "10"),
                new Claim("IdTuteur", "50")
            }, "Test");

            var services = new ServiceCollection();
            services.AddSingleton(resolver.Object);
            var sp = services.BuildServiceProvider();

            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(claims),
                RequestServices = sp
            };

            var controller = new FraisController(repo.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = http }
            };

            var action = await controller.GetFraisByEcoleAndLibelle(20, libelleFrais: null);

            action.Should().BeOfType<OkObjectResult>();
            repo.Verify(r => r.GetByEcoleAsync(20, null, null), Times.Once);
        }
    }
}
