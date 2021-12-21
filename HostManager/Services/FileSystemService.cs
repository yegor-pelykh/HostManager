using HostManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace HostManager.Services
{
    internal class FileSystemService
    {
        #region Properties
        private string HostsFilePath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
        #endregion

        #region Methods
        public IList<HostRecord> LoadHosts()
        {
            var lines = File.ReadAllLines(HostsFilePath);
            var hosts = new List<HostRecord>();
            foreach (var line in lines)
            {
                var records = ParseHostRecords(line);
                if (records != null)
                    hosts.AddRange(records);
            }

            return hosts;
        }

        public void SaveHosts(IList<HostRecord> hosts)
        {
            var lines = new List<string>();

            var minLength = hosts.Min(h => h.Address.ToString().Length);
            var maxLength = hosts.Max(h => h.Address.ToString().Length);

            var groups = hosts.GroupBy(h => h.Group);
            foreach (var group in groups)
            {
                if (lines.Count > 0)
                    lines.Add(string.Empty);

                lines.Add($"# {group.Key}");
                foreach (var record in group)
                    lines.Add(record.ToHostsEntry());
            }

            File.WriteAllLines(HostsFilePath, lines);
        }

        private IList<HostRecord> ParseHostRecords(string line)
        {
            var records = new List<HostRecord>();

            if (string.IsNullOrWhiteSpace(line))
                return records;

            IPAddress address = null;
            var hosts = new List<string>();

            var sections = line.Trim().Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var section in sections)
            {
                if (section.StartsWith('#'))
                    break;

                if (address == null)
                    address = IPAddress.Parse(section);
                else
                    hosts.Add(section.ToLower());
            }

            if (address != null)
            {
                foreach (var host in hosts)
                    records.Add(new HostRecord(address, host));
            }

            return records;
        }
        #endregion

    }

}
