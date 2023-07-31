using HostManager.Configuration;
using HostManager.Data;
using HostManager.Extensions;
using HostManager.Services;
using HostManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace HostManager.ViewModels
{
    internal class ShellWindowViewModel : BindableBase
    {
        public ShellWindowViewModel(
            IDialogService dialogService,
            FileSystemService fileSystemService,
            DnsResolverService dnsResolverService,
            AsnProviderService asnProviderService)
        {
            DnsResolverService = dnsResolverService;
            AsnProviderService = asnProviderService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            InitializeCommands();
            UpdateControlStates();
            Reload();
        }

        #region Commands
        public ICommand CommandReload { get; private set; }
        public ICommand CommandSave { get; private set; }
        public ICommand CommandAddRecord { get; private set; }
        public ICommand CommandEditRecord { get; private set; }
        public ICommand CommandRemoveRecord { get; private set; }
        public ICommand CommandAddMultipleRecords { get; private set; }
        public ICommand CommandUpdateAllHosts { get; private set; }
        public ICommand CommandCreateRoutesList { get; private set; }
        public ICommand CommandOpenSettings { get; private set; }
        #endregion

        #region Properties
        private DnsResolverService DnsResolverService { get; }
        
        private AsnProviderService AsnProviderService { get; }

        public ObservableCollection<HostRecord> Hosts
        {
            get => _hosts;
            set => SetProperty(ref _hosts, value);
        }

        public HostRecord SelectedHost
        {
            get => _selectedHost;
            set
            {
                if (SetProperty(ref _selectedHost, value))
                    UpdateControlStates();
            }
        }

        public bool CanEditRecord
        {
            get => _canEditRecord;
            set => SetProperty(ref _canEditRecord, value);
        }

        public bool CanRemoveRecord
        {
            get => _canRemoveRecord;
            set => SetProperty(ref _canRemoveRecord, value);
        }

        public string StatusBarText
        {
            get => _statusBarText;
            set => SetProperty(ref _statusBarText, value);
        }
        #endregion

        #region Methods
        private void InitializeCommands()
        {
            CommandReload = new DelegateCommand(Reload);
            CommandSave = new DelegateCommand(Save);
            CommandAddRecord = new DelegateCommand(AddRecord);
            CommandEditRecord = new DelegateCommand<object>(EditRecord);
            CommandRemoveRecord = new DelegateCommand(RemoveRecord);
            CommandAddMultipleRecords = new DelegateCommand(AddMultipleRecords);
            CommandUpdateAllHosts = new DelegateCommand(UpdateAllHosts);
            CommandCreateRoutesList = new DelegateCommand(CreateRoutesList);
            CommandOpenSettings = new DelegateCommand(OpenSettings);
        }

        private void UpdateControlStates()
        {
            CanEditRecord = SelectedHost != null;
            CanRemoveRecord = SelectedHost != null;
        }

        private void Reload()
        {
            if (Hosts == null)
                Hosts = new ObservableCollection<HostRecord>();
            else
                Hosts.Clear();

            try
            {
                var hosts = _fileSystemService.LoadHosts();
                if (hosts != null)
                {
                    Hosts.AddRange(hosts);
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));
                }
                
                RemoveDuplicates(Hosts);
            }
            catch
            {
                MessageBox.Show(
                    string.Format(L10n.Localization.GetLocalized("String.MsgLoadingRecordsFailed")),
                    L10n.Localization.GetLocalized("String.MsgCaptionLoadingRecordsFailed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            if (Hosts == null)
                return;

            try
            {
                _fileSystemService.SaveHosts(Hosts);
            }
            catch
            {
                MessageBox.Show(
                    string.Format(L10n.Localization.GetLocalized("String.MsgSavingRecordsFailed")),
                    L10n.Localization.GetLocalized("String.MsgCaptionSavingRecordsFailed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddRecord()
        {
            var parameters = new DialogParameters
            {
                { EditRecordDialogViewModel.InputDpMode, EditRecordDialogViewModel.DialogMode.Add }
            };
            _dialogService.ShowDialog(nameof(EditRecordDialog), parameters, result =>
            {
                var hostRecord = result.Parameters.GetValue<HostRecord>(EditRecordDialogViewModel.OutputDpRecord);
                if (hostRecord != null)
                {
                    Hosts.Add(hostRecord);
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));

                    SelectedHost = hostRecord;

                    RemoveDuplicates(Hosts);
                }
            });
        }

        private void EditRecord(object parameter)
        {
            var editableRecord = parameter is HostRecord
                ? (HostRecord)parameter
                : SelectedHost;
            
            if (editableRecord == null)
                return;

            var parameters = new DialogParameters
            {
                { EditRecordDialogViewModel.InputDpMode, EditRecordDialogViewModel.DialogMode.Edit },
                { EditRecordDialogViewModel.InputDpRecord, editableRecord }
            };
            _dialogService.ShowDialog(nameof(EditRecordDialog), parameters, result =>
            {
                var hostRecord = result.Parameters.GetValue<HostRecord>(EditRecordDialogViewModel.OutputDpRecord);
                if (hostRecord != null)
                {
                    editableRecord.Host = hostRecord.Host;
                    editableRecord.Address = hostRecord.Address;
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));

                    RemoveDuplicates(Hosts);
                }
            });
        }

        private void RemoveRecord()
        {
            if (SelectedHost != null)
            {
                if (MessageBox.Show(
                    string.Format(L10n.Localization.GetLocalized("String.MsgRemoveRecord"), Environment.NewLine, SelectedHost),
                    L10n.Localization.GetLocalized("String.MsgCaptionRemoveRecord"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Hosts.Remove(SelectedHost);
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));
                }
            }
        }

        private void AddMultipleRecords()
        {
            _dialogService.ShowDialog(nameof(AddMultipleRecordsDialog), null, result =>
            {
                var records = result.Parameters.GetValue<List<HostRecord>>(AddMultipleRecordsDialogViewModel.OutputDpRecords);
                if (records != null)
                {
                    Hosts.AddRange(records);
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));
                }

                RemoveDuplicates(Hosts);
            });
        }

        private async void UpdateAllHosts()
        {
            if (Hosts == null)
                return;

            if (MessageBox.Show(
                    L10n.Localization.GetLocalized("String.MsgUpdateAllHostsWarning")
                        .Replace("\n", Environment.NewLine),
                    L10n.Localization.GetLocalized("String.MsgCaptionUpdateAllHosts"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            var server = Settings.Default.DnsServer;
            var hostNames = Hosts
                .Select(h => h.Host)
                .ToArray();

            var progressMessageMask = L10n.Localization.GetLocalized("String.MsgUpdateAllHostsProgress");
            var progress = new Progress<Tuple<int, int>>(data  =>
            {
                StatusBarText = string.Format(progressMessageMask, data.Item1 + 1, hostNames.Length, data.Item2);
            });
            var records = await DnsResolverService.ResolveMultipleIpAsync(server, hostNames, progress);
            var failedHostsCount = hostNames.Count(h => !records.ContainsKey(h));

            for (var i = Hosts.Count - 1; i >= 0; i--)
            {
                var record = Hosts[i];
                if (!records.TryGetValue(record.Host, out var address) || address.Count <= 0)
                {
                    Hosts.RemoveAt(i);
                    continue;
                }
                var a = address[0].Address;
                if (!Equals(a, IPAddress.None))
                    record.Address = a;
            }

            StatusBarText = null;
            
            if (failedHostsCount > 0)
            {
                MessageBox.Show(
                    string.Format(L10n.Localization.GetLocalized("String.MsgUpdateAllHostsFinishedWithErrors"), failedHostsCount),
                    L10n.Localization.GetLocalized("String.MsgCaptionUpdateAllHosts"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    L10n.Localization.GetLocalized("String.MsgUpdateAllHostsFinished"),
                    L10n.Localization.GetLocalized("String.MsgCaptionUpdateAllHosts"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void CreateRoutesList()
        {
            if (Hosts == null)
                return;

            if (MessageBox.Show(
                    L10n.Localization.GetLocalized("String.MsgCreateRoutesListWarning")
                        .Replace("\n", Environment.NewLine),
                    L10n.Localization.GetLocalized("String.MsgCaptionCreateRoutesList"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            StatusBarText = L10n.Localization.GetLocalized("String.MsgCreateRoutesListLoadingDatabase");

            const string databaseFileName = "fullASN.json";
            var jsonString = await AsnProviderService.DownloadFileAsync($"https://ipapi.is/data/{databaseFileName}.zip", databaseFileName);
            if (jsonString == null)
                return;

            StatusBarText = L10n.Localization.GetLocalized("String.MsgCreateRoutesListProcessingDatabase");

            var asnRecords = await AsnProviderService.GetAsnRecordsAsync(jsonString);
            if (asnRecords == null)
                return;

            StatusBarText = L10n.Localization.GetLocalized("String.MsgCreateRoutesListCreatingList");

            SortedSet<HostRecord> failedHosts = null;
            var progress = new Progress<Tuple<int, int, SortedSet<HostRecord>>>(data =>
            {
                failedHosts = data.Item3;
                StatusBarText = string.Format(
                    L10n.Localization.GetLocalized("String.MsgCreateRoutesListSearchingSubnets"),
                    data.Item1 + 1, Hosts.Count, data.Item2);
            });
            var networks = await AsnProviderService.GetNetworksAsync(Hosts, asnRecords, progress);
            if (networks == null)
                return;

            StatusBarText = null;

            var parameters = new DialogParameters
            {
                { RoutesListDialogViewModel.InputDpNetworks, networks },
                { RoutesListDialogViewModel.InputDpFailedHosts, failedHosts },
            };
            _dialogService.ShowDialog(nameof(RoutesListDialog), parameters, null);
        }

        private void OpenSettings()
        {
            _dialogService.ShowDialog(nameof(ConfigurationDialog));
        }

        private int RemoveDuplicates(ObservableCollection<HostRecord> hosts)
        {
            var removedItemsCount = 0;

            var duplicates = hosts
                .GroupBy(r => r.Host.ToString())
                .Where(g => g.Count() > 1)
                .Select(g => new DuplicateRecord(g.Key, g.Select(r => r.Address).ToList()))
                .ToList();

            if (duplicates.Count > 0)
            {
                var parameters = new DialogParameters
                {
                    { DuplicatesDialogViewModel.InputDpDuplicates, duplicates }
                };
                _dialogService.ShowDialog(nameof(DuplicatesDialog), parameters, result =>
                {
                    var records = result.Parameters.GetValue<List<DuplicateRecord>>(DuplicatesDialogViewModel.OutputDpDuplicates);
                    foreach (var record in records)
                    {
                        var isFirst = true;
                        var currentlyRemovedItemsCount = hosts.RemoveAll(r => {
                            var shouldBeChecked = r.Host == record.Host;
                            if (!shouldBeChecked)
                                return false;

                            if (r.Address != record.SelectedAddress)
                                return true;

                            if (!isFirst)
                                return true;
                            
                            isFirst = false;
                            return false;
                        });
                        removedItemsCount += currentlyRemovedItemsCount;
                    }
                });
            }

            return removedItemsCount;
        }
        #endregion

        #region Fields
        private readonly IDialogService _dialogService;
        private readonly Services.FileSystemService _fileSystemService;
        private ObservableCollection<HostRecord> _hosts;
        private HostRecord _selectedHost;
        private bool _canEditRecord;
        private bool _canRemoveRecord;
        private string _statusBarText;
        #endregion

    }

}
