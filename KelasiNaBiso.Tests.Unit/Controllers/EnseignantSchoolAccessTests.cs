using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers;

public class EnseignantSchoolAccessTests
{
    [Fact]
    public async Task ForbidIfWrongSchoolAsync_Enseignant_SameJwtEcole_AllowsWithoutAgentLookup()
    {
        var agents = new Mock<IAgentRepository>(MockBehavior.Strict);
        var controller = CreateEnseignantController(idEcoleJwt: 10, idAgent: 7);

        var deny = await controller.ForbidIfWrongSchoolAsync(10, agentRepository: agents.Object);

        deny.Should().BeNull();
        agents.Verify(a => a.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ForbidIfWrongSchoolAsync_Enseignant_AgentEcoleMatchesQuery_Allows()
    {
        var agents = new Mock<IAgentRepository>();
        agents.Setup(a => a.GetByIdAsync(7)).ReturnsAsync(new Agent { IdAgent = 7, IdEcole = 20, Statut = true });
        var controller = CreateEnseignantController(idEcoleJwt: 10, idAgent: 7);

        var deny = await controller.ForbidIfWrongSchoolAsync(20, agentRepository: agents.Object);

        deny.Should().BeNull();
        agents.Verify(a => a.GetByIdAsync(7), Times.Once);
    }

    [Fact]
    public async Task ForbidIfWrongSchoolAsync_Enseignant_AgentEcoleDoesNotMatchQuery_Forbids()
    {
        var agents = new Mock<IAgentRepository>();
        agents.Setup(a => a.GetByIdAsync(7)).ReturnsAsync(new Agent { IdAgent = 7, IdEcole = 10, Statut = true });
        var controller = CreateEnseignantController(idEcoleJwt: 10, idAgent: 7);

        var deny = await controller.ForbidIfWrongSchoolAsync(20, agentRepository: agents.Object);

        deny.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static ControllerBase CreateEnseignantController(int idEcoleJwt, int idAgent)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, UserRoles.ENSEIGNANT),
            new Claim("IdEcole", idEcoleJwt.ToString()),
            new Claim("IdAgent", idAgent.ToString()),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "Test");

        return new FraisController(Mock.Of<IFraisRepository>())
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
}
