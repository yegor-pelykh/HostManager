using System.Linq;
using System.Net;

namespace HostManager.Data
{
    internal class HostRecord
    {
        internal HostRecord(IPAddress address, string host)
        {
            Address = address;
            Host = host;
        }

        #region Properties
        public IPAddress Address { get; set; }

        public string Host
        {
            get => _host;
            set
            {
                if (_host != value)
                {
                    _host = value;
                    UpdateGroup();
                }
            }
        }

        public string Group { get; set; }
        #endregion

        #region Methods
        public override string ToString() => $"{Host}: {Address}";
        
        public string ToHostsEntry() => $"{Address}\t\t\t{Host}";

        private void UpdateGroup()
        {
            var host = (string) Host;
            var subdomains = host.ToLower().Split('.');
            Group = subdomains.Length >= 2
                ? string.Join('.', subdomains.TakeLast(2))
                : null;
        }
        #endregion

        #region Fields
        private string _host;
        #endregion

    }

}
