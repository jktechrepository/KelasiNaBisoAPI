using KelasiNaBiso.Data;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Tests.Unit.Helpers
{
    /// <summary>
    /// Factory pour créer un DbContext en mémoire pour les tests
    /// </summary>
    public static class TestDbContextFactory
    {
        public static KelasiNaBisoDbContext CreateInMemoryContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<KelasiNaBisoDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new KelasiNaBisoDbContext(options);
        }
    }
}

