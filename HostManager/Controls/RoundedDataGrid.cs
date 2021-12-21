using HostManager.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace HostManager.Controls
{
    internal class RoundedDataGrid : DataGrid
    {
        #region Dependency Properties
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(RoundedDataGrid));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var border = TreeHelper.GetChild<Border>(this);
            if (border != null)
                border.CornerRadius = CornerRadius;
        }
        #endregion

    }

}
