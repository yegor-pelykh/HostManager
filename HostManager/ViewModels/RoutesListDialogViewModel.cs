using HostManager.L10n;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Input;
using HostManager.Data;

namespace HostManager.ViewModels
{
    public enum RoutesFormat
    {
        NetworkCidr,
        NetworkMask,
        OpenVpn
    }

    internal class RoutesListDialogViewModel : BindableBase, IDialogAware
    {
        internal RoutesListDialogViewModel()
        {
            RoutesFormat = RoutesFormat.NetworkCidr;
            UpdateFormatProperties();
            UpdateRoutesListTextContents();
            UpdateFailedHostsTextContents();
            InitializeCommands();
        }

        #region Events
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Commands
        public ICommand CommandClose { get; private set; }
        #endregion

        #region Properties
        public string Title => "String.RoutesListDialogTitle".GetLocalized();

        public Dictionary<string, SortedSet<IPNetwork>> Networks
        {
            get => _networks;
            private set
            {
                if (SetProperty(ref _networks, value))
                {
                    UpdateFormatProperties();
                    UpdateRoutesListTextContents();
                }
            }
        }

        public SortedSet<HostRecord> FailedHosts
        {
            get => _failedHosts;
            private set
            {
                if (SetProperty(ref _failedHosts, value))
                    UpdateFailedHostsTextContents();
            }
        }

        public RoutesFormat RoutesFormat
        {
            get => _routesFormat;
            set
            {
                if (SetProperty(ref _routesFormat, value))
                {
                    UpdateFormatProperties();
                    UpdateRoutesListTextContents();
                }
            }
        }

        public bool IsFormatNetworkCidr
        {
            get => _isFormatNetworkCidr;
            set
            {
                if (SetProperty(ref _isFormatNetworkCidr, value) && _isFormatNetworkCidr)
                    RoutesFormat = RoutesFormat.NetworkCidr;
            }
        }

        public bool IsFormatNetworkMask
        {
            get => _isFormatNetworkMask;
            set
            {
                if (SetProperty(ref _isFormatNetworkMask, value) && _isFormatNetworkMask)
                    RoutesFormat = RoutesFormat.NetworkMask;
            }
        }

        public bool IsFormatOpenVpn
        {
            get => _isFormatOpenVpn;
            set
            {
                if (SetProperty(ref _isFormatOpenVpn, value) && _isFormatOpenVpn)
                    RoutesFormat = RoutesFormat.OpenVpn;
            }
        }

        public string RoutesListTextContents
        {
            get => _routesListTextContents;
            set => SetProperty(ref _routesListTextContents, value);
        }

        public string FailedHostsTextContents
        {
            get => _failedHostsTextContents;
            set => SetProperty(ref _failedHostsTextContents, value);
        }
        #endregion

        #region Methods
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Networks = parameters.GetValue<Dictionary<string, SortedSet<IPNetwork>>>(InputDpNetworks);
            FailedHosts = parameters.GetValue<SortedSet<HostRecord>>(InputDpFailedHosts);
        }

        private void InitializeCommands()
        {
            CommandClose = new DelegateCommand(CloseDialog);
        }

        private void UpdateFormatProperties()
        {
            switch (RoutesFormat)
            {
                case RoutesFormat.NetworkCidr:
                    IsFormatNetworkCidr = true;
                    IsFormatNetworkMask = false;
                    IsFormatOpenVpn = false;
                    break;
                case RoutesFormat.NetworkMask:
                    IsFormatNetworkCidr = false;
                    IsFormatNetworkMask = true;
                    IsFormatOpenVpn = false;
                    break;
                case RoutesFormat.OpenVpn:
                    IsFormatNetworkCidr = false;
                    IsFormatNetworkMask = false;
                    IsFormatOpenVpn = true;
                    break;
                default:
                    break;
            }
        }

        private void UpdateRoutesListTextContents()
        {
            switch (RoutesFormat)
            {
                case RoutesFormat.NetworkCidr:
                    RoutesListTextContents = GetTextContentsNetworkCidr();
                    break;
                case RoutesFormat.NetworkMask:
                    RoutesListTextContents = GetTextContentsNetworkMask();
                    break;
                case RoutesFormat.OpenVpn:
                    RoutesListTextContents = GetTextContentsOpenVpn();
                    break;
                default:
                    break;
            }
        }

        private void UpdateFailedHostsTextContents()
        {
            if (FailedHosts != null)
            {
                var sb = new StringBuilder();
                foreach (var failedHost in FailedHosts)
                    sb.AppendLine($"{failedHost.Address}\t{failedHost.Host}");

                FailedHostsTextContents = sb.ToString();
            }
            else
            {
                FailedHostsTextContents = null;
            }
        }

        private string GetTextContentsNetworkCidr()
        {
            if (Networks == null)
                return null;

            var sb = new StringBuilder();
            foreach (var netGroupPair in Networks)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.AppendLine($"# {netGroupPair.Key}");
                foreach (var network in netGroupPair.Value)
                    sb.AppendLine($"{network.Network}/{network.Cidr}");
            }
            
            return sb.ToString();
        }

        private string GetTextContentsNetworkMask()
        {
            if (Networks == null)
                return null;

            var sb = new StringBuilder();
            foreach (var netGroupPair in Networks)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.AppendLine($"# {netGroupPair.Key}");
                foreach (var network in netGroupPair.Value)
                    sb.AppendLine($"{network.Network}/{network.Netmask}");
            }

            return sb.ToString();
        }

        private string GetTextContentsOpenVpn()
        {
            if (Networks == null)
                return null;

            var sb = new StringBuilder();
            foreach (var netGroupPair in Networks)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.AppendLine($"# {netGroupPair.Key}");
                foreach (var network in netGroupPair.Value)
                    sb.AppendLine($"push \"route {network.Network} {network.Netmask}\"");
            }
            
            return sb.ToString();
        }

        private void CloseDialog()
        {
            RequestClose?.Invoke(null);
        }
        #endregion

        #region Fields
        private Dictionary<string, SortedSet<IPNetwork>> _networks;
        private SortedSet<HostRecord> _failedHosts;
        private RoutesFormat _routesFormat;
        private bool _isFormatNetworkCidr;
        private bool _isFormatNetworkMask;
        private bool _isFormatOpenVpn;
        private string _routesListTextContents;
        private string _failedHostsTextContents;
        #endregion

        #region Constants
        public const string InputDpNetworks = "Networks";
        public const string InputDpFailedHosts = "FailedHosts";
        #endregion

    }

}
