using System.Text.Json;
using System.Text.Json.Serialization;

namespace UrlShortener.Converters;

public class RequestIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String when string.IsNullOrEmpty(reader.GetString()) => -1,
            JsonTokenType.String when int.TryParse(reader.GetString(), out int value) => value,
            JsonTokenType.Null => -1,
            _ => -1
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}