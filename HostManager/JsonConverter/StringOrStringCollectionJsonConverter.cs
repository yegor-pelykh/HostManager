using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManager.JsonConverter
{
    internal class StringOrStringCollectionJsonConverter : JsonConverter<IEnumerable<string>>
    {
        public override IEnumerable<string> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            List<string> items = new();

            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;

                        var str = reader.GetString();
                        items.Add(str);
                    }
                    break;
                }
                case JsonTokenType.String:
                {
                    var str = reader.GetString();
                    items.Add(str);
                    break;
                }
            }
            return items;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IEnumerable<string> value,
            JsonSerializerOptions options)
        {
            var array = value.ToArray();
            if (array.Length > 1)
            {
                writer.WriteStartArray();

                foreach (var str in array)
                    JsonSerializer.Serialize(writer, str, options);

                writer.WriteEndArray();
            }
            else
            {
                var str = array.FirstOrDefault() ?? string.Empty;
                writer.WriteStringValue(str);
            }
        }
    }
}
