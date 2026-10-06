using FluentAssertions;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Tests.Unit.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services;

public sealed class HoraireDeCoursServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_an_invalid_time_range()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new HoraireDeCoursService(context);
        var request = new CreateHoraireDeCoursRequest
        {
            HeureDebut = new TimeSpan(10, 0, 0),
            HeureFin = new TimeSpan(9, 0, 0),
            IdCours = 1,
            IdClasse = 1,
            IdAgent = 1,
            IdAnneeScolaire = 1
        };

        var action = () => service.CreateAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*heure de début*");
    }

    [Fact]
    public async Task CreateAsync_rejects_unknown_references()
    {
        await using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new HoraireDeCoursService(context);
        var request = new CreateHoraireDeCoursRequest
        {
            HeureDebut = new TimeSpan(9, 0, 0),
            HeureFin = new TimeSpan(10, 0, 0),
            IdCours = 1,
            IdClasse = 1,
            IdAgent = 1,
            IdAnneeScolaire = 1
        };

        var action = () => service.CreateAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*introuvable*");
    }
}
