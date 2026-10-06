using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KelasiNaBiso.Tests.Integration.Helpers;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace KelasiNaBiso.Tests.Integration.Controllers
{
    public class PresenceSwaggerDefaultsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public PresenceSwaggerDefaultsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("type/Agent/date/{date}")]
        [InlineData("agent/1/date/{date}")]
        [InlineData("date/{date}")]
        [InlineData("date/{date}/paged")]
        public async Task PresenceDateRoutes_ParseIsoDateAndRejectInvalidDates(string route)
        {
            using var client = CreateAuthenticatedClient();

            using var invalidDateResponse = await client.GetAsync(
                $"/api/Presence/{route.Replace("{date}", "2026-13-40", StringComparison.Ordinal)}");
            Assert.Equal(HttpStatusCode.BadRequest, invalidDateResponse.StatusCode);

            using var validDateResponse = await client.GetAsync(
                $"/api/Presence/{route.Replace("{date}", "2026-09-26", StringComparison.Ordinal)}");
            Assert.Equal(HttpStatusCode.OK, validDateResponse.StatusCode);
        }

        [Fact]
        public async Task StudentAttendanceEndpoint_RejectsInvalidOptionalFilters()
        {
            using var client = CreateAuthenticatedClient();

            using var invalidStudentResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel?idEleve=0");
            Assert.Equal(HttpStatusCode.BadRequest, invalidStudentResponse.StatusCode);

            using var incompletePeriodResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel?dateDebut=2026-09-01");
            Assert.Equal(HttpStatusCode.BadRequest, incompletePeriodResponse.StatusCode);

            using var mixedDateResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel?date=2026-09-01&dateDebut=2026-09-01&dateFin=2026-09-02");
            Assert.Equal(HttpStatusCode.BadRequest, mixedDateResponse.StatusCode);
        }

        [Fact]
        public async Task StudentAttendanceExportEndpoint_RejectsInvalidOptionalFilters()
        {
            using var client = CreateAuthenticatedClient();

            using var invalidStudentResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel/export?idEleve=0");
            Assert.Equal(HttpStatusCode.BadRequest, invalidStudentResponse.StatusCode);

            using var incompletePeriodResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel/export?dateDebut=2026-09-01");
            Assert.Equal(HttpStatusCode.BadRequest, incompletePeriodResponse.StatusCode);

            using var mixedDateResponse = await client.GetAsync(
                "/api/Presence/eleves/classe/1/feuille-appel/export?date=2026-09-01&dateDebut=2026-09-01&dateFin=2026-09-02");
            Assert.Equal(HttpStatusCode.BadRequest, mixedDateResponse.StatusCode);
        }

        [Fact]
        public async Task TypeAndDateEndpoint_UsesDateOnlyAndSwaggerDefaults()
        {
            using var client = _factory.CreateClient();
            using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
            var typeParameterOnly = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/type/{typePersonne}")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "typePersonne");
            Assert.Equal(
                "Agent",
                typeParameterOnly.GetProperty("schema").GetProperty("default").GetString());
            Assert.True(typeParameterOnly.GetProperty("required").GetBoolean());

            var dateParameter = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/date/{date}")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "date");
            var dateParameterSchema = dateParameter.GetProperty("schema");
            Assert.Equal("string", dateParameterSchema.GetProperty("type").GetString());
            Assert.Equal("date", dateParameterSchema.GetProperty("format").GetString());
            Assert.Equal(
                DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                dateParameterSchema.GetProperty("default").GetString());
            Assert.True(dateParameter.GetProperty("required").GetBoolean());

            var typeDateOperation = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/type/{typePersonne}/date/{date}")
                .GetProperty("get");
            Assert.False(typeDateOperation.TryGetProperty("requestBody", out _));

            var parameters = typeDateOperation
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            Assert.False(parameters.ContainsKey("idEcole"));

            var typeParameter = parameters["typePersonne"];
            Assert.Equal("Agent", typeParameter.GetProperty("schema").GetProperty("default").GetString());
            Assert.True(typeParameter.GetProperty("required").GetBoolean());

            var dateSchema = parameters["date"].GetProperty("schema");
            Assert.Equal("string", dateSchema.GetProperty("type").GetString());
            Assert.Equal("date", dateSchema.GetProperty("format").GetString());
            Assert.Equal(
                DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                dateSchema.GetProperty("default").GetString());
            Assert.True(parameters["date"].GetProperty("required").GetBoolean());

            var pagedDateParameter = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/date/{date}/paged")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "date");
            var pagedDateSchema = pagedDateParameter.GetProperty("schema");
            Assert.Equal("string", pagedDateSchema.GetProperty("type").GetString());
            Assert.Equal("date", pagedDateSchema.GetProperty("format").GetString());
            Assert.Equal(
                DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                pagedDateSchema.GetProperty("default").GetString());
            Assert.True(pagedDateParameter.GetProperty("required").GetBoolean());

            var dateRangeParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/date-range/paged")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            foreach (var parameterName in new[] { "dateDebut", "dateFin" })
            {
                var parameter = dateRangeParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.Equal(today, schema.GetProperty("default").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
            }

            var agentDateParameter = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/agent/{idAgent}/date/{date}")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "date");
            var agentDateSchema = agentDateParameter.GetProperty("schema");
            Assert.Equal("string", agentDateSchema.GetProperty("type").GetString());
            Assert.Equal("date", agentDateSchema.GetProperty("format").GetString());
            Assert.Equal(today, agentDateSchema.GetProperty("default").GetString());
            Assert.True(agentDateParameter.GetProperty("required").GetBoolean());

            var feuilleAppelDateParameter = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/eleves/classe/{idClasse}/feuille-appel")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "date");
            var feuilleAppelDateSchema = feuilleAppelDateParameter.GetProperty("schema");
            Assert.Equal("string", feuilleAppelDateSchema.GetProperty("type").GetString());
            Assert.Equal("date", feuilleAppelDateSchema.GetProperty("format").GetString());
            Assert.Equal(today, feuilleAppelDateSchema.GetProperty("default").GetString());
            Assert.True(
                !feuilleAppelDateParameter.TryGetProperty("required", out var feuilleAppelDateRequired) ||
                !feuilleAppelDateRequired.GetBoolean());

            var feuilleAppelParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/eleves/classe/{idClasse}/feuille-appel")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            Assert.Contains("idEleve", feuilleAppelParameters.Keys);
            foreach (var parameterName in new[] { "dateDebut", "dateFin" })
            {
                var parameter = feuilleAppelParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
            }

            var feuilleAppelExportDateParameter = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/eleves/classe/{idClasse}/feuille-appel/export")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .Single(parameter => parameter.GetProperty("name").GetString() == "date");
            var feuilleAppelExportDateSchema = feuilleAppelExportDateParameter.GetProperty("schema");
            Assert.Equal("string", feuilleAppelExportDateSchema.GetProperty("type").GetString());
            Assert.Equal("date", feuilleAppelExportDateSchema.GetProperty("format").GetString());
            Assert.Equal(today, feuilleAppelExportDateSchema.GetProperty("default").GetString());
            Assert.True(
                !feuilleAppelExportDateParameter.TryGetProperty("required", out var feuilleAppelExportDateRequired) ||
                !feuilleAppelExportDateRequired.GetBoolean());

            var feuilleAppelExportParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/eleves/classe/{idClasse}/feuille-appel/export")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            Assert.Contains("idEleve", feuilleAppelExportParameters.Keys);
            foreach (var parameterName in new[] { "dateDebut", "dateFin" })
            {
                var parameter = feuilleAppelExportParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
            }

            foreach (var route in new[]
                     {
                         "/api/Presence/agents/ecole/{idEcole}/feuille-appel",
                         "/api/Presence/agents/ecole/{idEcole}/feuille-appel/export"
                     })
            {
                var agentsFeuilleAppelDateParameter = document.RootElement
                    .GetProperty("paths")
                    .GetProperty(route)
                    .GetProperty("get")
                    .GetProperty("parameters")
                    .EnumerateArray()
                    .Single(parameter => parameter.GetProperty("name").GetString() == "date");
                var agentsFeuilleAppelDateSchema = agentsFeuilleAppelDateParameter.GetProperty("schema");
                Assert.Equal("string", agentsFeuilleAppelDateSchema.GetProperty("type").GetString());
                Assert.Equal("date", agentsFeuilleAppelDateSchema.GetProperty("format").GetString());
                Assert.Equal(today, agentsFeuilleAppelDateSchema.GetProperty("default").GetString());
                Assert.True(
                    !agentsFeuilleAppelDateParameter.TryGetProperty("required", out var agentsDateRequired) ||
                    !agentsDateRequired.GetBoolean(),
                    $"{route} date should be optional.");
            }

            var agentsFeuilleAppelParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/agents/ecole/{idEcole}/feuille-appel")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            foreach (var parameterName in new[] { "dateDebut", "dateFin" })
            {
                var parameter = agentsFeuilleAppelParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
            }
            Assert.Contains("idAgent", agentsFeuilleAppelParameters.Keys);

            var agentsFeuilleAppelExportParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/agents/ecole/{idEcole}/feuille-appel/export")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            foreach (var parameterName in new[] { "dateDebut", "dateFin" })
            {
                var parameter = agentsFeuilleAppelExportParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
            }
            Assert.Contains("idAgent", agentsFeuilleAppelExportParameters.Keys);

            var fonctionDateParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/agents/fonction/{fonction}")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            foreach (var parameterName in new[] { "date", "dateDebut", "dateFin" })
            {
                var parameter = fonctionDateParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
                Assert.False(schema.TryGetProperty("default", out _));
            }

            var dashboardDateParameters = document.RootElement
                .GetProperty("paths")
                .GetProperty("/api/Presence/dashboard/ecole/{idEcole}")
                .GetProperty("get")
                .GetProperty("parameters")
                .EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            foreach (var parameterName in new[] { "date", "dateDebut", "dateFin" })
            {
                var parameter = dashboardDateParameters[parameterName];
                var schema = parameter.GetProperty("schema");
                Assert.Equal("string", schema.GetProperty("type").GetString());
                Assert.Equal("date", schema.GetProperty("format").GetString());
                Assert.True(
                    !parameter.TryGetProperty("required", out var required) || !required.GetBoolean(),
                    $"{parameterName} should be optional.");
                Assert.False(schema.TryGetProperty("default", out _));
            }
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CreateTestToken());
            return client;
        }

        [Fact]
        public async Task TypeAndDateEndpoint_RequiresSchoolClaimFromToken()
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CreateTestToken(includeSchoolClaim: false));

            using var response = await client.GetAsync("/api/Presence/type/Agent/date/2026-09-26");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static string CreateTestToken(bool includeSchoolClaim = true)
        {
            const string secret = "KelasiNaBiso-Test-SecretKey-Min32Chars-ForHS256!!";
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256);
            var claims = includeSchoolClaim
                ? new[] { new System.Security.Claims.Claim("IdEcole", "1") }
                : Array.Empty<System.Security.Claims.Claim>();
            var token = new JwtSecurityToken(
                issuer: "KelasiNaBiso",
                audience: "KelasiNaBisoUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
