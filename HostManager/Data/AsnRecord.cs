using HostManager.JsonConverter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace HostManager.Data
{
    internal class AsnRecord
    {
        public AsnRecord() { }
        
        #region Properties
        [JsonPropertyName("asn")]
        [JsonConverter(typeof(LongJsonConverter))]
        public long AsnNumber { get; set; }
        [JsonPropertyName("org")]
        public string OrgName { get; set; }
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
        [JsonPropertyName("abuse")]
        [JsonConverter(typeof(StringOrStringCollectionJsonConverter))]
        public IEnumerable<string> Abuse { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("created")]
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime Created { get; set; }
        [JsonPropertyName("updated")]
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime Updated { get; set; }
        [JsonPropertyName("rir")]
        public string Rir { get; set; }
        [JsonPropertyName("descr")]
        public string Description { get; set; }
        [JsonPropertyName("country")]
        public string CountryCode { get; set; }
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("prefixes")]
        [JsonConverter(typeof(IpNetworkCollectionJsonConverter))]
        public IEnumerable<IPNetwork2> NetworksV4 { get; set; }
        [JsonPropertyName("prefixesIPv6")]
        [JsonConverter(typeof(IpNetworkCollectionJsonConverter))]
        public IEnumerable<IPNetwork2> NetworksV6 { get; set; }
        #endregion

        #region ToString() implementation
        public override string ToString() => $"{AsnNumber} - {OrgName} ({CountryCode})";
        #endregion

    }
}
