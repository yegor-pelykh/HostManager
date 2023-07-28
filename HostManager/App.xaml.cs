using HandyControl.Data;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System.Globalization;
using System.Windows;

namespace HostManager
{
    public partial class App : PrismApplication
    {
        #region Methods
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!SetLocalization())
            {
                MessageBox.Show(
                    MsgLocalizationFailed,
                    MsgErrorCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<Services.FileSystemService, Services.FileSystemService>();
            containerRegistry.RegisterSingleton<Services.DnsResolverService, Services.DnsResolverService>();

            containerRegistry.RegisterDialog<Views.DuplicatesDialog, ViewModels.DuplicatesDialogViewModel>();
            containerRegistry.RegisterDialog<Views.EditRecordDialog, ViewModels.EditRecordDialogViewModel>();
            containerRegistry.RegisterDialog<Views.AddMultipleRecordsDialog, ViewModels.AddMultipleRecordsDialogViewModel>();
            containerRegistry.RegisterDialog<Views.ConfigurationDialog, ViewModels.ConfigurationDialogViewModel>();

            ViewModelLocationProvider.Register(typeof(Views.ShellWindow).ToString(), typeof(ViewModels.ShellWindowViewModel));
        }

        protected override System.Windows.Window CreateShell() => Container.Resolve<Views.ShellWindow>();

        private bool SetLocalization()
        {
            var currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
            L10n.Localization.LanguageCode = currentLanguage;
            if (L10n.Localization.LanguageCode == currentLanguage)
                return true;

            L10n.Localization.LanguageCode = DefaultLanguageCode;
            if (L10n.Localization.LanguageCode == DefaultLanguageCode)
                return true;

            return false;
        }
        #endregion

        #region Constants
        private const string MsgLocalizationFailed = "An error occurred while preparing the localization.\nThe app will not load.";
        private const string MsgErrorCaption = "Error";
        private const string DefaultLanguageCode = "EN";
        #endregion

    }

}
