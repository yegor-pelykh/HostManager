using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostManager.JsonConverter
{
    internal class IpNetworkCollectionJsonConverter : JsonConverter<IEnumerable<IPNetwork>>
    {
        public override IEnumerable<IPNetwork> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            List<IPNetwork> items = new();

            if (reader.TokenType != JsonTokenType.StartArray)
                return items;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var str = reader.GetString();
                var network = IPNetwork.Parse(str);
                items.Add(network);
            }

            return items;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IEnumerable<IPNetwork> ipNetworkCollectionValue,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var network in ipNetworkCollectionValue)
            {
                var str = network.ToString();
                JsonSerializer.Serialize(writer, str, options);
            }

            writer.WriteEndArray();
        }
    }
}
