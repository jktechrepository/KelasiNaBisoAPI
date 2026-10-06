using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KelasiNaBiso.Serialization;

public sealed class HhMmTimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    private const string Format = @"hh\:mm";

    public override TimeSpan Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("L'heure doit être une chaîne au format HH:mm.");

        var value = reader.GetString();
        if (value is null || !TimeSpan.TryParseExact(
                value,
                Format,
                CultureInfo.InvariantCulture,
                out var time))
        {
            throw new JsonException("L'heure doit respecter le format HH:mm, par exemple 08:30.");
        }

        return time;
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeSpan value,
        JsonSerializerOptions options)
    {
        if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1) || value.Seconds != 0)
            throw new JsonException("L'heure doit être comprise entre 00:00 et 23:59, sans secondes.");

        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
