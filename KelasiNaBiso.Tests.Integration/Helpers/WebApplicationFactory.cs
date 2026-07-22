using KelasiNaBiso.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KelasiNaBiso.Tests.Integration.Helpers
{
    /// <summary>
    /// Factory pour créer une application de test avec DbContext en mémoire
    /// </summary>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remplacer le DbContext par une version InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<KelasiNaBisoDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<KelasiNaBisoDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Créer un scope pour obtenir le DbContext et initialiser les données
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<KelasiNaBisoDbContext>();

                    db.Database.EnsureCreated();
                }
            });
        }
    }
}

