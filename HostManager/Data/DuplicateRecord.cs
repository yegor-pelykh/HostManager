using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace HostManager.Data
{
    internal class DuplicateRecord : BindableBase
    {
        internal DuplicateRecord(string host, List<IPAddress> addresses)
        {
            Host = host;
            Addresses = addresses;
            SelectedAddress = addresses.FirstOrDefault();
        }

        #region Properties
        public string Host { get; set; }

        public List<IPAddress> Addresses
        {
            get => _addresses;
            set => SetProperty(ref _addresses, value);
        }

        public IPAddress SelectedAddress
        {
            get => _selectedAddress;
            set => SetProperty(ref _selectedAddress, value);
        }
        #endregion

        #region Methods
        public override string ToString() => $"{Host}: {string.Join(", ", Addresses)}";
        #endregion

        #region Fields
        private List<IPAddress> _addresses;
        private IPAddress _selectedAddress;
        #endregion

    }

}
