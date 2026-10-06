#pragma warning disable CS0618
using FluentAssertions;
using KelasiNaBiso.Controllers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Controllers
{
    public class InscriptionControllerGetByEcoleSearchTests
    {
        [Fact]
        public async Task GetInscriptionsByEcole_PassesSearchTermToRepository()
        {
            var repo = new Mock<IInscriptionRepository>();
            repo.Setup(r => r.GetByEcoleAsync(7, 101, "Alpha"))
                .ReturnsAsync(new ElevesAnneeScopedResult<IEnumerable<Inscription>>
                {
                    Data = Array.Empty<Inscription>(),
                    IdEcole = 7,
                    IdAnneeScolaire = 101
                });
            var controller = new InscriptionController(repo.Object, Mock.Of<IAuditService>());

            var result = await controller.GetInscriptionsByEcole(7, 101, "Alpha");

            result.Should().BeOfType<OkObjectResult>();
            repo.Verify(r => r.GetByEcoleAsync(7, 101, "Alpha"), Times.Once);
        }

        [Fact]
        public async Task GetInscriptionsByEcole_WithoutSearchTerm_PassesNull()
        {
            var repo = new Mock<IInscriptionRepository>();
            repo.Setup(r => r.GetByEcoleAsync(7, null, null))
                .ReturnsAsync(new ElevesAnneeScopedResult<IEnumerable<Inscription>>
                {
                    Data = Array.Empty<Inscription>(),
                    IdEcole = 7,
                    IdAnneeScolaire = 101
                });
            var controller = new InscriptionController(repo.Object, Mock.Of<IAuditService>());

            var result = await controller.GetInscriptionsByEcole(7);

            result.Should().BeOfType<OkObjectResult>();
            repo.Verify(r => r.GetByEcoleAsync(7, null, null), Times.Once);
        }
    }
}
