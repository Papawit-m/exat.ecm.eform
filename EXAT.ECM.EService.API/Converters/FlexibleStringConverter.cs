using System.Text.Json.Serialization;
using System.Text.Json;

namespace EXAT.ECM.EService.API.Converters
{
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    // Handle both integer and decimal numbers
                    if (reader.TryGetInt64(out long longValue))
                        return longValue.ToString();
                    if (reader.TryGetDouble(out double doubleValue))
                        return doubleValue.ToString();
                    return reader.GetDecimal().ToString();

                case JsonTokenType.True:
                    return "true";

                case JsonTokenType.False:
                    return "false";

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }
}
