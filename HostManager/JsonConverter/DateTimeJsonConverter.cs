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
            JsonSerializerOptions options)
        {
            if (DateTime.TryParseExact(reader.GetString()!, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                return dt;
            if (DateTime.TryParseExact(reader.GetString()!, "yyyyMMdd",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
                return dt;
            if (DateTime.TryParse(reader.GetString()!, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out dt))
                return dt;
            return new DateTime();
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) => writer.WriteStringValue(dateTimeValue.ToString(CultureInfo.InvariantCulture));

    }
}
