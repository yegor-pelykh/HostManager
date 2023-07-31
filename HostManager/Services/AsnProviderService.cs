using System.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using HostManager.Data;

namespace HostManager.Services
{
    internal class AsnProviderService
    {
        #region Methods
        internal async Task<string> DownloadFileAsync(string uri, string fileName)
        {
            try
            {
                await using var ns = await _httpClient.GetStreamAsync(uri);
                using var zip = new ZipArchive(ns, ZipArchiveMode.Read);
                var file = zip.Entries.FirstOrDefault(e =>
                    string.Compare(e.Name, fileName, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (file != null)
                {
                    using var unzip = new StreamReader(file.Open());
                    var lines = new List<string>();
                    return await unzip.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            return null;
        }

        internal Task<List<AsnRecord>> GetAsnRecordsAsync(string jsonString)
        {
            return Task.Run(() =>
            {
                var records = new List<AsnRecord>();
                var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;
                foreach (var rootChild in root.EnumerateObject())
                {
                    try
                    {
                        var record = rootChild.Value.Deserialize<AsnRecord>();
                        records.Add(record);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return records;
            });
        }

        internal Task<Dictionary<string, SortedSet<IPNetwork>>> GetNetworksAsync(IList<HostRecord> hostRecords,
            List<AsnRecord> asnRecords, IProgress<Tuple<int, int, SortedSet<HostRecord>>> progress = null)
        {
            return Task.Run(() =>
            {
                var networks = new Dictionary<string, SortedSet<IPNetwork>>();
                var failedHosts = new SortedSet<HostRecord>(
                    new SortedSet<HostRecord>(new HostRecordComparer(nameof(HostRecord.Host),
                        ListSortDirection.Ascending)));
                var i = 0;
                foreach (var hostRecord in hostRecords)
                {
                    var network = FindNetwork(asnRecords, hostRecord.Address, out var asnRecord);
                    if (network != null)
                    {
                        var asnId = $"{asnRecord.OrgName} ({asnRecord.CountryCode})";
                        if (networks.TryGetValue(asnId, out var existingNetworks))
                            existingNetworks.Add(network);
                        else
                            networks.Add(asnId, new SortedSet<IPNetwork>
                            {
                                network
                            });
                    }
                    else
                        failedHosts.Add(hostRecord);

                    progress?.Report(new Tuple<int, int, SortedSet<HostRecord>>(i++, networks.Count, failedHosts));
                }
                
                return networks;
            });
        }

        internal static IPNetwork FindNetwork(List<AsnRecord> asnRecords, IPAddress address, out AsnRecord record)
        {
            record = null;
            foreach (var asnRecord in asnRecords)
            {
                var networks = address.AddressFamily == AddressFamily.InterNetworkV6
                    ? asnRecord.NetworksV6
                    : asnRecord.NetworksV4;
                if (networks == null)
                    continue;

                var network = networks.FirstOrDefault(n => n.Contains(address));
                if (network == null)
                    continue;

                record = asnRecord;
                return network;
            }
            return null;
        }
        #endregion

        #region Fields
        private readonly HttpClient _httpClient = new();
        #endregion

    }

}
