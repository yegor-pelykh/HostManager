using HostManager.Behaviors;
using HostManager.Data;
using HostManager.Extensions;
using HostManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HostManager.ViewModels
{
    internal class ShellWindowViewModel : BindableBase
    {
        public ShellWindowViewModel(
            IDialogService dialogService,
            Services.FileSystemService fileSystemService)
        {
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
        public ICommand CommandOpenSettings { get; private set; }
        #endregion

        #region Properties
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
        #endregion

        #region Methods
        private void InitializeCommands()
        {
            CommandReload = new DelegateCommand(Reload);
            CommandSave = new DelegateCommand(Save);
            CommandAddRecord = new DelegateCommand(AddRecord);
            CommandEditRecord = new DelegateCommand<object>(EditRecord);
            CommandRemoveRecord = new DelegateCommand(RemoveRecord);
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

            Hosts.Clear();

            try
            {
                var hosts = _fileSystemService.LoadHosts();
                if (hosts != null)
                {
                    Hosts.AddRange(hosts);
                    Hosts.Sort(new HostRecordComparer(nameof(HostRecord.Host), ListSortDirection.Ascending));
                }

                // Hosts.AddRange(Hosts.Take(10));

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
                { EditRecordDialogViewModel.InputDPMode, EditRecordDialogViewModel.DialogMode.Add }
            };
            _dialogService.ShowDialog(nameof(EditRecordDialog), parameters, result =>
            {
                var hostRecord = result.Parameters.GetValue<HostRecord>(EditRecordDialogViewModel.OutputDPRecord);
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
                { EditRecordDialogViewModel.InputDPMode, EditRecordDialogViewModel.DialogMode.Edit },
                { EditRecordDialogViewModel.InputDPRecord, editableRecord }
            };
            _dialogService.ShowDialog(nameof(EditRecordDialog), parameters, result =>
            {
                var hostRecord = result.Parameters.GetValue<HostRecord>(EditRecordDialogViewModel.OutputDPRecord);
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
                    { DuplicatesDialogViewModel.InputDPDuplicates, duplicates }
                };
                _dialogService.ShowDialog(nameof(DuplicatesDialog), parameters, result =>
                {
                    var records = result.Parameters.GetValue<List<DuplicateRecord>>(DuplicatesDialogViewModel.OutputDPDuplicates);
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
        private IDialogService _dialogService;
        private Services.FileSystemService _fileSystemService;
        private ObservableCollection<HostRecord> _hosts;
        private HostRecord _selectedHost;
        private bool _canEditRecord;
        private bool _canRemoveRecord;
        #endregion

    }

}
