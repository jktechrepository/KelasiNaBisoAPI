using FluentAssertions;
using KelasiNaBiso.Data;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class AgentServiceRoleMappingTests : IDisposable
    {
        private readonly AgentService _agentService;

        public AgentServiceRoleMappingTests()
        {
            _agentService = new AgentService(
                TestDbContextFactory.CreateInMemoryContext(),
                Mock.Of<IUsernameGeneratorService>(),
                Mock.Of<IEmailService>(),
                Mock.Of<IUtilisateurRepository>(),
                Mock.Of<ILogger<AgentService>>());
        }

        [Theory]
        [InlineData("caissier", "Caissier")]
        [InlineData("caissière", "Caissier")]
        [InlineData("Caissier", "Caissier")]
        [InlineData("comptable", "Financier")]
        [InlineData("financier", "Financier")]
        [InlineData("controlleur", "Controleur")]
        [InlineData("contrôleur", "Controleur")]
        [InlineData("controleur", "Controleur")]
        [InlineData("contrôleur des frais", "Controleur")]
        public void DetermineRoleFromFonction_ShouldMapFinanceFunctions(string fonction, string expectedRole)
        {
            var result = InvokeDetermineRoleFromFonction(fonction);
            result.Should().Be(expectedRole);
        }

        private string InvokeDetermineRoleFromFonction(string? fonction)
        {
            var method = typeof(AgentService).GetMethod(
                "DetermineRoleFromFonction",
                BindingFlags.Instance | BindingFlags.NonPublic);

            method.Should().NotBeNull();
            return (string)method!.Invoke(_agentService, new object?[] { fonction })!;
        }

        public void Dispose()
        {
        }
    }
}
