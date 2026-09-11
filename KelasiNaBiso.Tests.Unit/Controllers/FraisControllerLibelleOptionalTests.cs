using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class FraisControllerLibelleOptionalTests
    {
        private static FraisController CreateController(Mock<IFraisRepository> repo, int idEcole = 13)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, UserRoles.ADMIN),
                new Claim("IdEcole", idEcole.ToString())
            }, authenticationType: "Test");

            return new FraisController(repo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
                    }
                }
            };
        }

        [Fact]
        public async Task GetFraisByEcoleAndLibelle_WithoutLibelle_ReturnsOkList_NotBadRequest()
        {
            var repo = new Mock<IFraisRepository>();
            var listResult = new ElevesAnneeScopedResult<IEnumerable<FraisDto>>
            {
                IdEcole = 13,
                IdAnneeScolaire = 17,
                Data = new List<FraisDto>
                {
                    new() { IdFrais = 1, LibelleFrais = "Inscription", IdEcole = 13, IdAnneeScolaire = 17 }
                }
            };
            repo.Setup(r => r.GetByEcoleAsync(13, 17, 192))
                .ReturnsAsync(listResult);

            var controller = CreateController(repo);

            var action = await controller.GetFraisByEcoleAndLibelle(
                idEcole: 13,
                libelleFrais: null,
                idAnneeScolaire: 17,
                idClasse: 192);

            var ok = action.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(listResult);
            repo.Verify(r => r.GetByEcoleAsync(13, 17, 192), Times.Once);
            repo.Verify(r => r.GetByEcoleAndLibelleAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task GetFraisByEcoleAndLibelle_WithLibelle_UsesLibelleLookup()
        {
            var repo = new Mock<IFraisRepository>();
            var single = new ElevesAnneeScopedResult<FraisDto?>
            {
                IdEcole = 13,
                IdAnneeScolaire = 17,
                Data = new FraisDto { IdFrais = 2, LibelleFrais = "Minerval", IdEcole = 13 }
            };
            repo.Setup(r => r.GetByEcoleAndLibelleAsync(13, "Minerval", 17, 192))
                .ReturnsAsync(single);

            var controller = CreateController(repo);

            var action = await controller.GetFraisByEcoleAndLibelle(
                13, "Minerval", 17, 192);

            action.Should().BeOfType<OkObjectResult>();
            repo.Verify(r => r.GetByEcoleAndLibelleAsync(13, "Minerval", 17, 192), Times.Once);
        }
    }
}
