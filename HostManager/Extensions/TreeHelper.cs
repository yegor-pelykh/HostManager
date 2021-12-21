using System.Windows;
using System.Windows.Media;

namespace HostManager.Extensions
{
    internal static class TreeHelper
    {
        internal static T GetParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            return parentObject.GetType().IsAssignableFrom(typeof(T))
                ? parentObject as T
                : GetParent<T>(parentObject);
        }

        internal static T GetChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = (child as T) ?? GetChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

    }

}
