using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
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

    [Fact]
    public void Model_maps_horaire_navigations_to_existing_fk_columns()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var entity = context.Model.FindEntityType(typeof(HoraireDeCours));
        entity.Should().NotBeNull();

        ForeignKeyName(entity!, nameof(HoraireDeCours.Classe)).Should().Be(nameof(HoraireDeCours.IdClasse));
        ForeignKeyName(entity!, nameof(HoraireDeCours.Cours)).Should().Be(nameof(HoraireDeCours.IdCours));
        ForeignKeyName(entity!, nameof(HoraireDeCours.Agent)).Should().Be(nameof(HoraireDeCours.IdAgent));
        ForeignKeyName(entity!, nameof(HoraireDeCours.AnneeScolaire)).Should().Be(nameof(HoraireDeCours.IdAnneeScolaire));

        entity!.FindProperty("ClasseIdClasse").Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_teacher_sees_full_class_when_idAgent_on_a_slot()
    {
        await using var context = await SeedScheduleWorldAsync();
        var service = new HoraireDeCoursService(context);

        var rows = await service.GetAsync(null, null, 1, null, null, 7, CancellationToken.None);

        rows.Select(r => r.IdHoraireDeCours).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task GetAsync_teacher_sees_full_class_when_titulaire()
    {
        await using var context = await SeedScheduleWorldAsync();
        var service = new HoraireDeCoursService(context);

        var rows = await service.GetAsync(null, null, 1, null, null, 7, CancellationToken.None);

        rows.Should().Contain(r => r.IdHoraireDeCours == 3 && r.IdClasse == 2);
        rows.Should().NotContain(r => r.IdClasse == 3);
    }

    [Fact]
    public async Task GetByIdAsync_teacher_cannot_read_unassigned_class()
    {
        await using var context = await SeedScheduleWorldAsync();
        var service = new HoraireDeCoursService(context);

        var row = await service.GetByIdAsync(4, CancellationToken.None, idAgentAffecte: 7);

        row.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_parent_sees_only_children_classes()
    {
        await using var context = await SeedScheduleWorldAsync();
        var service = new HoraireDeCoursService(context);

        var rows = await service.GetAsync(null, null, 1, null, 50, null, CancellationToken.None);

        rows.Select(r => r.IdClasse).Distinct().Should().BeEquivalentTo(new[] { 1 });
        rows.Should().NotContain(r => r.IdClasse == 2 || r.IdClasse == 3);
    }

    private static async Task<KelasiNaBiso.Data.KelasiNaBisoDbContext> SeedScheduleWorldAsync()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var ecole = TestDataBuilder.CreateEcole(1, "Ecole");
        var direction = TestDataBuilder.CreateDirection(1, 1);
        var c1 = TestDataBuilder.CreateClasse(1, "6A", idDirection: 1);
        var c2 = TestDataBuilder.CreateClasse(2, "6B", idDirection: 1);
        var c3 = TestDataBuilder.CreateClasse(3, "5A", idDirection: 1);
        var annee = TestDataBuilder.CreateAnneeScolaire(1, 1, "2026-2027");
        var teacher = TestDataBuilder.CreateAgent(7, "Prof", idEcole: 1);
        var other = TestDataBuilder.CreateAgent(8, "Autre", idEcole: 1);
        var tuteur = TestDataBuilder.CreateTuteur(50, "Parent");
        var eleve = TestDataBuilder.CreateEleve(1, 50);
        var inscription = TestDataBuilder.CreateInscription(1, 1, 1, 1, 1);

        context.Ecoles.Add(ecole);
        context.Directions.Add(direction);
        context.Classes.AddRange(c1, c2, c3);
        context.AnneeScolaires.Add(annee);
        context.Agents.AddRange(teacher, other);
        context.Tuteurs.Add(tuteur);
        context.Eleves.Add(eleve);
        context.Inscriptions.Add(inscription);
        context.Cours.AddRange(
            new Cours { IdCours = 1, NomCours = "Math", IdClasse = 1 },
            new Cours { IdCours = 2, NomCours = "Fr", IdClasse = 1 },
            new Cours { IdCours = 3, NomCours = "Sci", IdClasse = 2 },
            new Cours { IdCours = 4, NomCours = "Hist", IdClasse = 3 });
        context.HorairesDeCours.AddRange(
            Slot(1, 1, 1, 7),
            Slot(2, 1, 2, 8),
            Slot(3, 2, 3, 8),
            Slot(4, 3, 4, 8));
        context.TitulairesClasses.Add(new TitulaireClasse
        {
            IdTitulaireClasse = 1,
            IdAgent = 7,
            IdClasse = 2,
            IdAnneeScolaire = 1,
            Statut = true,
            DateDebut = DateTime.UtcNow.AddMonths(-1)
        });
        await context.SaveChangesAsync();
        return context;
    }

    private static HoraireDeCours Slot(int id, int idClasse, int idCours, int idAgent) => new()
    {
        IdHoraireDeCours = id,
        Jour = JourSemaine.Lundi,
        HeureDebut = new TimeSpan(8 + id, 0, 0),
        HeureFin = new TimeSpan(9 + id, 0, 0),
        IdCours = idCours,
        IdClasse = idClasse,
        IdAgent = idAgent,
        IdAnneeScolaire = 1,
        Statut = true
    };

    private static string ForeignKeyName(Microsoft.EntityFrameworkCore.Metadata.IEntityType entity, string navigation) =>
        entity.FindNavigation(navigation)!.ForeignKey.Properties.Single().Name;
}
