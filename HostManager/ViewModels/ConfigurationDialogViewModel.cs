using HostManager.Configuration;
using HostManager.Data;
using HostManager.L10n;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace HostManager.ViewModels
{
    internal class ConfigurationDialogViewModel : BindableBase, IDialogAware
    {
        internal ConfigurationDialogViewModel()
        {
            DnsServerList = Enum
                .GetValues(typeof(DnsServer))
                .Cast<DnsServer>()
                .ToList();
            InitializeCommands();
        }

        #region Events
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Commands
        public ICommand CommandClose { get; private set; }
        #endregion

        #region Properties
        public string Title => "String.ConfigSettings".GetLocalized();

        public List<DnsServer> DnsServerList
        {
            get => _dnsServerList;
            set => SetProperty(ref _dnsServerList, value);
        }

        public DnsServer SelectedDnsServer
        {
            get => Settings.Default.DnsServer;
            set
            {
                var current = Settings.Default.DnsServer;
                if (SetProperty(ref current, value))
                    Settings.Default.DnsServer = current;
            }
        }
        #endregion

        #region Methods
        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }

        private void InitializeCommands()
        {
            CommandClose = new DelegateCommand(Close);
        }

        private void Close()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        }
        #endregion

        #region Fields
        private List<DnsServer> _dnsServerList;
        #endregion

    }

}
