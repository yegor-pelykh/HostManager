using HostManager.Configuration;
using HostManager.Data;
using HostManager.L10n;
using HostManager.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HostManager.ViewModels
{
    internal class EditRecordDialogViewModel : BindableBase, IDialogAware
    {
        internal EditRecordDialogViewModel(DnsResolverService dnsResolverService)
        {
            DnsResolverService = dnsResolverService;
            InitializeCommands();
            UpdateCanEditAddress();
        }
        
        #region Events
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Commands
        public ICommand CommandAutoResolve { get; private set; }
        public ICommand CommandApply { get; private set; }
        #endregion

        #region Properties
        private DnsResolverService DnsResolverService { get; }

        public string Title => Mode == DialogMode.Add
            ? L10n.Localization.GetLocalized("String.AddingRecord")
            : L10n.Localization.GetLocalized("String.EditingRecord");

        public DialogMode Mode
        {
            get => _mode;
            private set => SetProperty(ref _mode, value);
        }

        public string HostName
        {
            get => _hostName;
            set
            {
                if (SetProperty(ref _hostName, value))
                {
                    UpdateCanAutoResolve();
                    UpdateCanApply();
                }
            }
        }

        public IPAddress HostAddress
        {
            get => _hostAddress;
            set
            {
                if (SetProperty(ref _hostAddress, value))
                    UpdateCanApply();
            }
        }

        public bool CanEditAddress
        {
            get => _canEditAddress;
            set => SetProperty(ref _canEditAddress, value);
        }

        public bool CanAutoResolve
        {
            get => _canAutoResolve;
            set => SetProperty(ref _canAutoResolve, value);
        }

        public bool CanApply
        {
            get => _canApply;
            set => SetProperty(ref _canApply, value);
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
            Mode = parameters.GetValue<DialogMode>(InputDPMode);
            var record = parameters.GetValue<HostRecord>(InputDPRecord);
            if (record != null)
            {
                HostName = record.Host;
                HostAddress = record.Address;
            }
        }

        private void InitializeCommands()
        {
            CommandAutoResolve = new DelegateCommand(AutoResolve);
            CommandApply = new DelegateCommand(ApplyChanges);
        }

        private bool IsValidHostName(string hostName)
        {
            return HostNameCheckRegex.IsMatch(hostName);
        }

        private bool IsValidIPAddress(IPAddress ipAddress)
        {
            return ipAddress != null;
        }

        private async Task ResolveAddressForHostNameAsync(string hostName)
        {
            var server = Settings.Default.DnsServer;

            try
            {
                var addresses = await DnsResolverService.ResolveIpAsync(server, hostName);
                if (addresses.Count > 0)
                    HostAddress = addresses[0].Address;
            }
            catch (Exception)
            {
                var message = string.Format(L10n.Localization.GetLocalized("String.MsgAutoResolveFailed"), hostName);
                MessageBox.Show(
                    message,
                    L10n.Localization.GetLocalized("String.MsgCaptionAutoResolveFailed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateCanAutoResolve()
        {
            CanAutoResolve = IsValidHostName(HostName) &&
                !_isAutoResolveStarted;
        }

        private void UpdateCanEditAddress()
        {
            CanEditAddress = !_isAutoResolveStarted;
        }

        private void UpdateCanApply()
        {
            CanApply = IsValidHostName(HostName) &&
                IsValidIPAddress(HostAddress) &&
                !_isAutoResolveStarted;
        }

        private void AutoResolve()
        {
            _isAutoResolveStarted = true;
            UpdateCanEditAddress();
            UpdateCanAutoResolve();
            UpdateCanApply();

            ResolveAddressForHostNameAsync(HostName)
                .ContinueWith(_ =>
                {
                    _isAutoResolveStarted = false;
                    UpdateCanEditAddress();
                    UpdateCanAutoResolve();
                    UpdateCanApply();
                });
        }

        private void ApplyChanges()
        {
            var record = new HostRecord(HostAddress, HostName);
            var result = new DialogResult(ButtonResult.OK, new DialogParameters
            {
                { OutputDPRecord, record},
            });
            RequestClose.Invoke(result);
        }
        #endregion

        #region Fields
        private DialogMode _mode;
        private string _hostName;
        private IPAddress _hostAddress;
        private bool _canEditAddress;
        private bool _canAutoResolve;
        private bool _canApply;
        private bool _isAutoResolveStarted;
        #endregion

        #region Static Readonly Fields
        private static readonly Regex HostNameCheckRegex =
            new Regex(@"^(([\w][\w\-\.]*)\.)?([\w][\w\-]+)(\.([\w][\w\.]*))?$");
        #endregion

        #region Constants
        public const string InputDPMode = "Mode";
        public const string InputDPRecord = "Record";
        public const string OutputDPRecord = "Record";
        #endregion

        #region Types
        public enum DialogMode { Add, Edit }
        #endregion

    }

}
