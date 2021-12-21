using HostManager.Data;
using HostManager.L10n;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace HostManager.ViewModels
{
    internal class DuplicatesDialogViewModel : BindableBase, IDialogAware
    {
        internal DuplicatesDialogViewModel()
        {
            _canClose = false;
            InitializeCommands();
        }

        #region Events
        public event Action<IDialogResult> RequestClose;
        #endregion

        #region Commands
        public ICommand CommandApply { get; private set; }
        #endregion

        #region Properties
        public string Title => Localization.GetLocalized("String.RemovingDuplicates");

        public List<DuplicateRecord> Duplicates
        {
            get => _duplicates;
            private set => SetProperty(ref _duplicates, value);
        }
        #endregion

        #region Methods
        public bool CanCloseDialog()
        {
            return _canClose;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Duplicates = parameters.GetValue<List<DuplicateRecord>>(InputDPDuplicates);
        }

        private void InitializeCommands()
        {
            CommandApply = new DelegateCommand(ApplyChanges);
        }

        private void ApplyChanges()
        {
            _canClose = true;

            var result = new DialogResult(ButtonResult.OK, new DialogParameters
            {
                { OutputDPDuplicates, Duplicates},
            });
            RequestClose.Invoke(result);
        }
        #endregion

        #region Fields
        private bool _canClose;
        private List<DuplicateRecord> _duplicates;
        #endregion

        #region Constants
        public const string InputDPDuplicates = "Duplicates";
        public const string OutputDPDuplicates = "Duplicates";
        #endregion

    }

}
