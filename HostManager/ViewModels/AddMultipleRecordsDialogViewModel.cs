using HostManager.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HostManager.Configuration;
using HostManager.Services;
using Localization = HostManager.L10n.Localization;

namespace HostManager.ViewModels
{
    public enum AddMultipleRecordsDialogState
    {
        EditMask,
        EditRecords,
    }

    internal class AddMultipleRecordsDialogViewModel : BindableBase, IDialogAware
    {
        public AddMultipleRecordsDialogViewModel(DnsResolverService dnsResolverService)
        {
            DnsResolverService = dnsResolverService;
            CurrentState = AddMultipleRecordsDialogState.EditMask;
            CounterFrom = 0;
            CounterTo = 9;
            InitializeCommands();
            UpdateCanCreateRecords();
        }

        #region Events
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Commands
        public ICommand CommandCreate { get; private set; }
        public ICommand CommandAdd { get; private set; }
        #endregion

        #region Properties
        public string Title => Localization.GetLocalized("String.AddingMultipleRecords");

        internal DnsResolverService DnsResolverService { get; }

        public AddMultipleRecordsDialogState CurrentState
        {
            get => _currentState;
            private set => SetProperty(ref _currentState, value);
        }

        public bool CanCreateRecords
        {
            get => _canCreateRecords;
            set => SetProperty(ref _canCreateRecords, value);
        }

        public string HostnameTemplate
        {
            get => _hostnameTemplate;
            set
            {
                if (SetProperty(ref _hostnameTemplate, value))
                {
                    UpdateCanCreateRecords();
                }
            }
        }

        public int CounterFrom
        {
            get => _counterFrom;
            set => SetProperty(ref _counterFrom, value);
        }

        public int CounterTo
        {
            get => _counterTo;
            set => SetProperty(ref _counterTo, value);
        }

        public List<HostRecord> Records
        {
            get => _records;
            private set => SetProperty(ref _records, value);
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
        }

        private void InitializeCommands()
        {
            CommandCreate = new DelegateCommand(CreateRecords);
            CommandAdd = new DelegateCommand(AddRecords);
        }
        
        private async void CreateRecords()
        {
            if (!IsHostnameTemplateValid(HostnameTemplate))
                return;

            var template = HostnameTemplate.Replace("*", "{0}");

            var records = new List<HostRecord>();
            for (var i = CounterFrom; i <= CounterTo; i++)
            {
                var host = string.Format(template, i);
                var address = await ResolveAddressForHostNameAsync(host);
                if (!Equals(address, IPAddress.None))
                    records.Add(new HostRecord(address, host));
            }

            Records = records;
            CurrentState = AddMultipleRecordsDialogState.EditRecords;
        }

        private void AddRecords()
        {
            var records = Records
                .Where(record => record.Address != null)
                .ToList();
            var hasInvalidRecords = records.Count != Records.Count;

            var message = string.Format(L10n.Localization.GetLocalized("String.MsgAddingMultipleRecords"), records.Count);
            if (hasInvalidRecords)
            {
                message = L10n.Localization.GetLocalized("String.MsgAddingMultipleRecordsWarning") +
                          Environment.NewLine + Environment.NewLine + message;
            }

            if (MessageBox.Show(
                    message,
                    L10n.Localization.GetLocalized("String.MsgCaptionAddingMultipleRecords"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                var result = new DialogResult(ButtonResult.OK, new DialogParameters
                {
                    { OutputDPRecords, records},
                });
                RequestClose?.Invoke(result);
            }
        }

        private async Task<IPAddress> ResolveAddressForHostNameAsync(string hostName)
        {
            var server = Settings.Default.DnsServer;

            try
            {
                var addresses = await DnsResolverService.ResolveIpAsync(server, hostName);
                return addresses.Count > 0
                    ? addresses[0].Address
                    : IPAddress.None;
            }
            catch (Exception)
            {
                return IPAddress.None;
            }
        }

        private void UpdateCanCreateRecords()
        {
            CanCreateRecords = IsHostnameTemplateValid(HostnameTemplate);
        }

        private bool IsHostnameTemplateValid(string template)
        {
            return !string.IsNullOrWhiteSpace(template) && template.Count(c => c == '*') == 1;
        }
        #endregion

        #region Fields
        private bool _canCreateRecords;
        private string _hostnameTemplate;
        private int _counterFrom;
        private int _counterTo;
        private AddMultipleRecordsDialogState _currentState;
        private List<HostRecord> _records;
        #endregion

        #region Constants
        public const string OutputDPRecords = "Records";
        #endregion

    }

}
