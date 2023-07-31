using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManager.JsonConverter
{
    internal class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) => writer.WriteStringValue(dateTimeValue.ToString(CultureInfo.InvariantCulture));

    }
}
