using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManager.JsonConverter
{
    internal class LongJsonConverter: JsonConverter<long>
    {
        public override long Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => long.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            long longValue,
            JsonSerializerOptions options) => writer.WriteStringValue(longValue.ToString(CultureInfo.InvariantCulture));
    }
}
