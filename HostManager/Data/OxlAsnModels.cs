using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace HostManager.Data
{
    public class OxlAsnDatabase : Dictionary<string, AsnJsonEntry>
    {
    }

    public class AsnJsonEntry
    {
        [JsonPropertyName("info")]
        public AsnInfo Info { get; set; }

        [JsonPropertyName("ipv4")]
        public List<string> Ipv4 { get; set; } = new List<string>();

        [JsonPropertyName("ipv6")]
        public List<string> Ipv6 { get; set; } = new List<string>();

        [JsonPropertyName("organization")]
        public AsnOrganization Organization { get; set; }
    }

    public class AsnInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }

    public class AsnOrganization
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    internal class IpAsnEntry
    {
        public IPNetwork2 Network { get; set; }
        public long AsnNumber { get; set; }
        public string OrgName { get; set; }
        public string CountryCode { get; set; }

        public class NetworkComparer : IComparer<IpAsnEntry>
        {
            public int Compare(IpAsnEntry x, IpAsnEntry y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return IPAddressComparer.Instance.Compare(x.Network.FirstUsable, y.Network.FirstUsable);
            }
        }
    }

    public class IPAddressComparer : IComparer<IPAddress>
    {
        public static readonly IPAddressComparer Instance = new IPAddressComparer();

        public int Compare(IPAddress x, IPAddress y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x.AddressFamily != y.AddressFamily)
            {
                return x.AddressFamily.CompareTo(y.AddressFamily);
            }

            byte[] xBytes = x.GetAddressBytes();
            byte[] yBytes = y.GetAddressBytes();

            for (int i = 0; i < xBytes.Length; i++)
            {
                int byteComparison = xBytes[i].CompareTo(yBytes[i]);
                if (byteComparison != 0)
                {
                    return byteComparison;
                }
            }
            return 0;
        }
    }
}