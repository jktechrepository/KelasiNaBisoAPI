using KelasiNaBiso.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KelasiNaBiso.Tests.Integration.Helpers
{
    /// <summary>
    /// Factory pour créer une application de test avec DbContext en mémoire.
    /// Utilise l'environnement Testing pour éviter UseUrls / RateLimit / seed SQL.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = "TestDb_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "KelasiNaBiso-Test-SecretKey-Min32Chars-ForHS256!!",
                    ["Jwt:Issuer"] = "KelasiNaBiso",
                    ["Jwt:Audience"] = "KelasiNaBisoUsers",
                    ["Jwt:ExpirationMinutes"] = "60",
                    ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<KelasiNaBisoDbContext>) ||
                        d.ServiceType == typeof(KelasiNaBisoDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<KelasiNaBisoDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.ConfigureWarnings(w =>
                        w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        }
    }
}
