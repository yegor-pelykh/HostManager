using DnsClient;
using DnsClient.Protocol;
using HostManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HostManager.Services
{
    internal class DnsResolver
    {
        #region Properties
        private string HostsFilePath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
        #endregion

        #region Methods
        internal async Task<List<ARecord>> ResolveIpAddressAsync(DnsServer server, string hostName)
        {
            var serverEndpoints = GetServerAddresses(server);
            if (serverEndpoints.Length == 0)
                throw new Exception("Unknown server for the request.");

            foreach (var endpoint in serverEndpoints)
            {
                var client = new LookupClient(endpoint);
                var result = await client.QueryAsync(hostName, QueryType.A);
                var answers = result.Answers
                    .OfType<ARecord>()
                    .ToList();
                if (answers.Count > 0)
                    return answers;
            }

            throw new Exception("The address cannot be resolved using this DNS server.");
        }

        private IPAddress[] GetServerAddresses(DnsServer server)
        {
            switch (server)
            {
                case DnsServer.Google: return new IPAddress[]
                {
                    IPAddress.Parse("8.8.8.8"),
                    IPAddress.Parse("8.8.4.4"),
                    IPAddress.Parse("2001:4860:4860::8888"),
                    IPAddress.Parse("2001:4860:4860::8844"),
                };
                case DnsServer.Quad9: return new IPAddress[]
                {
                    IPAddress.Parse("9.9.9.9"),
                    IPAddress.Parse("149.112.112.112"),
                    IPAddress.Parse("2620:fe::fe"),
                    IPAddress.Parse("2620:fe::9"),
                };
                case DnsServer.OpenDNS: return new IPAddress[]
                {
                    IPAddress.Parse("208.67.222.222"),
                    IPAddress.Parse("208.67.220.220"),
                    IPAddress.Parse("2620:119:35::35"),
                    IPAddress.Parse("2620:119:53::53"),
                };
                case DnsServer.Cloudflare: return new IPAddress[]
                {
                    IPAddress.Parse("1.1.1.1"),
                    IPAddress.Parse("1.0.0.1"),
                    IPAddress.Parse("2606:4700:4700::1111"),
                    IPAddress.Parse("2606:4700:4700::1001"),
                };
                default: return new IPAddress[] { };
            }
        }
        #endregion

        #region Fields
        #endregion

    }

}
