using HostManager.Data;
using System;
using System.ComponentModel;
using System.Configuration;

namespace HostManager.Configuration
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        #region Properties
        [UserScopedSetting()]
        public DnsServer DnsServer
        {
            get
            {
                var v = this[nameof(DnsServer)];
                return v != null
                    ? (DnsServer)this[nameof(DnsServer)]
                    : DnsServer.Google;
            }
            set => this[nameof(DnsServer)] = value;
        }
        #endregion

        #region Methods
        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            Save();
        }
        #endregion

        #region Static Properties
        public static Settings Default => _defaultInstance;
        #endregion

        #region Static Fields
        private static Settings _defaultInstance = (Settings)Synchronized(new Settings());
        #endregion

    }

}
