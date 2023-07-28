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
    internal class DnsResolverService
    {
        #region Methods
        internal async Task<List<ARecord>> ResolveIpAsync(DnsServer server, string hostName)
        {
            var serverEndpoints = GetServerAddresses(server);
            if (serverEndpoints.Length == 0)
                throw new Exception("Unknown server for the request.");

            var lookupClients = serverEndpoints.Select(e => new LookupClient(e));
            foreach (var client in lookupClients)
            {
                var result = await client.QueryAsync(hostName, QueryType.A);
                var answers = result.Answers
                    .OfType<ARecord>()
                    .ToList();
                if (answers.Count > 0)
                    return answers;
            }

            throw new Exception("The address cannot be resolved using this DNS server.");
        }

        internal async Task<Dictionary<string, List<ARecord>>> ResolveMultipleIpAsync(
            DnsServer server,
            IEnumerable<string> hostNames,
            IProgress<Tuple<int, int>> progress = null)
        {
            var serverEndpoints = GetServerAddresses(server);
            if (serverEndpoints.Length == 0)
                throw new Exception("Unknown server for the request.");

            var lookupClients = serverEndpoints
                .Select(e => new LookupClient(e))
                .ToArray();
            var currentProgress = 0;
            var failedHosts = 0;
            var results = new Dictionary<string, List<ARecord>>();
            foreach (var hostName in hostNames)
            {
                List<ARecord> records = null;
                foreach (var client in lookupClients)
                {
                    try
                    {
                        var result = await client.QueryAsync(hostName, QueryType.A);
                        var answers = result.Answers
                            .OfType<ARecord>()
                            .ToList();
                        if (answers.Count > 0)
                        {
                            records = answers;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }

                if (records != null)
                    results[hostName] = records;
                else
                    failedHosts++;

                progress?.Report(new Tuple<int, int>(currentProgress++, failedHosts));
            }

            return results;
        }

        private IPAddress[] GetServerAddresses(DnsServer server)
        {
            switch (server)
            {
                case DnsServer.Google: return new IPAddress[]
                {
                    IPAddress.Parse("8.8.8.8"),
                    IPAddress.Parse("8.8.4.4"),
                };
                case DnsServer.Quad9: return new IPAddress[]
                {
                    IPAddress.Parse("9.9.9.9"),
                    IPAddress.Parse("149.112.112.112"),
                };
                case DnsServer.OpenDNS: return new IPAddress[]
                {
                    IPAddress.Parse("208.67.222.222"),
                    IPAddress.Parse("208.67.220.220"),
                };
                case DnsServer.Cloudflare: return new IPAddress[]
                {
                    IPAddress.Parse("1.1.1.1"),
                    IPAddress.Parse("1.0.0.1"),
                };
                default: return new IPAddress[] { };
            }
        }
        #endregion
        
    }

}
