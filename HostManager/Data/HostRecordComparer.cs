using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace HostManager.Data
{
    internal class HostRecordComparer : IComparer<HostRecord>, IComparer
    {
        internal HostRecordComparer(string propertyName, ListSortDirection direction = ListSortDirection.Ascending)
        {
            PropertyName = propertyName;
            Direction = direction;
        }

        #region Properties
        internal string PropertyName { get; }
        internal ListSortDirection Direction { get; }
        #endregion

        #region Methods
        public int Compare(object x, object y)
        {
            var xHost = x as HostRecord;
            var yHost = y as HostRecord;

            return Compare(xHost, yHost);
        }

        public int Compare([AllowNull] HostRecord x, [AllowNull] HostRecord y)
        {
            var comparisonResult = x == null
                ? y == null ? 0 : -1
                : y == null ? 1 : CompareByProperty(x, y);

            if (Direction != ListSortDirection.Ascending && comparisonResult != 0)
                comparisonResult *= -1;

            return comparisonResult;
        }

        private int CompareByProperty(HostRecord x, HostRecord y)
        {
            switch (PropertyName)
            {
                case nameof(HostRecord.Host):
                    return CompareByHost(x, y);
                case nameof(HostRecord.Address):
                    return CompareByAddress(x, y);
                default:
                    return 0;
            }
        }

        public static int CompareByAddress(HostRecord x, HostRecord y)
        {
            var result = x.Address.AddressFamily.CompareTo(y.Address.AddressFamily);
            if (result != 0)
                return result;

            var xBytes = x.Address.GetAddressBytes();
            var yBytes = y.Address.GetAddressBytes();

            var octets = Math.Min(xBytes.Length, yBytes.Length);
            for (var i = 0; i < octets; i++)
            {
                var octetResult = xBytes[i].CompareTo(yBytes[i]);
                if (octetResult != 0)
                    return octetResult;
            }

            return 0;
        }

        public static int CompareByHost(HostRecord x, HostRecord y)
        {
            var xValue = x.Host.ToLower();
            var yValue = y.Host.ToLower();

            if (xValue == yValue)
                return 0;

            var mySubdomains = xValue.Split('.');
            var otherSubdomains = yValue.Split('.');
            var a = CompareDomains(mySubdomains, otherSubdomains, 0);
            return a;
        }

        private static int CompareDomains(string[] x, string[] y, int depth)
        {
            while (true)
            {
                var xOffset = x.Length - depth - 1;
                var yOffset = y.Length - depth - 1;

                if (xOffset >= 0 && yOffset < 0) return 1;

                if (yOffset >= 0 && xOffset < 0) return -1;

                var cmp = string.Compare(x[xOffset], y[yOffset], StringComparison.Ordinal);
                if (cmp == 0)
                {
                    depth += 1;
                    continue;
                }

                return cmp;
            }
        }
        #endregion

    }

}
