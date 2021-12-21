using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HostManager.Extensions
{
    internal static class CollectionHelper
    {
        #region Methods
        internal static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToList();
            foreach (var itemToRemove in itemsToRemove)
                collection.Remove(itemToRemove);
            return itemsToRemove.Count;
        }

        internal static void Sort<T>(this ObservableCollection<T> collection, IComparer<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
                collection.Move(collection.IndexOf(sortableList[i]), i);
        }
        #endregion

    }

}
