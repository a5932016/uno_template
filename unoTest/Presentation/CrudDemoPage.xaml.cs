using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace unoTest.Presentation;

/// <summary>
/// CRUD 示範頁面
/// </summary>
public sealed partial class CrudDemoPage : Page
{
    public CrudDemoPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // 設置 XamlRoot，讓 ViewModel 中的 ContentDialog 可以正確顯示
        if (DataContext is CrudDemoViewModel vm && this.XamlRoot != null)
        {
            vm.SetXamlRoot(this.XamlRoot);
        }
    }

    private void OnSearchEnterKeyInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (DataContext is CrudDemoViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
        }
        args.Handled = true;
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CrudDemoViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
        }
    }

    private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CrudDemoViewModel vm)
        {
            vm.UpdateSelection(ProductListView.SelectedItems.Cast<ProductViewItem>().ToList());
        }
    }

    private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CrudDemoViewModel vm)
        {
            if (sender is CheckBox checkBox && checkBox.IsChecked == true)
            {
                ProductListView.SelectAll();
            }
            else
            {
                ProductListView.SelectedItems.Clear();
            }
        }
    }

    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CrudDemoViewModel vm && 
            PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int pageSize))
        {
            vm.ChangePageSize(pageSize);
        }
    }
}
