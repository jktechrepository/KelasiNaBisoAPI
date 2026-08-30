using FluentAssertions;
using KelasiNaBiso.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    /// <summary>
    /// Smoke tests sécurité multi-tenant (filtre SchoolTenantActionFilter).
    /// </summary>
    public class SchoolTenantSmokeTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public SchoolTenantSmokeTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetClassesByEcole_ShouldReturnUnauthorized_WhenAnonymous()
        {
            var response = await _client.GetAsync("/api/Classe/ecole/1");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetEleveParEcoleAll_ShouldReturnUnauthorized_WhenAnonymous()
        {
            var response = await _client.GetAsync("/api/EleveParEcole");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
