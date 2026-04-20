using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using unoTest.Services;
using System.Text;

namespace unoTest.Presentation;

/// <summary>
/// CRUD 示範頁面 ViewModel
/// </summary>
public partial class CrudDemoViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly INavigator? _navigator;
    private XamlRoot? _xamlRoot;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<ProductViewItem> _products = new();

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAllSelected;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private int _totalPages;

    #endregion

    #region Computed Properties

    public string[] Categories => new[] { "全部" }.Concat(ProductCategories.All).ToArray();

    public bool HasSelection => Products.Any(p => p.IsSelected);

    public bool IsEmpty => !IsLoading && Products.Count == 0;

    public bool CanGoPrevious => CurrentPage > 1;

    public bool CanGoNext => CurrentPage < TotalPages;

    public string PageInfo => TotalItems > 0 
        ? $"顯示 {(CurrentPage - 1) * PageSize + 1}-{Math.Min(CurrentPage * PageSize, TotalItems)} 共 {TotalItems} 筆" 
        : "沒有資料";

    public string PageDisplay => $"{CurrentPage} / {Math.Max(1, TotalPages)}";

    #endregion

    #region Commands

    public IAsyncRelayCommand SearchCommand { get; }
    public IRelayCommand ClearFilterCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand<ProductViewItem> EditCommand { get; }
    public IAsyncRelayCommand<ProductViewItem> DeleteCommand { get; }
    public IAsyncRelayCommand DeleteSelectedCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public IAsyncRelayCommand FirstPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand LastPageCommand { get; }

    #endregion

    private List<ProductViewItem> _selectedItems = new();

    public CrudDemoViewModel(
        IProductService? productService = null,
        INavigator? navigator = null)
    {
        _productService = productService ?? new MockProductService();
        _navigator = navigator;

        // 初始化 Commands
        SearchCommand = new AsyncRelayCommand(LoadDataAsync);
        ClearFilterCommand = new RelayCommand(ClearFilter);
        AddCommand = new AsyncRelayCommand(AddProductAsync);
        EditCommand = new AsyncRelayCommand<ProductViewItem>(EditProductAsync);
        DeleteCommand = new AsyncRelayCommand<ProductViewItem>(DeleteProductAsync);
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => HasSelection);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        FirstPageCommand = new AsyncRelayCommand(GoToFirstPageAsync, () => CanGoPrevious);
        PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, () => CanGoPrevious);
        NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, () => CanGoNext);
        LastPageCommand = new AsyncRelayCommand(GoToLastPageAsync, () => CanGoNext);

        // 初始載入
        _ = LoadDataAsync();
    }

    /// <summary>
    /// 設置 XamlRoot（必須在頁面載入後呼叫）
    /// </summary>
    public void SetXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    #region Data Operations

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var category = SelectedCategory == "全部" ? null : SelectedCategory;
            var result = await _productService.GetPagedAsync(
                CurrentPage, 
                PageSize, 
                SearchKeyword, 
                category);

            Products.Clear();
            foreach (var item in result.Items)
            {
                Products.Add(new ProductViewItem(item));
            }

            TotalItems = result.TotalCount;
            TotalPages = result.TotalPages;

            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(PageDisplay));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearFilter()
    {
        SearchKeyword = string.Empty;
        SelectedCategory = null;
        CurrentPage = 1;
        _ = LoadDataAsync();
    }

    private async Task AddProductAsync()
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "新增產品",
            PrimaryButtonText = "儲存",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var editForm = CreateEditForm(null);
        dialog.Content = editForm;

        // 驗證輸入
        dialog.PrimaryButtonClick += (s, e) =>
        {
            var nameTextBox = editForm.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "NameTextBox");
            if (string.IsNullOrWhiteSpace(nameTextBox?.Text))
            {
                e.Cancel = true;
                ShowValidationError(nameTextBox!, "產品名稱不可為空");
            }
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var newProduct = GetProductFromForm(editForm);
            await _productService.CreateAsync(newProduct);
            await LoadDataAsync();
        }
    }

    private async Task EditProductAsync(ProductViewItem? item)
    {
        if (item == null || _xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "編輯產品",
            PrimaryButtonText = "儲存",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot
        };

        var editForm = CreateEditForm(item.Product);
        dialog.Content = editForm;

        // 驗證輸入
        dialog.PrimaryButtonClick += (s, e) =>
        {
            var nameTextBox = editForm.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "NameTextBox");
            if (string.IsNullOrWhiteSpace(nameTextBox?.Text))
            {
                e.Cancel = true;
                ShowValidationError(nameTextBox!, "產品名稱不可為空");
            }
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var updatedProduct = GetProductFromForm(editForm, item.Product.Id);
            await _productService.UpdateAsync(updatedProduct);
            await LoadDataAsync();
        }
    }

    private async Task DeleteProductAsync(ProductViewItem? item)
    {
        if (item == null || _xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = "確認刪除",
            Content = $"確定要刪除「{item.Name}」嗎？此操作無法復原。",
            PrimaryButtonText = "刪除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await _productService.DeleteAsync(item.Id);
            await LoadDataAsync();
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_xamlRoot == null) return;

        var selectedIds = _selectedItems.Select(p => p.Id).ToList();
        if (!selectedIds.Any()) return;

        var dialog = new ContentDialog
        {
            Title = "確認批次刪除",
            Content = $"確定要刪除選中的 {selectedIds.Count} 個產品嗎？此操作無法復原。",
            PrimaryButtonText = "刪除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await _productService.DeleteManyAsync(selectedIds);
            await LoadDataAsync();
        }
    }

    private async Task ExportAsync()
    {
        if (_xamlRoot == null) return;

        try
        {
            // 獲取所有資料用於匯出
            var allData = await _productService.GetPagedAsync(1, int.MaxValue, SearchKeyword, 
                SelectedCategory == "全部" ? null : SelectedCategory);

            if (!allData.Items.Any())
            {
                await ShowMessageAsync("匯出", "沒有可匯出的資料。");
                return;
            }

            // 生成 CSV 內容
            var csv = new StringBuilder();
            csv.AppendLine("ID,產品名稱,描述,分類,價格,庫存,狀態,建立時間");

            foreach (var item in allData.Items)
            {
                csv.AppendLine($"{item.Id},\"{EscapeCsv(item.Name)}\",\"{EscapeCsv(item.Description ?? "")}\",\"{item.Category}\",{item.Price},{item.Stock},{(item.IsActive ? "啟用" : "停用")},{item.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            // 顯示匯出結果（在實際應用中應該使用 FileSavePicker）
            var previewDialog = new ContentDialog
            {
                Title = $"匯出成功 - 共 {allData.Items.Count()} 筆",
                Content = new ScrollViewer
                {
                    MaxHeight = 400,
                    Content = new TextBlock
                    {
                        Text = csv.ToString(),
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                        FontSize = 12,
                        IsTextSelectionEnabled = true
                    }
                },
                CloseButtonText = "關閉",
                XamlRoot = _xamlRoot
            };

            await previewDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("匯出失敗", $"發生錯誤：{ex.Message}");
        }
    }

    private string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "確定",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    private void ShowValidationError(Control control, string message)
    {
        // 設置錯誤提示
        ToolTipService.SetToolTip(control, message);
        
        // 視覺提示（可選：添加紅色邊框等）
        if (control is TextBox textBox)
        {
            textBox.Header = $"產品名稱 ⚠️ {message}";
        }
    }

    #endregion

    #region Pagination

    private async Task GoToFirstPageAsync()
    {
        CurrentPage = 1;
        await LoadDataAsync();
    }

    private async Task GoToPreviousPageAsync()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
            await LoadDataAsync();
        }
    }

    private async Task GoToNextPageAsync()
    {
        if (CanGoNext)
        {
            CurrentPage++;
            await LoadDataAsync();
        }
    }

    private async Task GoToLastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadDataAsync();
    }

    public void ChangePageSize(int newPageSize)
    {
        PageSize = newPageSize;
        CurrentPage = 1;
        _ = LoadDataAsync();
    }

    #endregion

    #region Selection

    public void UpdateSelection(List<ProductViewItem> selectedItems)
    {
        _selectedItems = selectedItems;
        OnPropertyChanged(nameof(HasSelection));
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Helper Methods

    private StackPanel CreateEditForm(Product? product)
    {
        var form = new StackPanel { Spacing = 12, MinWidth = 400 };

        form.Children.Add(new TextBox
        {
            Header = "產品名稱",
            PlaceholderText = "請輸入產品名稱",
            Text = product?.Name ?? "",
            Name = "NameTextBox"
        });

        form.Children.Add(new TextBox
        {
            Header = "描述",
            PlaceholderText = "請輸入產品描述（選填）",
            Text = product?.Description ?? "",
            Name = "DescriptionTextBox",
            AcceptsReturn = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Height = 80
        });

        var categoryComboBox = new ComboBox
        {
            Header = "分類",
            PlaceholderText = "選擇分類",
            ItemsSource = ProductCategories.All,
            Name = "CategoryComboBox"
        };
        if (product != null)
        {
            categoryComboBox.SelectedItem = product.Category;
        }
        form.Children.Add(categoryComboBox);

        form.Children.Add(new NumberBox
        {
            Header = "價格",
            PlaceholderText = "0.00",
            Value = (double)(product?.Price ?? 0),
            Name = "PriceNumberBox",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        });

        form.Children.Add(new NumberBox
        {
            Header = "庫存",
            PlaceholderText = "0",
            Value = product?.Stock ?? 0,
            Name = "StockNumberBox",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        });

        form.Children.Add(new ToggleSwitch
        {
            Header = "狀態",
            OnContent = "啟用",
            OffContent = "停用",
            IsOn = product?.IsActive ?? true,
            Name = "IsActiveToggle"
        });

        return form;
    }

    private Product GetProductFromForm(StackPanel form, int id = 0)
    {
        var nameTextBox = form.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "NameTextBox");
        var descTextBox = form.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "DescriptionTextBox");
        var categoryComboBox = form.Children.OfType<ComboBox>().FirstOrDefault(t => t.Name == "CategoryComboBox");
        var priceNumberBox = form.Children.OfType<NumberBox>().FirstOrDefault(t => t.Name == "PriceNumberBox");
        var stockNumberBox = form.Children.OfType<NumberBox>().FirstOrDefault(t => t.Name == "StockNumberBox");
        var isActiveToggle = form.Children.OfType<ToggleSwitch>().FirstOrDefault(t => t.Name == "IsActiveToggle");

        return new Product
        {
            Id = id,
            Name = nameTextBox?.Text ?? "",
            Description = descTextBox?.Text,
            Category = categoryComboBox?.SelectedItem?.ToString() ?? "",
            Price = (decimal)(priceNumberBox?.Value ?? 0),
            Stock = (int)(stockNumberBox?.Value ?? 0),
            IsActive = isActiveToggle?.IsOn ?? true
        };
    }

    #endregion
}

/// <summary>
/// 產品顯示項目（帶選擇狀態）
/// </summary>
public partial class ProductViewItem : ObservableObject
{
    public Product Product { get; }

    public ProductViewItem(Product product)
    {
        Product = product;
    }

    [ObservableProperty]
    private bool _isSelected;

    public int Id => Product.Id;
    public string Name => Product.Name;
    public string? Description => Product.Description;
    public string Category => Product.Category;
    public decimal Price => Product.Price;
    public int Stock => Product.Stock;
    public bool IsActive => Product.IsActive;
    public DateTime CreatedAt => Product.CreatedAt;
}
