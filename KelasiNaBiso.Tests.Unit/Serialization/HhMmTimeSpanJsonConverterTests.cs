using System.Text.Json;
using FluentAssertions;
using KelasiNaBiso.Models.DTOs;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Serialization;

public sealed class HhMmTimeSpanJsonConverterTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Create_request_reads_HH_mm()
    {
        var request = JsonSerializer.Deserialize<CreateHoraireDeCoursRequest>(
            "{\"heureDebut\":\"08:30\",\"heureFin\":\"10:00\"}",
            JsonOptions);

        request!.HeureDebut.Should().Be(new TimeSpan(8, 30, 0));
        request.HeureFin.Should().Be(new TimeSpan(10, 0, 0));
    }

    [Fact]
    public void Response_writes_HH_mm()
    {
        var response = new HoraireDeCoursResponse(
            1,
            Models.Enums.JourSemaine.Lundi,
            new TimeSpan(8, 30, 0),
            new TimeSpan(10, 0, 0),
            1,
            "Mathématiques",
            1,
            "6e",
            1,
            "Jean Enseignant",
            1,
            "2026-2027",
            null,
            true);

        var json = JsonSerializer.Serialize(response, JsonOptions);

        json.Should().Contain("\"heureDebut\":\"08:30\"");
        json.Should().Contain("\"heureFin\":\"10:00\"");
    }

    [Fact]
    public void Create_request_rejects_seconds_format()
    {
        var action = () => JsonSerializer.Deserialize<CreateHoraireDeCoursRequest>(
            "{\"heureDebut\":\"08:30:00\",\"heureFin\":\"10:00\"}",
            JsonOptions);

        action.Should().Throw<JsonException>()
            .WithMessage("*format HH:mm*");
    }
}
