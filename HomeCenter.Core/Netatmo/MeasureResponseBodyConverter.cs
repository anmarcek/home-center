using System.Text.Json;
using System.Text.Json.Serialization;
using HomeCenter.Core.Netatmo.RestApiDto;

namespace HomeCenter.Core.Netatmo;

internal class GetMeasureResponseBodyConverter : JsonConverter<GetMeasureResponseBody>
{
    public override GetMeasureResponseBody Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new GetMeasureResponseBody();

        // Parse data that look like this: { "1761288641": [10.7, 84] }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (long.TryParse(prop.Name, out long key))
                    {
                        var values = new List<double>();
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            if (item.TryGetDouble(out double val))
                            {
                                values.Add(val);
                            }
                        }
                        result[key] = values;
                    }
                }
            }
        }
        // Parse empty data that look like this: []
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Skip();
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, GetMeasureResponseBody value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            writer.WriteStartArray();
            foreach (var val in kvp.Value)
            {
                writer.WriteNumberValue(val);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
}