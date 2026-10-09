using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers;

public sealed class UtilisateurControllerBasculeRoleTests : IDisposable
{
    private readonly KelasiNaBiso.Data.KelasiNaBisoDbContext _context;
    private readonly SimpleJwtService _jwtService;
    private readonly UtilisateurController _controller;

    public UtilisateurControllerBasculeRoleTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestSecretKeyForJWTTokenGeneration123456789" },
                { "Jwt:Issuer", "KelasiNaBiso" },
                { "Jwt:Audience", "KelasiNaBisoUsers" },
                { "Jwt:ExpirationMinutes", "1440" }
            })
            .Build();
        _jwtService = new SimpleJwtService(configuration);
        var permissionService = new PermissionService(_context, Mock.Of<ILogger<PermissionService>>());
        var refreshTokens = new Mock<IRefreshTokenService>();
        refreshTokens
            .Setup(s => s.GenerateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync("refresh-token-bascule");

        _controller = new UtilisateurController(
            Mock.Of<IUtilisateurRepository>(),
            Mock.Of<IV_UtilisateurRepository>(),
            Mock.Of<IUserDeviceRepository>(),
            _jwtService,
            permissionService,
            refreshTokens.Object,
            configuration,
            Mock.Of<ILogger<UtilisateurController>>(),
            _context,
            Mock.Of<IAuditService>(),
            Mock.Of<IEmailService>());
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task BasculeRole_Returns403_WhenTargetIsNotCurrentUser()
    {
        SetUser(userId: 2, roles: new[] { UserRoles.ENSEIGNANT });

        var result = await _controller.BasculeRole(1, 10);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task BasculeRole_Returns404_WhenRoleDoesNotExist()
    {
        await SeedDualRoleUserAsync();
        SetUser(userId: 1, roles: new[] { UserRoles.ENSEIGNANT, UserRoles.PARENT });

        var result = await _controller.BasculeRole(1, 99);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task BasculeRole_Returns400_WhenRoleNotAssignedToUser()
    {
        await SeedDualRoleUserAsync();
        var extra = TestDataBuilder.CreateRole(3, "Admin", niveau: 1);
        _context.Roles.Add(extra);
        await _context.SaveChangesAsync();
        SetUser(userId: 1, roles: new[] { UserRoles.ENSEIGNANT, UserRoles.PARENT });

        var result = await _controller.BasculeRole(1, 3);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _context.UserRoles.Should().Contain(ur => ur.IdRole == 1 && ur.IsPrimary);
        _context.UserRoles.Should().Contain(ur => ur.IdRole == 2 && !ur.IsPrimary);
    }

    [Fact]
    public async Task BasculeRole_ReturnsTokenWithChosenRolePermissions_WhenSelf()
    {
        await SeedDualRoleUserAsync();
        SetUser(userId: 1, roles: new[] { UserRoles.ENSEIGNANT, UserRoles.PARENT });

        var result = await _controller.BasculeRole(1, 2);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<AuthentificationResponse>().Subject;
        body.Success.Should().BeTrue();
        body.NomRole.Should().Be("Parent");
        body.Permissions.Should().BeEquivalentTo("Eleve.ReadChildren", "Paiement.Read");
        body.Roles.Should().HaveCount(2);
        body.PrimaryRole!.Nom.Should().Be("Enseignant");
        body.RefreshToken.Should().Be("refresh-token-bascule");
        body.AccessToken.Should().NotBeNullOrEmpty();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body.AccessToken);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .Should().Equal("Parent");
        jwt.Claims.First(c => c.Type == "primaryRole").Value.Should().Be("Parent");
        jwt.Claims.First(c => c.Type == "idRole").Value.Should().Be("2");
        jwt.Claims.First(c => c.Type == "roles").Value.Should().Contain("Enseignant");

        _context.UserRoles.Single(ur => ur.IdRole == 1).IsPrimary.Should().BeTrue();
        _context.UserRoles.Single(ur => ur.IdRole == 2).IsPrimary.Should().BeFalse();
    }

    private async Task SeedDualRoleUserAsync()
    {
        var ecole = TestDataBuilder.CreateEcole(1, "Test Ecole");
        var utilisateur = TestDataBuilder.CreateUtilisateur(1, "test@test.com", idEcole: 1);
        var enseignant = TestDataBuilder.CreateRole(1, "Enseignant", niveau: 4);
        var parent = TestDataBuilder.CreateRole(2, "Parent", niveau: 5);
        var p1 = TestDataBuilder.CreatePermission(1, "Note.Create", "Note");
        var p2 = TestDataBuilder.CreatePermission(2, "Note.Read", "Note");
        var p3 = TestDataBuilder.CreatePermission(3, "Eleve.ReadChildren", "Eleve");
        var p4 = TestDataBuilder.CreatePermission(4, "Paiement.Read", "Paiement");

        _context.Ecoles.Add(ecole);
        _context.Utilisateurs.Add(utilisateur);
        _context.Roles.AddRange(enseignant, parent);
        _context.Permissions.AddRange(p1, p2, p3, p4);
        _context.RolePermissions.AddRange(
            TestDataBuilder.CreateRolePermission(1, 1),
            TestDataBuilder.CreateRolePermission(1, 2),
            TestDataBuilder.CreateRolePermission(2, 3),
            TestDataBuilder.CreateRolePermission(2, 4));
        _context.UserRoles.AddRange(
            TestDataBuilder.CreateUserRole(1, 1, isPrimary: true),
            TestDataBuilder.CreateUserRole(1, 2, isPrimary: false));
        await _context.SaveChangesAsync();
    }

    private void SetUser(int userId, IEnumerable<string> roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }
}
