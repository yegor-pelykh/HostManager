using HostManager.Data;
using HostManager.Extensions;
using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace HostManager.Behaviors
{
    class HostsDataGridBehavior : Behavior<DataGrid>
    {
        #region Dependency Properties
        public static readonly DependencyProperty RowDoubleClickCommandProperty = DependencyProperty.Register(
            nameof(RowDoubleClickCommand), typeof(ICommand), typeof(HostsDataGridBehavior));

        public ICommand RowDoubleClickCommand
        {
            get => (ICommand)GetValue(RowDoubleClickCommandProperty);
            set => SetValue(RowDoubleClickCommandProperty, value);
        }
        #endregion

        #region Methods
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Sorting += OnSorting;
            AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
            AssociatedObject.Sorting -= OnSorting;

            base.OnDetaching();
        }

        private void OnSorting(object sender, DataGridSortingEventArgs args)
        {
            var dataGrid = (DataGrid)sender;

            var lcView = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (lcView == null)
                return;

            args.Handled = true;

            var direction = args.Column.SortDirection != ListSortDirection.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            args.Column.SortDirection = direction;

            lcView.CustomSort = new HostRecordComparer(args.Column.SortMemberPath, direction);
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var row = (args.OriginalSource as DependencyObject).GetParent<DataGridRow>();
            if (row == null)
                return;

            var item = row.Item;
            if (RowDoubleClickCommand.CanExecute(item))
                RowDoubleClickCommand.Execute(item);
        }
        #endregion

    }

}