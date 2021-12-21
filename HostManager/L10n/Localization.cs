using System;
using System.Windows;

namespace HostManager.L10n
{
    internal static class Localization
    {
        #region Properties
        public static string LanguageCode
        {
            get => _languageCode;
            set
            {
                var code = value.ToUpper();
                if (SetLanguageCode(code))
                    _languageCode = code;
            }
        }
        #endregion

        #region Methods
        public static string GetLocalized(this string key, string defaultValue = "") =>
            Application.Current.TryFindResource(key) as string ?? defaultValue;

        private static bool SetLanguageCode(string languageCode) =>
            languageCode != null && TryLoadDictionary(languageCode);

        private static bool TryLoadDictionary(string languageCode)
        {
            if (_loadedDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_loadedDictionary);
                _loadedDictionary = null;
            }

            try
            {
                var dictionary = new ResourceDictionary()
                {
                    Source = new Uri($"pack://application:,,,/L10n/Strings/{languageCode}.xaml", UriKind.Absolute)
                };
                Application.Current.Resources.MergedDictionaries.Insert(0, dictionary);
                _loadedDictionary = dictionary;

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Fields
        private static string _languageCode;
        private static ResourceDictionary _loadedDictionary;
        #endregion

    }

}
