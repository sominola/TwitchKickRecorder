using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchRecorderApi.Converters;

public class NullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return null;

        if (DateTimeOffset.TryParseExact(s, Format, null, System.Globalization.DateTimeStyles.AssumeLocal, out var dto))
            return dto;

        if (DateTimeOffset.TryParse(s, out dto))
            return dto;

        throw new JsonException($"Invalid date format: {s}");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(Format));
        else
            writer.WriteNullValue();
    }
}
