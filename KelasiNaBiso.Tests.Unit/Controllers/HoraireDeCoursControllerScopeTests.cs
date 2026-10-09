using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers;

public sealed class HoraireDeCoursControllerScopeTests
{
    [Fact]
    public async Task Get_AdminWithTutorClaim_DoesNotApplyTutorScope()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        repository.Setup(r => r.GetAsync(
                null, null, 22, 13, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KelasiNaBiso.Models.DTOs.HoraireDeCoursResponse>());
        var controller = CreateController(
            repository.Object, new[] { UserRoles.ADMIN, UserRoles.ENSEIGNANT, UserRoles.PARENT },
            idEcole: 13, idTuteur: 50);

        var result = await controller.Get(null, null, 22, 13, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repository.Verify(r => r.GetAsync(
            null, null, 22, 13, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_ParentWithTutorClaim_AppliesTutorScope()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        repository.Setup(r => r.GetAsync(
                null, null, 22, null, 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KelasiNaBiso.Models.DTOs.HoraireDeCoursResponse>());
        var controller = CreateController(repository.Object, new[] { UserRoles.PARENT }, idEcole: 13, idTuteur: 50);

        var result = await controller.Get(null, null, 22, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repository.Verify(r => r.GetAsync(
            null, null, 22, null, 50, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_ParentAndTeacherWithoutAdministrativeRole_AppliesTutorScope()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        repository.Setup(r => r.GetAsync(
                null, null, 22, null, 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KelasiNaBiso.Models.DTOs.HoraireDeCoursResponse>());
        var controller = CreateController(
            repository.Object, new[] { UserRoles.PARENT, UserRoles.ENSEIGNANT }, idEcole: 13, idTuteur: 50);

        var result = await controller.Get(null, null, 22, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repository.Verify(r => r.GetAsync(
            null, null, 22, null, 50, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_ParentWithoutTutorClaim_ReturnsForbidden()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        var controller = CreateController(repository.Object, new[] { UserRoles.PARENT }, idEcole: 13, idTuteur: null);

        var result = await controller.Get(null, null, 22, null, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        repository.Verify(r => r.GetAsync(
            It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
            It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_Enseignant_IdEcoleMatchesAgentSchool_Allows()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        repository.Setup(r => r.GetAsync(
                null, null, 22, 20, null, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KelasiNaBiso.Models.DTOs.HoraireDeCoursResponse>());

        var agents = new Mock<IAgentRepository>();
        agents.Setup(a => a.GetByIdAsync(7))
            .ReturnsAsync(new Agent { IdAgent = 7, IdEcole = 20, Statut = true });

        var controller = CreateController(
            repository.Object, new[] { UserRoles.ENSEIGNANT },
            idEcole: 10, idTuteur: null, idAgent: 7, agentRepository: agents.Object);

        var result = await controller.Get(null, null, 22, 20, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repository.Verify(r => r.GetAsync(
            null, null, 22, 20, null, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_EnseignantWithoutAgentClaim_ReturnsForbidden()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        var controller = CreateController(
            repository.Object, new[] { UserRoles.ENSEIGNANT }, idEcole: 10, idTuteur: null, idAgent: null);

        var result = await controller.Get(null, null, 22, null, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        repository.Verify(r => r.GetAsync(
            It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
            It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_EnseignantOnly_PassesTeacherAgentScope()
    {
        var repository = new Mock<IHoraireDeCoursRepository>();
        repository.Setup(r => r.GetAsync(
                null, null, 22, 10, null, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KelasiNaBiso.Models.DTOs.HoraireDeCoursResponse>());
        var controller = CreateController(
            repository.Object, new[] { UserRoles.ENSEIGNANT },
            idEcole: 10, idTuteur: null, idAgent: 7);

        var result = await controller.Get(null, null, 22, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repository.Verify(r => r.GetAsync(
            null, null, 22, 10, null, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static HoraireDeCoursController CreateController(
        IHoraireDeCoursRepository repository,
        string[] roles,
        int idEcole,
        int? idTuteur,
        int? idAgent = null,
        IAgentRepository? agentRepository = null)
    {
        var claims = new List<Claim>
        {
            new Claim("IdEcole", idEcole.ToString()),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (idTuteur.HasValue)
            claims.Add(new Claim("IdTuteur", idTuteur.Value.ToString()));
        if (idAgent.HasValue)
            claims.Add(new Claim("IdAgent", idAgent.Value.ToString()));

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        if (agentRepository != null)
        {
            var services = new ServiceCollection();
            services.AddSingleton(agentRepository);
            http.RequestServices = services.BuildServiceProvider();
        }

        return new HoraireDeCoursController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = http }
        };
    }
}
