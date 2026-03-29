using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using System.Net;
using HostManager.Data;
using System.Text.Json;

namespace HostManager.Services
{
    internal class AsnProviderService
    {
        #region Methods
        internal async Task<string> GetAsnDatabaseFilePathAsync()
        {
            string appRoot = AppDomain.CurrentDomain.BaseDirectory;
            string asnDirectory = Path.Combine(appRoot, "asn");
            Directory.CreateDirectory(asnDirectory);

            string zipFileName = "asn_ipv4_small.json.zip";
            string jsonFileName = "asn_ipv4_small.json";
            string zipFilePath = Path.Combine(asnDirectory, zipFileName);
            string jsonFilePath = Path.Combine(asnDirectory, jsonFileName);

            DateTime today = DateTime.Today;
            FileInfo zipFileInfo = new FileInfo(zipFilePath);
            FileInfo jsonFileInfo = new FileInfo(jsonFilePath);

            bool needsDownload = true;

            if (zipFileInfo.Exists && jsonFileInfo.Exists)
            {
                if (zipFileInfo.LastWriteTime.Date == today.Date)
                {
                    needsDownload = false;
                }
            }

            if (needsDownload)
            {
                try
                {
                    using (var response = await _httpClient.GetAsync("https://geoip.oxl.app/file/asn_ipv4_small.json.zip", HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }

                    await Task.Run(() =>
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                        {
                            var entry = archive.Entries.FirstOrDefault(e => e.Name.Equals(jsonFileName, StringComparison.OrdinalIgnoreCase));
                            if (entry != null)
                            {
                                entry.ExtractToFile(jsonFilePath, true);
                            }
                            else
                            {
                                throw new InvalidDataException($"JSON file '{jsonFileName}' not found in the downloaded zip archive.");
                            }
                        }
                    });
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (File.Exists(jsonFilePath))
            {
                return jsonFilePath;
            }
            return null;
        }

        private static List<IpAsnEntry> _allIpAsnEntriesCache;
        private static readonly object _cacheLock = new object();

        internal async Task<Dictionary<string, SortedSet<IPNetwork2>>> GetNetworksAsync(IList<HostRecord> hostRecords,
            string jsonFilePath, IProgress<Tuple<int, int, SortedSet<HostRecord>>> progress = null)
        {
            var networks = new Dictionary<string, SortedSet<IPNetwork2>>();
            var failedHosts = new SortedSet<HostRecord>(new HostRecordComparer(nameof(HostRecord.Host)));

            if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
            {
                int i = 0;
                foreach (var hostRecord in hostRecords)
                {
                    failedHosts.Add(hostRecord);
                    progress?.Report(new Tuple<int, int, SortedSet<HostRecord>>(i++, networks.Count, failedHosts));
                }
                return networks;
            }

            if (_allIpAsnEntriesCache == null)
            {
                List<IpAsnEntry> computedAsnEntries = null;
                computedAsnEntries = await Task.Run(async () =>
                {
                    var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                    var oxlAsnDatabase = JsonSerializer.Deserialize<OxlAsnDatabase>(jsonContent);

                    var ipAsnEntries = new List<IpAsnEntry>();
                    foreach (var asnEntryKvp in oxlAsnDatabase)
                    {
                        if (!long.TryParse(asnEntryKvp.Key, out long asnNumber))
                            continue;

                        var entry = asnEntryKvp.Value;
                        string orgName = entry.Organization?.Name ?? entry.Info?.Name ?? "Unknown Organization";
                        string countryCode = entry.Info?.Country ?? "ZZ";

                        foreach (var ipv4Cidr in entry.Ipv4)
                        {
                            if (IPNetwork2.TryParse(ipv4Cidr, out var network))
                            {
                                ipAsnEntries.Add(new IpAsnEntry
                                {
                                    Network = network,
                                    AsnNumber = asnNumber,
                                    OrgName = orgName,
                                    CountryCode = countryCode
                                });
                            }
                        }
                    }
                    ipAsnEntries.Sort(new IpAsnEntry.NetworkComparer());
                    return ipAsnEntries;
                });

                lock (_cacheLock)
                {
                    if (_allIpAsnEntriesCache == null)
                    {
                        _allIpAsnEntriesCache = computedAsnEntries;
                    }
                }
            }

            var resultTuple = await Task.Run(() =>
            {
                var localNetworks = new Dictionary<string, SortedSet<IPNetwork2>>();
                var localFailedHosts = new SortedSet<HostRecord>(new HostRecordComparer(nameof(HostRecord.Host)));
                int currentHostIndex = 0;

                foreach (var hostRecord in hostRecords)
                {
                    IpAsnEntry foundAsnEntry = null;

                    int approxIndex = _allIpAsnEntriesCache.BinarySearch(
                        new IpAsnEntry { Network = IPNetwork2.Parse($"{hostRecord.Address}/32") },
                        new IpAsnEntry.NetworkComparer());

                    if (approxIndex < 0)
                    {
                        approxIndex = ~approxIndex;
                    }

                    for (int k = Math.Min(approxIndex, _allIpAsnEntriesCache.Count - 1); k >= 0; k--)
                    {
                        var entry = _allIpAsnEntriesCache[k];
                        if (entry.Network.Contains(hostRecord.Address))
                        {
                            foundAsnEntry = entry;
                            break;
                        }
                    }

                    if (foundAsnEntry != null)
                    {
                        string asnId = $"{foundAsnEntry.AsnNumber} - {foundAsnEntry.OrgName} ({foundAsnEntry.CountryCode})";
                        if (localNetworks.TryGetValue(asnId, out var existingNetworks))
                            existingNetworks.Add(foundAsnEntry.Network);
                        else
                            localNetworks.Add(asnId, new SortedSet<IPNetwork2> { foundAsnEntry.Network });
                    }
                    else
                    {
                        localFailedHosts.Add(hostRecord);
                    }

                    progress?.Report(new Tuple<int, int, SortedSet<HostRecord>>(currentHostIndex++, localNetworks.Count, localFailedHosts));
                }
                return Tuple.Create(localNetworks, localFailedHosts);
            });

            networks = resultTuple.Item1;
            failedHosts = resultTuple.Item2;

            return networks;
        }
        #endregion

        #region Fields
        private readonly HttpClient _httpClient = new();
        #endregion

    }

}